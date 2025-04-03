using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;

namespace Xprema.Framework.Entities.Permission;

/// <summary>
/// Extension methods for registering the enhanced authorization system
/// </summary>
public static class AuthorizationServiceCollectionExtensions
{
    /// <summary>
    /// Adds the enhanced Xprema authorization system to the service collection
    /// </summary>
    public static IServiceCollection AddXpremaAuthorization(this IServiceCollection services)
    {
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
    
    /// <summary>
    /// Adds permission authorization to MVC options
    /// </summary>
    public static MvcOptions AddPermissionAuthorization(this MvcOptions options)
    {
        // Add global filter for permission authorization
        options.Filters.Add<PermissionAuthorizationFilter>();
        return options;
    }
}

/// <summary>
/// Filter that integrates the permission system with ASP.NET Core MVC
/// </summary>
public class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly IAuthorizationService _authorizationService;
    
    public PermissionAuthorizationFilter(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }
    
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // Skip authorization if action allows anonymous access
        if (context.ActionDescriptor.EndpointMetadata.Any(em => em is Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute))
        {
            return;
        }
        
        // Get permission attributes
        var permissionAttributes = context.ActionDescriptor.EndpointMetadata
            .OfType<PermissionAuthorizeAttribute>()
            .ToList();
            
        if (!permissionAttributes.Any())
        {
            return; // No permission attributes, skip
        }
        
        // Check permissions
        foreach (var attribute in permissionAttributes)
        {
            bool isAuthorized;
            
            if (attribute.RequireAllPermissions)
            {
                isAuthorized = await _authorizationService.IsGrantedAllAsync(attribute.PermissionNames ?? Array.Empty<string>());
            }
            else
            {
                isAuthorized = await _authorizationService.IsGrantedAnyAsync(attribute.PermissionNames ?? Array.Empty<string>());
            }
            
            if (!isAuthorized)
            {
                // Return 403 Forbidden if user doesn't have required permissions
                context.Result = new ForbidResult();
                return;
            }
        }
    }
} 