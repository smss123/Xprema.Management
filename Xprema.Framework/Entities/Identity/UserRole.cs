using Xprema.Framework.Entities.Common;
using Xprema.Framework.Entities.MultiTenancy;
using Xprema.Framework.Entities.Permission;

namespace Xprema.Framework.Entities.Identity;

// Renamed to avoid conflict with Permission.UserRole
public class UserRoleIdentity : BaseEntity<Guid>, ITenantEntity
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public Guid TenantId { get; set; }
    
    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
    public virtual Tenant? Tenant { get; set; }
} 