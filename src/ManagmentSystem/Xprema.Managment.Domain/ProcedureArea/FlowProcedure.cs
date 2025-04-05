using System;
using System.Collections.Generic;
using Xprema.Framework.Entities.Common;
using Xprema.Managment.Domain.ActionArea;
using Xprema.Managment.Domain.TaskArea;

namespace Xprema.Managment.Domain.ProcedureArea;

/// <summary>
///    /// Represents a flow procedure in the system.
/// </summary>
public class FlowProcedure:BaseEntity<Guid>
{
    public required string ProcedureName { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public bool IsSystem { get; set; }
    
    // Added ABP-inspired fields
    public bool IsActive { get; set; } = true;
    public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreationTime { get; set; } = DateTime.Now;
    public Guid? CreatorId { get; set; }
    public DateTime? LastModificationTime { get; set; }
    public Guid? LastModifierId { get; set; }
    public bool IsDeleted { get; set; }
    public Guid? DeleterId { get; set; }
    public DateTime? DeletionTime { get; set; }
    
    // Multi-tenancy support
    public Guid? TenantId { get; set; }
    
    // Navigation properties
    public virtual ICollection<FlowProcedureStep> Steps { get; set; } = new List<FlowProcedureStep>();
    public virtual ICollection<FlowTask> Tasks { get; set; } = new List<FlowTask>();
}

