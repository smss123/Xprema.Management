namespace Xprema.Framework.Entities.Permission;

/// <summary>
/// Defines the authorization service operations
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Checks if the current user is authorized based on the given permission name
    /// </summary>
    /// <param name="permissionName">The name of the permission to check</param>
    /// <returns>True if authorized, false otherwise</returns>
    Task<bool> IsGrantedAsync(string permissionName);

    /// <summary>
    /// Checks if a specific user is authorized based on the given permission name
    /// </summary>
    /// <param name="userId">The ID of the user to check</param>
    /// <param name="permissionName">The name of the permission to check</param>
    /// <returns>True if authorized, false otherwise</returns>
    Task<bool> IsGrantedAsync(Guid userId, string permissionName);

    /// <summary>
    /// Checks if the current user has all the specified permissions
    /// </summary>
    /// <param name="permissionNames">The names of the permissions to check</param>
    /// <returns>True if the user has all the permissions, false otherwise</returns>
    Task<bool> IsGrantedAllAsync(params string[] permissionNames);

    /// <summary>
    /// Checks if the current user has any of the specified permissions
    /// </summary>
    /// <param name="permissionNames">The names of the permissions to check</param>
    /// <returns>True if the user has any of the permissions, false otherwise</returns>
    Task<bool> IsGrantedAnyAsync(params string[] permissionNames);

    /// <summary>
    /// Gets all permissions for the current user
    /// </summary>
    /// <returns>A collection of permissions</returns>
    Task<IEnumerable<Permission>> GetPermissionsAsync();

    /// <summary>
    /// Gets all permissions for a specific user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <returns>A collection of permissions</returns>
    Task<IEnumerable<Permission>> GetPermissionsAsync(Guid userId);

    /// <summary>
    /// Gets all roles for the current user
    /// </summary>
    /// <returns>A collection of roles</returns>
    Task<IEnumerable<Role>> GetRolesAsync();

    /// <summary>
    /// Gets all roles for a specific user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <returns>A collection of roles</returns>
    Task<IEnumerable<Role>> GetRolesAsync(Guid userId);
} 