namespace Xprema.Framework.Entities.Permission;

/// <summary>
/// Attribute to require specific permissions for API endpoints
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class RequirePermissionAttribute : Attribute
{
    public string PermissionSystemName { get; }
    
    public RequirePermissionAttribute(string permissionSystemName)
    {
        PermissionSystemName = permissionSystemName;
    }
} 