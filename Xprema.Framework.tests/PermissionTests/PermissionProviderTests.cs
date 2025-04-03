using System;
using System.Collections.Generic;
using Moq;
using Xprema.Framework.Entities.Permission;
using Xunit;

namespace Xprema.Framework.Tests.PermissionTests;

public class PermissionProviderTests
{
    private class TestPermissionProvider : PermissionProvider
    {
        public Action<IPermissionDefinitionContext>? DefineAction { get; set; }
        
        public override void Define(IPermissionDefinitionContext context)
        {
            DefineAction?.Invoke(context);
        }
    }
    
    [Fact]
    public void PermissionDefinitionContext_AddGroup_ShouldCreateGroup()
    {
        // Arrange
        var permissionManagerMock = new Mock<IPermissionManager>();
        var context = new PermissionDefinitionContext(permissionManagerMock.Object);
        var groupName = "TestGroup";
        var groupDisplayName = "Test Group";
        
        // Act
        var group = context.AddGroup(groupName, groupDisplayName);
        
        // Assert
        Assert.Equal(groupName, group.Name);
        Assert.Equal(groupDisplayName, group.DisplayName);
    }
    
    [Fact]
    public void PermissionDefinitionContext_GetGroup_ShouldReturnExistingGroup()
    {
        // Arrange
        var permissionManagerMock = new Mock<IPermissionManager>();
        var context = new PermissionDefinitionContext(permissionManagerMock.Object);
        var groupName = "TestGroup";
        var groupDisplayName = "Test Group";
        
        var group = context.AddGroup(groupName, groupDisplayName);
        
        // Act
        var retrievedGroup = context.GetGroup(groupName);
        
        // Assert
        Assert.Same(group, retrievedGroup);
    }
    
    [Fact]
    public void PermissionDefinitionContext_GetNonExistingGroup_ShouldThrowException()
    {
        // Arrange
        var permissionManagerMock = new Mock<IPermissionManager>();
        var context = new PermissionDefinitionContext(permissionManagerMock.Object);
        
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => context.GetGroup("NonExistingGroup"));
    }
    
    [Fact]
    public void PermissionGroupDefinitionContext_AddPermission_ShouldCreatePermission()
    {
        // Arrange
        var permissionManagerMock = new Mock<IPermissionManager>();
        // Create a real PermissionDefinition instead of trying to mock it
        var permission = new PermissionDefinition("TestGroup.TestPermission", "Test Permission", "Test permission description");
        
        permissionManagerMock
            .Setup(m => m.AddPermission(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(permission);
            
        var context = new PermissionGroupDefinitionContext("TestGroup", "Test Group", permissionManagerMock.Object);
        
        // Act
        var result = context.AddPermission(
            "TestPermission",
            "Test Permission",
            "Test permission description");
            
        // Assert
        permissionManagerMock.Verify(m => m.AddPermission(
            "TestGroup.TestPermission",
            "Test Permission",
            "Test permission description",
            "TestGroup"), Times.Once);
            
        Assert.Same(permission, result);
    }
    
    [Fact]
    public void PermissionGroupDefinitionContext_GetPermission_ShouldReturnExistingPermission()
    {
        // Arrange
        var permissionManagerMock = new Mock<IPermissionManager>();
        var permissionName = "TestPermission";
        var fullPermissionName = "TestGroup.TestPermission";
        
        // Create a real PermissionDefinition instead of trying to mock it
        var permission = new PermissionDefinition(fullPermissionName, "Test Permission", "Test Description");
        
        permissionManagerMock
            .Setup(m => m.AddPermission(
                It.Is<string>(s => s == fullPermissionName),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(permission);
            
        var context = new PermissionGroupDefinitionContext("TestGroup", "Test Group", permissionManagerMock.Object);
        var addedPermission = context.AddPermission(permissionName, "Test Permission");
        
        // Add the permission to the internal dictionary directly
        var dictionary = new Dictionary<string, PermissionDefinition>
        {
            { fullPermissionName, addedPermission }
        };
        
        var contextType = typeof(PermissionGroupDefinitionContext);
        var permissionsField = contextType.GetField("_permissions", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (permissionsField != null)
        {
            permissionsField.SetValue(context, dictionary);
        }
        
        // Act
        var retrievedPermission = context.GetPermission(permissionName);
        
        // Assert
        Assert.Same(addedPermission, retrievedPermission);
    }
    
    [Fact]
    public void PermissionGroupDefinitionContext_GetNonExistingPermission_ShouldThrowException()
    {
        // Arrange
        var permissionManagerMock = new Mock<IPermissionManager>();
        var context = new PermissionGroupDefinitionContext("TestGroup", "Test Group", permissionManagerMock.Object);
        
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => context.GetPermission("NonExistingPermission"));
    }
    
    [Fact]
    public void TestPermissionProvider_Define_ShouldCallDefineAction()
    {
        // Arrange
        var provider = new TestPermissionProvider();
        var permissionManagerMock = new Mock<IPermissionManager>();
        var context = new PermissionDefinitionContext(permissionManagerMock.Object);
        
        bool defineActionCalled = false;
        provider.DefineAction = ctx => 
        {
            Assert.Same(context, ctx);
            defineActionCalled = true;
        };
        
        // Act
        provider.Define(context);
        
        // Assert
        Assert.True(defineActionCalled);
    }
} 