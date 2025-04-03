using Xprema.Framework.Entities.Common;
using Xprema.Framework.Entities.MultiTenancy;

namespace Xprema.Framework.Entities.Permission;

public class Permission : BaseEntity<Guid>, ITenantEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string SystemName { get; set; }
    public string? Group { get; set; }
    public Guid TenantId { get; set; }
    
    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
} 