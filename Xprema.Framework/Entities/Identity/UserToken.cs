using System.ComponentModel.DataAnnotations;
using Xprema.Framework.Entities.Common;
using Xprema.Framework.Entities.MultiTenancy;

namespace Xprema.Framework.Entities.Identity;

public class UserToken : BaseEntity<Guid>, ITenantEntity
{
    public Guid UserId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string TokenType { get; set; } = null!;
    
    [Required]
    [MaxLength(500)]
    public string TokenValue { get; set; } = null!;
    
    public DateTime? ExpirationDate { get; set; }
    
    public bool IsUsed { get; set; }
    
    public Guid TenantId { get; set; }
    
    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Tenant? Tenant { get; set; }
} 