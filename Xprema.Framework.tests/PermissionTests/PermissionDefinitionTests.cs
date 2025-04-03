using System.Linq;
using Xprema.Framework.Entities.Permission;
using Xunit;

namespace Xprema.Framework.Tests.PermissionTests;

public class PermissionDefinitionTests
{
    [Fact]
    public void PermissionDefinition_Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var name = "Test.Permission";
        var displayName = "Test Permission";
        var description = "Test permission description";
        
        // Act
        var permission = new PermissionDefinition(name, displayName, description);
        
        // Assert
        Assert.Equal(name, permission.Name);
        Assert.Equal(displayName, permission.DisplayName);
        Assert.Equal(description, permission.Description);
        Assert.Null(permission.Parent);
        Assert.Empty(permission.Children);
        Assert.False(permission.IsEnabledByDefault);
    }
    
    [Fact]
    public void PermissionDefinition_WithParent_ShouldSetParentAndAddToChildren()
    {
        // Arrange
        var parentName = "Parent.Permission";
        var parentDisplayName = "Parent Permission";
        var childName = "Child.Permission";
        var childDisplayName = "Child Permission";
        
        // Act
        var parent = new PermissionDefinition(parentName, parentDisplayName);
        var child = new PermissionDefinition(childName, childDisplayName, parent: parent);
        
        // Assert
        Assert.Same(parent, child.Parent);
        Assert.Contains(child, parent.Children);
        Assert.Single(parent.Children);
    }
    
    [Fact]
    public void PermissionDefinition_AddChild_ShouldCreateChildWithCorrectProperties()
    {
        // Arrange
        var parentName = "Parent.Permission";
        var parentDisplayName = "Parent Permission";
        var childName = "ChildName";
        var childDisplayName = "Child Display Name";
        var childDescription = "Child Description";
        
        var parent = new PermissionDefinition(parentName, parentDisplayName);
        
        // Act
        var child = parent.AddChild(childName, childDisplayName, childDescription);
        
        // Assert
        Assert.Equal(childName, child.Name);
        Assert.Equal(childDisplayName, child.DisplayName);
        Assert.Equal(childDescription, child.Description);
        Assert.Same(parent, child.Parent);
        Assert.Contains(child, parent.Children);
        Assert.Single(parent.Children);
    }
    
    [Fact]
    public void PermissionDefinition_AddMultipleChildren_ShouldAddToCollection()
    {
        // Arrange
        var parent = new PermissionDefinition("Parent", "Parent");
        
        // Act
        var child1 = parent.AddChild("Child1", "Child 1");
        var child2 = parent.AddChild("Child2", "Child 2");
        var child3 = parent.AddChild("Child3", "Child 3");
        
        // Assert
        Assert.Equal(3, parent.Children.Count);
        Assert.Contains(child1, parent.Children);
        Assert.Contains(child2, parent.Children);
        Assert.Contains(child3, parent.Children);
    }
    
    [Fact]
    public void PermissionDefinition_NestingChildren_ShouldCreateHierarchy()
    {
        // Arrange
        var root = new PermissionDefinition("Root", "Root Permission");
        
        // Act
        var level1Child = root.AddChild("Level1", "Level 1");
        var level2Child = level1Child.AddChild("Level2", "Level 2");
        var level3Child = level2Child.AddChild("Level3", "Level 3");
        
        // Assert
        Assert.Same(root, level1Child.Parent);
        Assert.Same(level1Child, level2Child.Parent);
        Assert.Same(level2Child, level3Child.Parent);
        
        Assert.Contains(level1Child, root.Children);
        Assert.Contains(level2Child, level1Child.Children);
        Assert.Contains(level3Child, level2Child.Children);
        
        Assert.Empty(level3Child.Children);
    }
    
    [Fact]
    public void PermissionDefinition_IsEnabledByDefault_ShouldBeConfigurable()
    {
        // Arrange & Act
        var enabledPermission = new PermissionDefinition("Enabled", "Enabled")
        {
            IsEnabledByDefault = true
        };
        
        var disabledPermission = new PermissionDefinition("Disabled", "Disabled")
        {
            IsEnabledByDefault = false
        };
        
        // Assert
        Assert.True(enabledPermission.IsEnabledByDefault);
        Assert.False(disabledPermission.IsEnabledByDefault);
    }
} 