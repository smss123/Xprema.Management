namespace Xprema.Framework.Entities.Permission;

/// <summary>
/// Defines a permission in the system, similar to ABP's permission definition
/// </summary>
public class PermissionDefinition
{
    /// <summary>
    /// Gets the unique name of the permission
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the display name of the permission
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets or sets the description of the permission
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the group name of the permission
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    /// Gets or sets whether this permission is enabled by default
    /// </summary>
    public bool IsEnabledByDefault { get; set; }

    /// <summary>
    /// Gets the parent permission
    /// </summary>
    public PermissionDefinition? Parent { get; }

    /// <summary>
    /// Gets the list of child permissions
    /// </summary>
    public List<PermissionDefinition> Children { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionDefinition"/> class
    /// </summary>
    /// <param name="name">The unique name of the permission</param>
    /// <param name="displayName">The display name of the permission</param>
    /// <param name="description">The description of the permission</param>
    /// <param name="parent">The parent permission</param>
    public PermissionDefinition(
        string name,
        string displayName,
        string? description = null,
        PermissionDefinition? parent = null)
    {
        Name = name;
        DisplayName = displayName;
        Description = description;
        Parent = parent;
        Children = new List<PermissionDefinition>();

        parent?.Children.Add(this);
    }
    
    /// <summary>
    /// Adds a child permission
    /// </summary>
    /// <param name="name">The unique name of the child permission</param>
    /// <param name="displayName">The display name of the child permission</param>
    /// <param name="description">The description of the child permission</param>
    /// <returns>The newly created child permission definition</returns>
    public PermissionDefinition AddChild(
        string name,
        string displayName,
        string? description = null)
    {
        var child = new PermissionDefinition(
            name,
            displayName,
            description,
            this);
            
        return child;
    }
} 