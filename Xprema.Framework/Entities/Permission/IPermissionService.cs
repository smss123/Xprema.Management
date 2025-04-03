namespace Xprema.Framework.Entities.Permission;

/// <summary>
/// Service for managing permissions and authorization
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Check if a user has a specific permission
    /// </summary>
    Task<bool> HasPermissionAsync(Guid userId, string permissionSystemName);
    
    /// <summary>
    /// Check if a user has any of the specified permissions
    /// </summary>
    Task<bool> HasAnyPermissionAsync(Guid userId, IEnumerable<string> permissionSystemNames);
    
    /// <summary>
    /// Check if a user has all of the specified permissions
    /// </summary>
    Task<bool> HasAllPermissionsAsync(Guid userId, IEnumerable<string> permissionSystemNames);
    
    /// <summary>
    /// Get all permissions for a user
    /// </summary>
    Task<IEnumerable<Permission>> GetUserPermissionsAsync(Guid userId);
    
    /// <summary>
    /// Get all roles for a user
    /// </summary>
    Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId);
    
    /// <summary>
    /// Assign a role to a user
    /// </summary>
    Task AssignRoleToUserAsync(Guid userId, Guid roleId, string assignedBy);
    
    /// <summary>
    /// Remove a role from a user
    /// </summary>
    Task RemoveRoleFromUserAsync(Guid userId, Guid roleId, string removedBy);
    
    /// <summary>
    /// Create a new role
    /// </summary>
    Task<Role> CreateRoleAsync(string name, string? description, bool isSystemRole, string createdBy);
    
    /// <summary>
    /// Create a new permission
    /// </summary>
    Task<Permission> CreatePermissionAsync(string name, string systemName, string? description, string? group, string createdBy);
    
    /// <summary>
    /// Assign a permission to a role
    /// </summary>
    Task AssignPermissionToRoleAsync(Guid roleId, Guid permissionId, string assignedBy);
    
    /// <summary>
    /// Remove a permission from a role
    /// </summary>
    Task RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId, string removedBy);
} 