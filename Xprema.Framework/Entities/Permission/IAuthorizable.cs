namespace Xprema.Framework.Entities.Permission;

/// <summary>
/// Interface for entities that can have roles and permissions
/// </summary>
public interface IAuthorizable
{
    Guid Id { get; }
    ICollection<UserRole> UserRoles { get; }
} 