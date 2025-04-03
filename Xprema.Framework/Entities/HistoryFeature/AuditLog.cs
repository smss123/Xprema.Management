using System.ComponentModel.DataAnnotations;
using Xprema.Framework.Entities.Common;
using Xprema.Framework.Entities.MultiTenancy;

namespace Xprema.Framework.Entities.HistoryFeature;

/// <summary>
/// Represents an audit log entry
/// </summary>
public class AuditLog : BaseEntity<Guid>, ITenantEntity
{
    [Required]
    [MaxLength(50)]
    public string UserId { get; set; } = null!;
    
    [Required]
    [MaxLength(200)]
    public string Activity { get; set; } = null!;
    
    [MaxLength(100)]
    public string? EntityType { get; set; }
    
    [MaxLength(50)]
    public string? EntityId { get; set; }
    
    public string? OldValues { get; set; }
    
    public string? NewValues { get; set; }
    
    public DateTime Timestamp { get; set; }
    
    public Guid TenantId { get; set; }
} 