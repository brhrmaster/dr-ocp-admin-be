using DrOcupacional.Backend.Api.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DrOcupacional.Backend.Api.Configuration;

public class IntrospectTokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IntrospectTokenValidationMiddleware> _logger;

    public IntrospectTokenValidationMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<IntrospectTokenValidationMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();
            var identityAuthority = _configuration["Identity:Authority"] ?? "http://localhost:8081";
            var identityIssuer = _configuration["Identity:Issuer"] ?? "DrOcupacional.Identity";
            var identityAudience = _configuration["Identity:Audience"] ?? "ui-app";

            try
            {
                var introspectResult = await ValidateTokenViaIntrospect(token, identityAuthority);
                
                if (introspectResult != null && introspectResult.Active)
                {
                    if (ValidateIssuerAndAudience(introspectResult, identityIssuer, identityAudience))
                    {
                        var principal = CreatePrincipalFromToken(token);
                        if (principal != null)
                        {
                            context.User = principal;
                            context.Items["TokenValidatedViaIntrospect"] = true;
                            
                            _logger.LogInformation(
                                "Token validado via introspect no middleware customizado para usuário: {Sub}. User.Identity.IsAuthenticated: {IsAuthenticated}, AuthenticationType: {AuthType}", 
                                introspectResult.Sub, 
                                context.User.Identity?.IsAuthenticated,
                                context.User.Identity?.AuthenticationType);
                        }
                        else
                        {
                            _logger.LogWarning("Falha ao criar principal do token para usuário: {Sub}", introspectResult.Sub);
                        }
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Token rejeitado: issuer ou audience inválidos. Iss: {Iss}, Aud: {Aud}", 
                            introspectResult.Iss, 
                            introspectResult.Aud);
                    }
                }
                else
                {
                    _logger.LogWarning("Token rejeitado pelo introspect: não está ativo");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar token via introspect no middleware customizado");
                // Continuar com a validação JWT padrão
            }
        }
        
        await _next(context);
    }

    private async Task<IntrospectResponse?> ValidateTokenViaIntrospect(string token, string identityAuthority)
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
            _logger.LogWarning("Falha ao chamar introspect endpoint: {Status}", response.StatusCode);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        return System.Text.Json.JsonSerializer.Deserialize<IntrospectResponse>(content);
    }

    private bool ValidateIssuerAndAudience(
        IntrospectResponse introspectResult,
        string identityIssuer,
        string identityAudience)
    {
        if (!string.IsNullOrEmpty(introspectResult.Iss) && introspectResult.Iss != identityIssuer)
        {
            return false;
        }
        
        if (!string.IsNullOrEmpty(introspectResult.Aud) && introspectResult.Aud != identityAudience)
        {
            return false;
        }

        return true;
    }

    private ClaimsPrincipal? CreatePrincipalFromToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            var claims = jwtToken.Claims.ToList();
            // Criar identidade autenticada - o tipo "Bearer" garante que IsAuthenticated seja true
            var identity = new ClaimsIdentity(claims, "Bearer");
            
            // Garantir que a identidade esteja marcada como autenticada
            if (!identity.IsAuthenticated)
            {
                _logger.LogWarning("Identidade criada não está marcada como autenticada");
            }
            
            return new ClaimsPrincipal(identity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar principal do token");
            return null;
        }
    }
}

public static class IntrospectTokenValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseIntrospectTokenValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<IntrospectTokenValidationMiddleware>();
    }
}

