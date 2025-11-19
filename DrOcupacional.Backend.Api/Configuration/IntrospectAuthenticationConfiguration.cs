using Microsoft.AspNetCore.Authentication;

namespace DrOcupacional.Backend.Api.Configuration;

/// <summary>
/// Configuration for OAuth2 Token Introspection authentication.
/// This follows RFC 7662 (OAuth 2.0 Token Introspection) for validating access tokens.
/// </summary>
public static class IntrospectAuthenticationConfiguration
{
    public static IServiceCollection AddIntrospectAuthentication(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Register HttpClient for introspection calls
        services.AddHttpClient();

        // Configure authentication using token introspection
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = "Introspect";
            options.DefaultChallengeScheme = "Introspect";
        })
        .AddScheme<AuthenticationSchemeOptions, IntrospectAuthenticationHandler>(
            "Introspect",
            options => { });

        services.AddAuthorization();

        return services;
    }
}

