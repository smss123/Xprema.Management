using Microsoft.Extensions.DependencyInjection;

namespace Xprema.Framework.Entities.Permission;

/// <summary>
/// Manages permission definitions in the system
/// </summary>
public class PermissionManager : IPermissionManager
{
    private readonly Dictionary<string, PermissionDefinition> _permissions = new();
    private readonly IServiceProvider _serviceProvider;
    
    public PermissionManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    /// <summary>
    /// Gets all permission definitions
    /// </summary>
    public IReadOnlyCollection<PermissionDefinition> GetPermissions()
    {
        return _permissions.Values.ToList();
    }
    
    /// <summary>
    /// Gets a permission by name
    /// </summary>
    public PermissionDefinition? GetPermission(string name)
    {
        return _permissions.TryGetValue(name, out var permission) ? permission : null;
    }
    
    /// <summary>
    /// Adds a permission definition to the system
    /// </summary>
    public PermissionDefinition AddPermission(string name, string displayName, string? description = null, string? group = null)
    {
        if (_permissions.ContainsKey(name))
        {
            throw new InvalidOperationException($"Permission with name '{name}' already exists.");
        }
        
        var permission = new PermissionDefinition(name, displayName, description)
        {
            Group = group
        };
        
        _permissions[name] = permission;
        return permission;
    }
    
    /// <summary>
    /// Removes a permission definition from the system
    /// </summary>
    public void RemovePermission(string name)
    {
        if (!_permissions.ContainsKey(name))
        {
            throw new InvalidOperationException($"Permission with name '{name}' does not exist.");
        }
        
        _permissions.Remove(name);
    }
    
    /// <summary>
    /// Gets all permission definitions for a specific group
    /// </summary>
    public IReadOnlyCollection<PermissionDefinition> GetPermissionsByGroup(string groupName)
    {
        return _permissions.Values.Where(p => p.Group == groupName).ToList();
    }
    
    /// <summary>
    /// Ensures a permission exists in the database
    /// </summary>
    /// <param name="permissionDefinition">The permission definition to ensure exists</param>
    /// <param name="createdBy">The user creating the permission (defaults to "system")</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task EnsurePermissionAsync(PermissionDefinition permissionDefinition, string createdBy = "system")
    {
        // Get the permission service from service provider
        var permissionService = _serviceProvider.GetRequiredService<IPermissionService>();
        
        try
        {
            var existingPermissions = await permissionService.GetUserPermissionsAsync(Guid.Empty);
            
            // Check if the permission exists by system name
            if (existingPermissions.Any(p => p.SystemName == permissionDefinition.Name))
            {
                return;
            }
            
            // Create the permission in the database
            var result = await permissionService.CreatePermissionAsync(
                permissionDefinition.DisplayName,
                permissionDefinition.Name,
                permissionDefinition.Description,
                permissionDefinition.Group,
                createdBy);
                
            // Add children recursively
            foreach (var child in permissionDefinition.Children)
            {
                await EnsurePermissionAsync(child, createdBy);
            }
        }
        catch (Exception ex)
        {
            // Log exception and rethrow
            Console.WriteLine($"Error ensuring permission: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Ensures all defined permissions exist in the database
    /// </summary>
    public async Task EnsureAllPermissionsAsync(string createdBy = "system")
    {
        foreach (var permission in _permissions.Values)
        {
            await EnsurePermissionAsync(permission, createdBy);
        }
    }
} 