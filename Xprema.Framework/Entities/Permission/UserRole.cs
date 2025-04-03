using Xprema.Framework.Entities.Common;
using Xprema.Framework.Entities.MultiTenancy;

namespace Xprema.Framework.Entities.Permission;

public class UserRole : BaseEntity<Guid>, ITenantEntity
{
    public required Guid UserId { get; set; }
    public required Guid RoleId { get; set; }
    public Guid TenantId { get; set; }
    
    // Navigation properties
    public Role Role { get; set; } = null!;
} 