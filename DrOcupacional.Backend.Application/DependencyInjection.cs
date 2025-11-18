using DrOcupacional.Backend.Application.Interfaces;
using DrOcupacional.Backend.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DrOcupacional.Backend.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IMenuService, MenuService>();

        return services;
    }
}