namespace Xprema.Framework.Entities.Permission;

/// <summary>
/// Provides the default set of permissions for the application
/// </summary>
public class DefaultPermissionProvider : PermissionProvider
{
    public const string AdminGroup = "Administration";
    public const string UserManagementGroup = "UserManagement";
    public const string TenantManagementGroup = "TenantManagement";
    
    // Administration permissions
    public const string SystemAccess = AdminGroup + ".SystemAccess";
    public const string ManageSystem = AdminGroup + ".ManageSystem";
    public const string ViewSystem = AdminGroup + ".ViewSystem";
    
    // User management permissions
    public const string UserAccess = UserManagementGroup + ".UserAccess";
    public const string UserCreate = UserManagementGroup + ".UserCreate";
    public const string UserEdit = UserManagementGroup + ".UserEdit";
    public const string UserDelete = UserManagementGroup + ".UserDelete";
    public const string UserView = UserManagementGroup + ".UserView";
    
    // Role management permissions
    public const string RoleAccess = UserManagementGroup + ".RoleAccess";
    public const string RoleCreate = UserManagementGroup + ".RoleCreate";
    public const string RoleEdit = UserManagementGroup + ".RoleEdit";
    public const string RoleDelete = UserManagementGroup + ".RoleDelete";
    public const string RoleView = UserManagementGroup + ".RoleView";
    
    // Tenant management permissions
    public const string TenantAccess = TenantManagementGroup + ".TenantAccess";
    public const string TenantCreate = TenantManagementGroup + ".TenantCreate";
    public const string TenantEdit = TenantManagementGroup + ".TenantEdit";
    public const string TenantDelete = TenantManagementGroup + ".TenantDelete";
    public const string TenantView = TenantManagementGroup + ".TenantView";
    
    /// <summary>
    /// Defines the permissions in the system
    /// </summary>
    public override void Define(IPermissionDefinitionContext context)
    {
        // Administration group
        var adminGroup = context.AddGroup(AdminGroup, "Administration");
        
        var systemAccess = adminGroup.AddPermission(
            name: "SystemAccess",
            displayName: "System Access",
            description: "Allows access to system administration functions");
            
        systemAccess.AddChild(
            name: "ManageSystem",
            displayName: "Manage System",
            description: "Allows managing system settings")
            .IsEnabledByDefault = false;
            
        systemAccess.AddChild(
            name: "ViewSystem",
            displayName: "View System",
            description: "Allows viewing system information")
            .IsEnabledByDefault = true;
            
        // User management group
        var userGroup = context.AddGroup(UserManagementGroup, "User Management");
        
        var userAccess = userGroup.AddPermission(
            name: "UserAccess",
            displayName: "User Access",
            description: "Allows access to user management functions");
            
        userAccess.AddChild(
            name: "UserCreate",
            displayName: "Create Users",
            description: "Allows creating new users");
                
        userAccess.AddChild(
            name: "UserEdit",
            displayName: "Edit Users",
            description: "Allows editing existing users");
                
        userAccess.AddChild(
            name: "UserDelete",
            displayName: "Delete Users",
            description: "Allows deleting users")
            .IsEnabledByDefault = false;
                
        userAccess.AddChild(
            name: "UserView",
            displayName: "View Users",
            description: "Allows viewing user information")
            .IsEnabledByDefault = true;
            
        // Role management permissions
        var roleAccess = userGroup.AddPermission(
            name: "RoleAccess",
            displayName: "Role Access",
            description: "Allows access to role management functions");
            
        roleAccess.AddChild(
            name: "RoleCreate",
            displayName: "Create Roles",
            description: "Allows creating new roles");
                
        roleAccess.AddChild(
            name: "RoleEdit",
            displayName: "Edit Roles",
            description: "Allows editing existing roles");
                
        roleAccess.AddChild(
            name: "RoleDelete",
            displayName: "Delete Roles",
            description: "Allows deleting roles")
            .IsEnabledByDefault = false;
                
        roleAccess.AddChild(
            name: "RoleView",
            displayName: "View Roles",
            description: "Allows viewing role information")
            .IsEnabledByDefault = true;
            
        // Tenant management group
        var tenantGroup = context.AddGroup(TenantManagementGroup, "Tenant Management");
        
        var tenantAccess = tenantGroup.AddPermission(
            name: "TenantAccess",
            displayName: "Tenant Access",
            description: "Allows access to tenant management functions");
            
        tenantAccess.AddChild(
            name: "TenantCreate",
            displayName: "Create Tenants",
            description: "Allows creating new tenants");
                
        tenantAccess.AddChild(
            name: "TenantEdit",
            displayName: "Edit Tenants",
            description: "Allows editing existing tenants");
                
        tenantAccess.AddChild(
            name: "TenantDelete",
            displayName: "Delete Tenants",
            description: "Allows deleting tenants")
            .IsEnabledByDefault = false;
                
        tenantAccess.AddChild(
            name: "TenantView",
            displayName: "View Tenants",
            description: "Allows viewing tenant information")
            .IsEnabledByDefault = true;
    }
} 