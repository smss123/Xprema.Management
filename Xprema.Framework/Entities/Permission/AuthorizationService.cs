using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Xprema.Framework.Entities.Identity;

namespace Xprema.Framework.Entities.Permission;

/// <summary>
/// Service for authorization checks, similar to ABP's authorization system
/// </summary>
public class AuthorizationService : IAuthorizationService
{
    private readonly IPermissionService _permissionService;
    private readonly IServiceProvider _serviceProvider;

    public AuthorizationService(
        IPermissionService permissionService,
        IServiceProvider serviceProvider)
    {
        _permissionService = permissionService;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Checks if the current user is authorized based on the given permission name
    /// </summary>
    /// <param name="permissionName">The name of the permission to check</param>
    /// <returns>True if authorized, false otherwise</returns>
    public async Task<bool> IsGrantedAsync(string permissionName)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return false;
        }

        return await _permissionService.HasPermissionAsync(userId.Value, permissionName);
    }

    /// <summary>
    /// Checks if a specific user is authorized based on the given permission name
    /// </summary>
    /// <param name="userId">The ID of the user to check</param>
    /// <param name="permissionName">The name of the permission to check</param>
    /// <returns>True if authorized, false otherwise</returns>
    public async Task<bool> IsGrantedAsync(Guid userId, string permissionName)
    {
        return await _permissionService.HasPermissionAsync(userId, permissionName);
    }

    /// <summary>
    /// Checks if the current user has all the specified permissions
    /// </summary>
    /// <param name="permissionNames">The names of the permissions to check</param>
    /// <returns>True if the user has all the permissions, false otherwise</returns>
    public async Task<bool> IsGrantedAllAsync(params string[] permissionNames)
    {
        var userId = GetCurrentUserId();
        if (userId == null || permissionNames.Length == 0)
        {
            return false;
        }

        return await _permissionService.HasAllPermissionsAsync(userId.Value, permissionNames);
    }

    /// <summary>
    /// Checks if the current user has any of the specified permissions
    /// </summary>
    /// <param name="permissionNames">The names of the permissions to check</param>
    /// <returns>True if the user has any of the permissions, false otherwise</returns>
    public async Task<bool> IsGrantedAnyAsync(params string[] permissionNames)
    {
        var userId = GetCurrentUserId();
        if (userId == null || permissionNames.Length == 0)
        {
            return false;
        }

        return await _permissionService.HasAnyPermissionAsync(userId.Value, permissionNames);
    }

    /// <summary>
    /// Gets all permissions for the current user
    /// </summary>
    /// <returns>A collection of permissions</returns>
    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return new List<Permission>();
        }

        return await _permissionService.GetUserPermissionsAsync(userId.Value);
    }

    /// <summary>
    /// Gets all permissions for a specific user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <returns>A collection of permissions</returns>
    public async Task<IEnumerable<Permission>> GetPermissionsAsync(Guid userId)
    {
        return await _permissionService.GetUserPermissionsAsync(userId);
    }

    /// <summary>
    /// Gets all roles for the current user
    /// </summary>
    /// <returns>A collection of roles</returns>
    public async Task<IEnumerable<Role>> GetRolesAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return new List<Role>();
        }

        return await _permissionService.GetUserRolesAsync(userId.Value);
    }

    /// <summary>
    /// Gets all roles for a specific user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <returns>A collection of roles</returns>
    public async Task<IEnumerable<Role>> GetRolesAsync(Guid userId)
    {
        return await _permissionService.GetUserRolesAsync(userId);
    }

    /// <summary>
    /// Gets the current user ID from the current HttpContext
    /// </summary>
    /// <returns>The ID of the current user, or null if not authenticated</returns>
    private Guid? GetCurrentUserId()
    {
        try
        {
            // Try to get IHttpContextAccessor from service provider
            var httpContextAccessor = _serviceProvider.GetService<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            if (httpContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var userIdClaim = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return null;
            }

            return userId;
        }
        catch
        {
            return null;
        }
    }
} 