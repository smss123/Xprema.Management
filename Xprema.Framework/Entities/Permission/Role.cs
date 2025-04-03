using Xprema.Framework.Entities.Common;
using Xprema.Framework.Entities.MultiTenancy;

namespace Xprema.Framework.Entities.Permission;

public class Role : BaseEntity<Guid>, ITenantEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; } = false;
    public Guid TenantId { get; set; }
    
    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
} 