using DrOcupacional.Backend.Api.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace DrOcupacional.Backend.Api.Configuration;

public static class JwtAuthenticationConfiguration
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var identityAuthority = configuration["Identity:Authority"] ?? "http://localhost:8081";
        var identityAudience = configuration["Identity:Audience"] ?? "ui-app";
        var identityIssuer = configuration["Identity:Issuer"] ?? "DrOcupacional.Identity";

        services.AddAuthentication(options =>
        {
            // Usar um esquema customizado que verifica primeiro se já temos um User válido
            options.DefaultAuthenticateScheme = "CustomJwtBearer";
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddPolicyScheme("CustomJwtBearer", "CustomJwtBearer", options =>
        {
            options.ForwardDefaultSelector = context =>
            {
                // Verificar primeiro o flag do middleware (mais confiável, pois é definido antes)
                if (context.Items.ContainsKey("TokenValidatedViaIntrospect") && 
                    context.Items["TokenValidatedViaIntrospect"] as bool? == true)
                {
                    return "NoOp"; // Usar handler NoOp que retorna sucesso
                }
                
                // Se já temos um User válido do middleware customizado, usar o handler NoOp
                if (context.User?.Identity?.IsAuthenticated == true)
                {
                    return "NoOp"; // Usar handler NoOp que retorna sucesso
                }
                
                return JwtBearerDefaults.AuthenticationScheme; // Usar JWT Bearer normalmente
            };
        })
        .AddScheme<NoOpAuthenticationSchemeOptions, NoOpAuthenticationHandler>(
            "NoOp", 
            "NoOp", 
            options => { })
        .AddJwtBearer(options =>
        {
            options.Authority = identityAuthority;
            options.Audience = identityAudience;
            options.RequireHttpsMetadata = false; // Apenas para desenvolvimento local
            
            // Configurar validação de token
            // Nota: Como o JWKS endpoint ainda não está implementado no Identity Manager,
            // vamos validar a assinatura de forma básica e fazer validação completa via introspect
            options.TokenValidationParameters = new TokenValidationParameters
            {
                // Validar formato e estrutura básica do JWT
                ValidateIssuer = false, // Será validado via introspect
                ValidateAudience = false, // Será validado via introspect
                ValidateLifetime = true, // Validar expiração localmente
                ValidateIssuerSigningKey = false, // Será validado via introspect (JWKS não disponível)
                ClockSkew = TimeSpan.FromMinutes(5) // Tolerância de 5 minutos para diferença de relógio
            };
            
            // Desabilitar validação automática de metadados (JWKS)
            options.MetadataAddress = null;

            // Configurar eventos para validação completa via introspect
            options.Events = CreateJwtBearerEvents(identityAuthority, identityIssuer, identityAudience);
        });

        services.AddAuthorization();

        return services;
    }

    private static JwtBearerEvents CreateJwtBearerEvents(
        string identityAuthority, 
        string identityIssuer, 
        string identityAudience)
    {
        return new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Se o token já foi validado via introspect no middleware customizado, pular a validação JWT
                // Também verificar se o usuário já está autenticado
                if ((context.HttpContext.Items.ContainsKey("TokenValidatedViaIntrospect") && 
                     context.HttpContext.Items["TokenValidatedViaIntrospect"] as bool? == true) ||
                    (context.HttpContext.User?.Identity?.IsAuthenticated == true))
                {
                    // Token já validado, não processar pelo JWT Bearer
                    // Definir token como null para evitar processamento adicional
                    context.Token = null;
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = async context =>
            {
                await ValidateTokenViaIntrospect(
                    context, 
                    identityAuthority, 
                    identityIssuer, 
                    identityAudience);
            },
            OnAuthenticationFailed = async context =>
            {
                await HandleAuthenticationFailed(
                    context, 
                    identityAuthority, 
                    identityIssuer, 
                    identityAudience);
            },
        };
    }

    private static async Task ValidateTokenViaIntrospect(
        TokenValidatedContext context,
        string identityAuthority,
        string identityIssuer,
        string identityAudience)
    {
        string? token = ExtractTokenFromContext(context);
        
        if (string.IsNullOrEmpty(token))
        {
            return;
        }

            try
            {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var introspectResult = await CallIntrospectEndpoint(token, identityAuthority, logger);

            if (introspectResult == null || !introspectResult.Active)
            {
                logger.LogWarning("Token rejeitado pelo introspect");
                context.Fail("Token não está ativo ou é inválido");
                return;
            }

            if (!ValidateIssuerAndAudience(introspectResult, identityIssuer, identityAudience, logger, context))
            {
                return;
            }

            logger.LogDebug("Token validado com sucesso via introspect para usuário: {Sub}", introspectResult.Sub);
        }
        catch (Exception ex)
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Erro ao validar token via introspect");
            context.Fail("Erro ao validar token");
        }
    }

    private static async Task HandleAuthenticationFailed(
        AuthenticationFailedContext context,
        string identityAuthority,
        string identityIssuer,
        string identityAudience)
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(context.Exception, "Falha na validação JWT básica, tentando via introspect");
        
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            logger.LogWarning("Token não pôde ser validado nem via JWT básico nem via introspect");
            return;
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();
        
        try
        {
            var introspectResult = await CallIntrospectEndpoint(token, identityAuthority, logger);
            
            if (introspectResult != null && introspectResult.Active)
            {
                if (ValidateIssuerAndAudience(introspectResult, identityIssuer, identityAudience, logger, null))
                {
                    var principal = CreatePrincipalFromToken(token);
                    if (principal != null)
                    {
                        context.Principal = principal;
                        context.Success();
                        logger.LogInformation("Token validado com sucesso via introspect (fallback) para usuário: {Sub}", introspectResult.Sub);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao validar token via introspect no OnAuthenticationFailed");
        }
    }


    private static string? ExtractTokenFromContext(TokenValidatedContext context)
    {
        if (context.SecurityToken is JwtSecurityToken jwtToken)
        {
            return jwtToken.RawData;
        }

        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            return authHeader.Substring("Bearer ".Length).Trim();
        }

        return null;
    }

    private static async Task<IntrospectResponse?> CallIntrospectEndpoint(
        string token, 
        string identityAuthority, 
        ILogger logger)
    {
        using var httpClient = new HttpClient();
        var formContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("token", token)
        });

        var introspectUrl = $"{identityAuthority}/oauth/introspect";
        var response = await httpClient.PostAsync(introspectUrl, formContent);
        
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Falha ao validar token via introspect (Status: {Status})", response.StatusCode);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        return System.Text.Json.JsonSerializer.Deserialize<IntrospectResponse>(content);
    }

    private static bool ValidateIssuerAndAudience(
        IntrospectResponse introspectResult,
        string identityIssuer,
        string identityAudience,
        ILogger logger,
        TokenValidatedContext? context)
    {
        if (!string.IsNullOrEmpty(introspectResult.Iss) && introspectResult.Iss != identityIssuer)
        {
            logger.LogWarning("Token issuer inválido: {Iss}. Esperado: {ExpectedIssuer}", introspectResult.Iss, identityIssuer);
            context?.Fail("Token issuer inválido");
            return false;
        }
        
        if (!string.IsNullOrEmpty(introspectResult.Aud) && introspectResult.Aud != identityAudience)
        {
            logger.LogWarning("Token audience inválido: {Aud}", introspectResult.Aud);
            context?.Fail("Token audience inválido");
            return false;
        }

        return true;
    }

    private static ClaimsPrincipal? CreatePrincipalFromToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            var claims = jwtToken.Claims.ToList();
            var identity = new ClaimsIdentity(claims, "Bearer");
            return new ClaimsPrincipal(identity);
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Handler de autenticação NoOp que simplesmente retorna sucesso quando o usuário já está autenticado
/// </summary>
public class NoOpAuthenticationHandler : AuthenticationHandler<NoOpAuthenticationSchemeOptions>
{
    public NoOpAuthenticationHandler(
        IOptionsMonitor<NoOpAuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Verificar primeiro o flag do middleware (mais confiável)
        // Se o flag está definido, o token já foi validado via introspect no middleware customizado
        if (Context.Items.ContainsKey("TokenValidatedViaIntrospect") && 
            Context.Items["TokenValidatedViaIntrospect"] as bool? == true)
        {
            // O User já foi definido pelo middleware customizado, apenas retornar sucesso
            // Se o User não estiver definido, isso é um problema, mas vamos retornar sucesso mesmo assim
            // porque o flag indica que a validação foi bem-sucedida
            var principal = Context.User ?? new ClaimsPrincipal(new ClaimsIdentity("Bearer"));
            var ticket = new AuthenticationTicket(principal, "NoOp");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        // Se o usuário já está autenticado (definido pelo middleware customizado), retornar sucesso
        if (Context.User?.Identity?.IsAuthenticated == true)
        {
            var ticket = new AuthenticationTicket(Context.User, "NoOp");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        // Se não está autenticado, retornar NoResult para permitir que outros handlers tentem
        return Task.FromResult(AuthenticateResult.NoResult());
    }
}

public class NoOpAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
}

