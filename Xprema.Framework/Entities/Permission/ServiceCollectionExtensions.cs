using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xprema.Framework.Entities.MultiTenancy;

namespace Xprema.Framework.Entities.Permission;

/// <summary>
/// Extension methods for registering the permission system in ASP.NET Core
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add the permission system to the service collection
    /// </summary>
    public static IServiceCollection AddXpremaPermissions<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        // Add tenant services
        services.AddScoped<ITenantContextAccessor, TenantContextAccessor<TDbContext>>();
        services.AddScoped<ITenantService, TenantService<TDbContext>>();
        
        // Add permission service
        services.AddScoped<IPermissionService, PermissionService<TDbContext>>();
        
        return services;
    }
} 