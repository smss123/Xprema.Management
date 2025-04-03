using Microsoft.EntityFrameworkCore;
using Xprema.Framework.Entities.MultiTenancy;
using Xprema.Framework.Entities.Permission;

namespace Xprema.Framework.Tests;

public class PermissionServiceTests : TestBase
{
    [Fact]
    public async Task CreateRole_ShouldReturnValidRole()
    {
        // Arrange
        var tenant = await CreateTestTenantAsync();
        TenantContextAccessor.SetCurrentTenantId(tenant.Id);
        string roleName = "Admin";
        string description = "Administrator role";
        
        // Act
        var role = await PermissionService.CreateRoleAsync(roleName, description, true, "system");
        
        // Assert
        Assert.NotNull(role);
        Assert.Equal(roleName, role.Name);
        Assert.Equal(description, role.Description);
        Assert.True(role.IsSystemRole);
        Assert.Equal("system", role.CreatedBy);
        Assert.Equal(tenant.Id, role.TenantId);
    }
    
    [Fact]
    public async Task CreatePermission_ShouldReturnValidPermission()
    {
        // Arrange
        var tenant = await CreateTestTenantAsync();
        TenantContextAccessor.SetCurrentTenantId(tenant.Id);
        string permissionName = "Create Product";
        string systemName = "Products.Create";
        string description = "Permission to create products";
        string group = "Products";
        
        // Act
        var permission = await PermissionService.CreatePermissionAsync(
            permissionName, 
            systemName, 
            description, 
            group, 
            "system");
        
        // Assert
        Assert.NotNull(permission);
        Assert.Equal(permissionName, permission.Name);
        Assert.Equal(systemName, permission.SystemName);
        Assert.Equal(description, permission.Description);
        Assert.Equal(group, permission.Group);
        Assert.Equal("system", permission.CreatedBy);
        Assert.Equal(tenant.Id, permission.TenantId);
    }
    
    [Fact]
    public async Task AssignPermissionToRole_ShouldCreateAssociation()
    {
        // Arrange
        var tenant = await CreateTestTenantAsync();
        TenantContextAccessor.SetCurrentTenantId(tenant.Id);
        var role = await PermissionService.CreateRoleAsync("Admin", "Administrator role", true, "system");
        var permission = await PermissionService.CreatePermissionAsync(
            "Create Product", 
            "Products.Create", 
            "Permission to create products", 
            "Products", 
            "system");
        
        // Act
        await PermissionService.AssignPermissionToRoleAsync(role.Id, permission.Id, "system");
        
        // Assert
        var rolePermission = await DbContext.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id);
        Assert.NotNull(rolePermission);
        Assert.Equal(tenant.Id, rolePermission.TenantId);
    }
    
    [Fact]
    public async Task RemovePermissionFromRole_ShouldMarkAssociationAsDeleted()
    {
        // Arrange
        var tenant = await CreateTestTenantAsync();
        TenantContextAccessor.SetCurrentTenantId(tenant.Id);
        var role = await PermissionService.CreateRoleAsync("Admin", "Administrator role", true, "system");
        var permission = await PermissionService.CreatePermissionAsync(
            "Create Product", 
            "Products.Create", 
            "Permission to create products", 
            "Products", 
            "system");
        await PermissionService.AssignPermissionToRoleAsync(role.Id, permission.Id, "system");
        
        // Act
        await PermissionService.RemovePermissionFromRoleAsync(role.Id, permission.Id, "admin");
        
        // Assert
        var rolePermission = await DbContext.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id);
        Assert.NotNull(rolePermission);
        Assert.True(rolePermission.IsDeleted);
        Assert.Equal("admin", rolePermission.DeletedBy);
        Assert.NotNull(rolePermission.DeletedDate);
    }
    
    [Fact]
    public async Task AssignRoleToUser_ShouldCreateAssociation()
    {
        // Arrange
        var tenant = await CreateTestTenantAsync();
        TenantContextAccessor.SetCurrentTenantId(tenant.Id);
        var role = await PermissionService.CreateRoleAsync("Admin", "Administrator role", true, "system");
        var userId = Guid.NewGuid();
        
        // Act
        await PermissionService.AssignRoleToUserAsync(userId, role.Id, "system");
        
        // Assert
        var userRole = await DbContext.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);
        Assert.NotNull(userRole);
        Assert.Equal(tenant.Id, userRole.TenantId);
    }
    
    [Fact]
    public async Task RemoveRoleFromUser_ShouldMarkAssociationAsDeleted()
    {
        // Arrange
        var tenant = await CreateTestTenantAsync();
        TenantContextAccessor.SetCurrentTenantId(tenant.Id);
        var role = await PermissionService.CreateRoleAsync("Admin", "Administrator role", true, "system");
        var userId = Guid.NewGuid();
        await PermissionService.AssignRoleToUserAsync(userId, role.Id, "system");
        
        // Act
        await PermissionService.RemoveRoleFromUserAsync(userId, role.Id, "admin");
        
        // Assert
        var userRole = await DbContext.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);
        Assert.NotNull(userRole);
        Assert.True(userRole.IsDeleted);
        Assert.Equal("admin", userRole.DeletedBy);
        Assert.NotNull(userRole.DeletedDate);
    }
    
    [Fact]
    public async Task HasPermission_ShouldReturnTrueWhenUserHasPermission()
    {
        // Arrange
        var (tenant, userId, role, permission) = await CreateTestTenantWithUserAndPermissionAsync();
        
        // Make sure we're in the correct tenant context
        TenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        // Act
        var hasPermission = await PermissionService.HasPermissionAsync(userId, permission.SystemName);
        
        // Assert
        Assert.True(hasPermission);
    }
    
    [Fact]
    public async Task HasPermission_ShouldReturnFalseWhenUserDoesNotHavePermission()
    {
        // Arrange
        var (tenant, userId, _, _) = await CreateTestTenantWithUserAndPermissionAsync();
        
        // Make sure we're in the correct tenant context
        TenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        // Act
        var hasPermission = await PermissionService.HasPermissionAsync(userId, "NonExistentPermission");
        
        // Assert
        Assert.False(hasPermission);
    }
    
    [Fact]
    public async Task HasAnyPermission_ShouldReturnTrueWhenUserHasAnyPermission()
    {
        // Arrange
        var (tenant, userId, _, permission) = await CreateTestTenantWithUserAndPermissionAsync();
        
        // Make sure we're in the correct tenant context
        TenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        var permissionNames = new[] { permission.SystemName, "NonExistentPermission" };
        
        // Act
        var hasAnyPermission = await PermissionService.HasAnyPermissionAsync(userId, permissionNames);
        
        // Assert
        Assert.True(hasAnyPermission);
    }
    
    [Fact]
    public async Task HasAnyPermission_ShouldReturnFalseWhenUserHasNoPermissions()
    {
        // Arrange
        var (tenant, userId, _, _) = await CreateTestTenantWithUserAndPermissionAsync();
        
        // Make sure we're in the correct tenant context
        TenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        var permissionNames = new[] { "NonExistentPermission1", "NonExistentPermission2" };
        
        // Act
        var hasAnyPermission = await PermissionService.HasAnyPermissionAsync(userId, permissionNames);
        
        // Assert
        Assert.False(hasAnyPermission);
    }
    
    [Fact]
    public async Task HasAllPermissions_ShouldReturnTrueWhenUserHasAllPermissions()
    {
        // Arrange
        var (tenant, userId, role, permission) = await CreateTestTenantWithUserAndPermissionAsync();
        
        // Ensure the tenant context is set
        TenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        // Create a second permission and assign to the same role
        var permission2 = await PermissionService.CreatePermissionAsync(
            "Edit Product",
            "Products.Edit",
            "Permission to edit products",
            "Products",
            "system");
        await PermissionService.AssignPermissionToRoleAsync(role.Id, permission2.Id, "system");
        
        var permissionNames = new[] { permission.SystemName, permission2.SystemName };
        
        // Act
        var hasAllPermissions = await PermissionService.HasAllPermissionsAsync(userId, permissionNames);
        
        // Assert
        Assert.True(hasAllPermissions);
    }
    
    [Fact]
    public async Task HasAllPermissions_ShouldReturnFalseWhenUserDoesNotHaveAllPermissions()
    {
        // Arrange
        var (tenant, userId, _, permission) = await CreateTestTenantWithUserAndPermissionAsync();
        
        // Ensure the tenant context is set
        TenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        var permissionNames = new[] { permission.SystemName, "NonExistentPermission" };
        
        // Act
        var hasAllPermissions = await PermissionService.HasAllPermissionsAsync(userId, permissionNames);
        
        // Assert
        Assert.False(hasAllPermissions);
    }
    
    [Fact]
    public async Task GetUserPermissions_ShouldReturnAllPermissionsForUser()
    {
        // Arrange
        var (tenant, userId, role, permission) = await CreateTestTenantWithUserAndPermissionAsync();
        
        // Ensure the tenant context is set
        TenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        // Create a second permission and assign to the same role
        var permission2 = await PermissionService.CreatePermissionAsync(
            "Edit Product",
            "Products.Edit",
            "Permission to edit products",
            "Products",
            "system");
        await PermissionService.AssignPermissionToRoleAsync(role.Id, permission2.Id, "system");
        
        // Act
        var permissions = await PermissionService.GetUserPermissionsAsync(userId);
        
        // Assert
        Assert.NotNull(permissions);
        Assert.Equal(2, permissions.Count());
        Assert.Contains(permissions, p => p.SystemName == permission.SystemName);
        Assert.Contains(permissions, p => p.SystemName == permission2.SystemName);
    }
    
    [Fact]
    public async Task GetUserRoles_ShouldReturnAllRolesForUser()
    {
        // Arrange
        var (tenant, userId, role, _) = await CreateTestTenantWithUserAndPermissionAsync();
        
        // Ensure the tenant context is set
        TenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        // Create a second role and assign to the same user
        var role2 = await PermissionService.CreateRoleAsync("Editor", "Editor role", false, "system");
        await PermissionService.AssignRoleToUserAsync(userId, role2.Id, "system");
        
        // Act
        var roles = await PermissionService.GetUserRolesAsync(userId);
        
        // Assert
        Assert.NotNull(roles);
        Assert.Equal(2, roles.Count());
        Assert.Contains(roles, r => r.Id == role.Id);
        Assert.Contains(roles, r => r.Id == role2.Id);
    }
    
    [Fact]
    public async Task Permission_ShouldBeIsolatedByTenant()
    {
        // Arrange
        // Create first tenant with a role and permission
        var (tenant1, userId1, role1, permission1) = await CreateTestTenantWithUserAndPermissionAsync(
            name: "Tenant 1", 
            identifier: "tenant1",
            permissionSystemName: "Test.Permission1");
        
        // Create second tenant with a role and permission
        var (tenant2, userId2, role2, permission2) = await CreateTestTenantWithUserAndPermissionAsync(
            name: "Tenant 2", 
            identifier: "tenant2",
            permissionSystemName: "Test.Permission2");
        
        // Act & Assert for tenant 1
        TenantContextAccessor.SetCurrentTenantId(tenant1.Id);
        var hasPermission1InTenant1 = await PermissionService.HasPermissionAsync(userId1, permission1.SystemName);
        var hasPermission2InTenant1 = await PermissionService.HasPermissionAsync(userId1, permission2.SystemName);
        
        Assert.True(hasPermission1InTenant1);
        Assert.False(hasPermission2InTenant1);
        
        // Act & Assert for tenant 2
        TenantContextAccessor.SetCurrentTenantId(tenant2.Id);
        var hasPermission1InTenant2 = await PermissionService.HasPermissionAsync(userId2, permission1.SystemName);
        var hasPermission2InTenant2 = await PermissionService.HasPermissionAsync(userId2, permission2.SystemName);
        
        Assert.False(hasPermission1InTenant2);
        Assert.True(hasPermission2InTenant2);
    }
} 