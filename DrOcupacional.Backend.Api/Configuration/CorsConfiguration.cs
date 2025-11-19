namespace DrOcupacional.Backend.Api.Configuration;

public static class CorsConfiguration
{
    public static IServiceCollection AddCorsConfiguration(
        this IServiceCollection services, 
        IWebHostEnvironment environment)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:3000")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
            
            // Policy para desenvolvimento - permite requisições sem origem (curl, Postman, etc)
            if (environment.IsDevelopment())
            {
                options.AddPolicy("AllowDevelopment", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            }
        });

        return services;
    }

    public static WebApplication UseCorsConfiguration(this WebApplication app)
    {
        // Usar CORS apropriado baseado no ambiente
        if (app.Environment.IsDevelopment())
        {
            app.UseCors("AllowDevelopment");
        }
        else
        {
            app.UseCors("AllowFrontend");
        }

        return app;
    }
}


