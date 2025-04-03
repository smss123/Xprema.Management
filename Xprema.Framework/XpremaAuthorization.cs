using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Xprema.Framework.Entities.Permission;

namespace Xprema.Framework;

/// <summary>
/// Registration extensions for Xprema authorization services
/// </summary>
public static class XpremaAuthorization
{
    /// <summary>
    /// Adds the enhanced Xprema authorization system to the service collection
    /// </summary>
    public static IServiceCollection AddXpremaEnhancedAuthorization(this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        // Register permission manager and authorization service
        services.AddScoped<IPermissionManager, PermissionManager>();
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        services.AddScoped<IPermissionDefinitionContext, PermissionDefinitionContext>();
        
        // Register all permission providers
        var permissionProviders = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(PermissionProvider)))
            .ToList();
            
        foreach (var providerType in permissionProviders)
        {
            services.AddScoped(typeof(PermissionProvider), providerType);
        }
        
        return services;
    }
} 