using Xprema.Framework.Entities.Common;

namespace Xprema.Framework.Entities.MultiTenancy;

public class TenantUser : BaseEntity<Guid>
{
    public required Guid TenantId { get; set; }
    public required Guid UserId { get; set; }
    public bool IsAdmin { get; set; } = false;
    
    // Navigation properties
    public Tenant Tenant { get; set; } = null!;
} 