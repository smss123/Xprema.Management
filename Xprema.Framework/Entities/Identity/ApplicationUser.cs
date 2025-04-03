using System.ComponentModel.DataAnnotations;
using Xprema.Framework.Entities.Common;
using Xprema.Framework.Entities.MultiTenancy;
using Xprema.Framework.Entities.Permission;

namespace Xprema.Framework.Entities.Identity;

public class ApplicationUser : BaseEntity<Guid>
{
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = null!;
    
    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = null!;
    
    [MaxLength(250)]
    public string? PasswordHash { get; set; }
    
    [MaxLength(256)]
    public string? DisplayName { get; set; }
    
    public bool EmailConfirmed { get; set; }
    
    public bool TwoFactorEnabled { get; set; }
    
    public DateTime? LastLoginDate { get; set; }
    
    [MaxLength(100)]
    public string? PhoneNumber { get; set; }
    
    public bool PhoneNumberConfirmed { get; set; }
    
    public bool IsLockedOut { get; set; }
    
    public DateTime? LockoutEnd { get; set; }
    
    public int AccessFailedCount { get; set; }
    
    [MaxLength(36)]
    public string? SecurityStamp { get; set; }
    
    [MaxLength(100)]
    public string? TwoFactorKey { get; set; }
    
    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<TenantUser> TenantUsers { get; set; } = new List<TenantUser>();
    public virtual ICollection<UserToken> UserTokens { get; set; } = new List<UserToken>();
} 