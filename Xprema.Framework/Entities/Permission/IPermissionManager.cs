namespace Xprema.Framework.Entities.Permission;

/// <summary>
/// Interface for managing permission definitions in the system
/// </summary>
public interface IPermissionManager
{
    /// <summary>
    /// Gets all permission definitions
    /// </summary>
    IReadOnlyCollection<PermissionDefinition> GetPermissions();
    
    /// <summary>
    /// Gets a permission by name
    /// </summary>
    PermissionDefinition? GetPermission(string name);
    
    /// <summary>
    /// Adds a permission definition to the system
    /// </summary>
    PermissionDefinition AddPermission(string name, string displayName, string? description = null, string? group = null);
    
    /// <summary>
    /// Removes a permission definition from the system
    /// </summary>
    void RemovePermission(string name);
    
    /// <summary>
    /// Gets all permission definitions for a specific group
    /// </summary>
    IReadOnlyCollection<PermissionDefinition> GetPermissionsByGroup(string groupName);
    
    /// <summary>
    /// Ensures a permission exists in the database
    /// </summary>
    Task EnsurePermissionAsync(PermissionDefinition permissionDefinition, string createdBy = "system");
    
    /// <summary>
    /// Ensures all defined permissions exist in the database
    /// </summary>
    Task EnsureAllPermissionsAsync(string createdBy = "system");
} 