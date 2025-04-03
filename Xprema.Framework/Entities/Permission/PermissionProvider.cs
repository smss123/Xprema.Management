namespace Xprema.Framework.Entities.Permission;

/// <summary>
/// Abstract base class for permission providers
/// </summary>
public abstract class PermissionProvider
{
    /// <summary>
    /// Defines permissions in the system
    /// </summary>
    /// <param name="context">The permission definition context</param>
    public abstract void Define(IPermissionDefinitionContext context);
}

/// <summary>
/// Context for defining permissions
/// </summary>
public interface IPermissionDefinitionContext
{
    /// <summary>
    /// Adds a permission group
    /// </summary>
    IPermissionGroupDefinitionContext AddGroup(string name, string displayName);
    
    /// <summary>
    /// Gets a permission group by name
    /// </summary>
    IPermissionGroupDefinitionContext GetGroup(string name);
    
    /// <summary>
    /// Gets the permission manager
    /// </summary>
    IPermissionManager PermissionManager { get; }
}

/// <summary>
/// Context for defining permissions within a group
/// </summary>
public interface IPermissionGroupDefinitionContext
{
    /// <summary>
    /// Gets the name of the group
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Gets the display name of the group
    /// </summary>
    string DisplayName { get; }
    
    /// <summary>
    /// Adds a permission to the group
    /// </summary>
    PermissionDefinition AddPermission(
        string name,
        string displayName,
        string? description = null);
        
    /// <summary>
    /// Gets a permission by name
    /// </summary>
    PermissionDefinition GetPermission(string name);
}

/// <summary>
/// Implementation of the permission definition context
/// </summary>
public class PermissionDefinitionContext : IPermissionDefinitionContext
{
    private readonly Dictionary<string, IPermissionGroupDefinitionContext> _groups = new();
    
    public PermissionDefinitionContext(IPermissionManager permissionManager)
    {
        PermissionManager = permissionManager;
    }
    
    public IPermissionManager PermissionManager { get; }
    
    public IPermissionGroupDefinitionContext AddGroup(string name, string displayName)
    {
        if (_groups.ContainsKey(name))
        {
            return _groups[name];
        }
        
        var group = new PermissionGroupDefinitionContext(name, displayName, PermissionManager);
        _groups[name] = group;
        return group;
    }
    
    public IPermissionGroupDefinitionContext GetGroup(string name)
    {
        if (!_groups.TryGetValue(name, out var group))
        {
            throw new InvalidOperationException($"Permission group '{name}' not found.");
        }
        
        return group;
    }
}

/// <summary>
/// Implementation of the permission group definition context
/// </summary>
public class PermissionGroupDefinitionContext : IPermissionGroupDefinitionContext
{
    private readonly Dictionary<string, PermissionDefinition> _permissions = new();
    private readonly IPermissionManager _permissionManager;
    
    public PermissionGroupDefinitionContext(
        string name,
        string displayName,
        IPermissionManager permissionManager)
    {
        Name = name;
        DisplayName = displayName;
        _permissionManager = permissionManager;
    }
    
    public string Name { get; }
    
    public string DisplayName { get; }
    
    public PermissionDefinition AddPermission(
        string name,
        string displayName,
        string? description = null)
    {
        var fullName = $"{Name}.{name}";
        
        if (_permissions.ContainsKey(fullName))
        {
            return _permissions[fullName];
        }
        
        var permission = _permissionManager.AddPermission(
            fullName,
            displayName,
            description,
            Name);
            
        _permissions[fullName] = permission;
        return permission;
    }
    
    public PermissionDefinition GetPermission(string name)
    {
        var fullName = $"{Name}.{name}";
        
        if (!_permissions.TryGetValue(fullName, out var permission))
        {
            throw new InvalidOperationException($"Permission '{fullName}' not found.");
        }
        
        return permission;
    }
} 