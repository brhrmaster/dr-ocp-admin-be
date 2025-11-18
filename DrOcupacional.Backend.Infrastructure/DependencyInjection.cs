using DrOcupacional.Backend.Domain.Repositories;
using DrOcupacional.Backend.Infrastructure.Data;
using DrOcupacional.Backend.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DrOcupacional.Backend.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddSingleton<IDbConnectionFactory>(sp => new DbConnectionFactory(connectionString));
        services.AddScoped<IMenuRepository, MenuRepository>();

        return services;
    }
}