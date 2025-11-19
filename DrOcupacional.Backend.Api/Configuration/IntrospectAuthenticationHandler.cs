using DrOcupacional.Backend.Api.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace DrOcupacional.Backend.Api.Configuration;

/// <summary>
/// Authentication handler that validates access tokens using OAuth2 Token Introspection (RFC 7662).
/// This is the recommended approach for resource servers to validate tokens with the authorization server.
/// </summary>
public class IntrospectAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IntrospectAuthenticationHandler> _logger;

    public IntrospectAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
        : base(options, logger, encoder)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger.CreateLogger<IntrospectAuthenticationHandler>();
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Extract token from Authorization header
        var authHeader = Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();
        if (string.IsNullOrEmpty(token))
        {
            return AuthenticateResult.NoResult();
        }

        try
        {
            // Validate token via introspection endpoint
            var introspectResult = await ValidateTokenViaIntrospectAsync(token);
            
            if (introspectResult == null || !introspectResult.Active)
            {
                _logger.LogWarning("Token validation failed: token is not active");
                return AuthenticateResult.Fail("Token is not active or invalid");
            }

            // Validate issuer and audience
            var identityIssuer = _configuration["Identity:Issuer"] ?? "DrOcupacional.Identity";
            var identityAudience = _configuration["Identity:Audience"] ?? "ui-app";

            if (!ValidateIssuerAndAudience(introspectResult, identityIssuer, identityAudience))
            {
                _logger.LogWarning(
                    "Token validation failed: invalid issuer or audience. Iss: {Iss}, Aud: {Aud}",
                    introspectResult.Iss,
                    introspectResult.Aud);
                return AuthenticateResult.Fail("Token issuer or audience is invalid");
            }

            // Extract claims from token
            var principal = CreatePrincipalFromToken(token, introspectResult);
            if (principal == null)
            {
                _logger.LogWarning("Failed to create principal from token");
                return AuthenticateResult.Fail("Failed to extract claims from token");
            }

            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            
            _logger.LogDebug(
                "Token validated successfully via introspection for user: {Sub}",
                introspectResult.Sub);

            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token via introspection");
            return AuthenticateResult.Fail("Error validating token");
        }
    }

    private async Task<IntrospectResponse?> ValidateTokenViaIntrospectAsync(string token)
    {
        var identityAuthority = _configuration["Identity:Authority"] ?? "http://localhost:8081";
        var introspectUrl = $"{identityAuthority}/oauth/introspect";

        using var httpClient = _httpClientFactory.CreateClient();
        
        var formContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("token", token)
        });

        var response = await httpClient.PostAsync(introspectUrl, formContent);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Introspection endpoint returned error status: {StatusCode}",
                response.StatusCode);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IntrospectResponse>(content);
    }

    private static bool ValidateIssuerAndAudience(
        IntrospectResponse introspectResult,
        string expectedIssuer,
        string expectedAudience)
    {
        // Validate issuer if present in response
        if (!string.IsNullOrEmpty(introspectResult.Iss) && 
            !string.Equals(introspectResult.Iss, expectedIssuer, StringComparison.Ordinal))
        {
            return false;
        }

        // Validate audience if present in response
        if (!string.IsNullOrEmpty(introspectResult.Aud) && 
            !string.Equals(introspectResult.Aud, expectedAudience, StringComparison.Ordinal))
        {
            return false;
        }

        return true;
    }

    private static ClaimsPrincipal? CreatePrincipalFromToken(
        string token,
        IntrospectResponse introspectResult)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            
            // Read token to extract claims
            if (!handler.CanReadToken(token))
            {
                return null;
            }

            var jwtToken = handler.ReadJwtToken(token);
            var claims = jwtToken.Claims.ToList();

            // Add claims from introspection response if not already present
            if (!string.IsNullOrEmpty(introspectResult.Sub) && 
                !claims.Any(c => c.Type == ClaimTypes.NameIdentifier))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, introspectResult.Sub));
            }

            if (!string.IsNullOrEmpty(introspectResult.Username) && 
                !claims.Any(c => c.Type == ClaimTypes.Name))
            {
                claims.Add(new Claim(ClaimTypes.Name, introspectResult.Username));
            }

            if (!string.IsNullOrEmpty(introspectResult.Scope))
            {
                claims.Add(new Claim("scope", introspectResult.Scope));
            }

            var identity = new ClaimsIdentity(claims, "Bearer", ClaimTypes.Name, ClaimTypes.Role);
            return new ClaimsPrincipal(identity);
        }
        catch
        {
            return null;
        }
    }
}

