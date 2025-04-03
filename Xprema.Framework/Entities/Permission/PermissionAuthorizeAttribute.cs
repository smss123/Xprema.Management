using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Xprema.Framework.Entities.Permission;

/// <summary>
/// Authorization attribute for controller methods to check for permissions
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class PermissionAuthorizeAttribute : AuthorizeAttribute, IAsyncAuthorizationFilter
{
    public string? PermissionName { get; }
    public string[]? PermissionNames { get; }
    public bool RequireAllPermissions { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionAuthorizeAttribute"/> class with a single permission
    /// </summary>
    /// <param name="permissionName">The name of the permission to check</param>
    public PermissionAuthorizeAttribute(string permissionName)
    {
        PermissionName = permissionName;
        PermissionNames = null;
        RequireAllPermissions = false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionAuthorizeAttribute"/> class with multiple permissions
    /// </summary>
    /// <param name="requireAllPermissions">Whether to require all permissions or any permission</param>
    /// <param name="permissionNames">The names of the permissions to check</param>
    public PermissionAuthorizeAttribute(bool requireAllPermissions, params string[] permissionNames)
    {
        PermissionName = null;
        PermissionNames = permissionNames;
        RequireAllPermissions = requireAllPermissions;
    }

    /// <summary>
    /// Checks authorization based on the defined permissions
    /// </summary>
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // Skip if anonymous access is allowed
        if (context.ActionDescriptor.EndpointMetadata.Any(m => m is AllowAnonymousAttribute))
        {
            return;
        }

        // Skip if user is not authenticated
        if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Get the authorization service
        var authorizationService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();

        bool isAuthorized;

        if (!string.IsNullOrEmpty(PermissionName))
        {
            // Check for a single permission
            isAuthorized = await authorizationService.IsGrantedAsync(PermissionName);
        }
        else if (PermissionNames != null && PermissionNames.Length > 0)
        {
            // Check for multiple permissions
            isAuthorized = RequireAllPermissions
                ? await authorizationService.IsGrantedAllAsync(PermissionNames)
                : await authorizationService.IsGrantedAnyAsync(PermissionNames);
        }
        else
        {
            // No permissions specified, consider authorized (basic user authentication)
            isAuthorized = true;
        }

        if (!isAuthorized)
        {
            context.Result = new ForbidResult();
        }
    }
} 