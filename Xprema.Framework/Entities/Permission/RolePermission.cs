using Xprema.Framework.Entities.Common;
using Xprema.Framework.Entities.MultiTenancy;

namespace Xprema.Framework.Entities.Permission;

public class RolePermission : BaseEntity<Guid>, ITenantEntity
{
    public required Guid RoleId { get; set; }
    public required Guid PermissionId { get; set; }
    public Guid TenantId { get; set; }
    
    // Navigation properties
    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
} 