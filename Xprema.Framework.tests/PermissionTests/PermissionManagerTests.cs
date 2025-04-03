using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xprema.Framework.Entities.Permission;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace Xprema.Framework.Tests.PermissionTests;

public class PermissionManagerTests : TestBase
{
    private readonly IPermissionManager _permissionManager;
    
    public PermissionManagerTests()
    {
        _permissionManager = ServiceProvider.GetRequiredService<IPermissionManager>();
    }
    
    [Fact]
    public void PermissionManager_AddPermission_ShouldAddPermissionToCollection()
    {
        // Arrange
        var name = "Test.Permission";
        var displayName = "Test Permission";
        var description = "Test permission description";
        var group = "TestGroup";
        
        // Act
        var permission = _permissionManager.AddPermission(name, displayName, description, group);
        
        // Assert
        Assert.NotNull(permission);
        Assert.Equal(name, permission.Name);
        Assert.Equal(displayName, permission.DisplayName);
        Assert.Equal(description, permission.Description);
        Assert.Equal(group, permission.Group);
        
        var retrievedPermission = _permissionManager.GetPermission(name);
        Assert.NotNull(retrievedPermission);
        Assert.Equal(permission, retrievedPermission);
    }
    
    [Fact]
    public void PermissionManager_AddDuplicatePermission_ShouldThrowException()
    {
        // Arrange
        var name = "Test.DuplicatePermission";
        var displayName = "Test Permission";
        
        // Act & Assert
        _permissionManager.AddPermission(name, displayName);
        Assert.Throws<InvalidOperationException>(() => 
            _permissionManager.AddPermission(name, "Different display name"));
    }
    
    [Fact]
    public void PermissionManager_RemovePermission_ShouldRemovePermissionFromCollection()
    {
        // Arrange
        var name = "Test.RemovePermission";
        var displayName = "Test Permission";
        _permissionManager.AddPermission(name, displayName);
        
        // Act
        _permissionManager.RemovePermission(name);
        
        // Assert
        var permission = _permissionManager.GetPermission(name);
        Assert.Null(permission);
    }
    
    [Fact]
    public void PermissionManager_RemoveNonExistingPermission_ShouldThrowException()
    {
        // Arrange
        var name = "Test.NonExistingPermission";
        
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _permissionManager.RemovePermission(name));
    }
    
    [Fact]
    public void PermissionManager_GetPermissionsByGroup_ShouldReturnGroupedPermissions()
    {
        // Arrange
        var group1 = "Group1";
        var group2 = "Group2";
        
        _permissionManager.AddPermission("Test.Group1.Permission1", "Permission 1", group: group1);
        _permissionManager.AddPermission("Test.Group1.Permission2", "Permission 2", group: group1);
        _permissionManager.AddPermission("Test.Group2.Permission1", "Permission 1", group: group2);
        
        // Act
        var group1Permissions = _permissionManager.GetPermissionsByGroup(group1);
        var group2Permissions = _permissionManager.GetPermissionsByGroup(group2);
        
        // Assert
        Assert.Equal(2, group1Permissions.Count);
        Assert.Single(group2Permissions);
        Assert.All(group1Permissions, p => Assert.Equal(group1, p.Group));
        Assert.All(group2Permissions, p => Assert.Equal(group2, p.Group));
    }
    
    [Fact]
    public async Task PermissionManager_EnsurePermission_ShouldCreateInDatabase()
    {
        // Arrange
        var name = "Test.EnsurePermission";
        var displayName = "Ensure Permission";
        var description = "Test permission description";
        var group = "TestGroup";
        
        var permissionDefinition = new PermissionDefinition(name, displayName, description)
        {
            Group = group
        };
        
        // Create a test tenant to provide tenant context
        var tenant = await CreateTestTenantAsync("Permission Test Tenant", "permission-test");
        
        // Set the tenant context - this is required for permissions to be created
        TenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        // Act
        await _permissionManager.EnsurePermissionAsync(permissionDefinition);
        
        // Assert - query the Permissions DbSet directly
        var dbPermission = await DbContext.Permissions
            .FirstOrDefaultAsync(p => p.SystemName == name && p.TenantId == tenant.Id);
        
        Assert.NotNull(dbPermission);
        Assert.Equal(displayName, dbPermission.Name);
        Assert.Equal(description, dbPermission.Description);
        Assert.Equal(group, dbPermission.Group);
    }
} 