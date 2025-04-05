using System;
using Xprema.Framework.Entities.Common;
using Xprema.Managment.Domain.ProcedureArea;

namespace Xprema.Managment.Domain.TaskArea;

public class FlowTask : BaseEntity<Guid>
{
    public required string TaskName { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? DueDate { get; set; }
    public Guid? AssignedToId { get; set; }
    public Guid? ProcedureId { get; set; }
    public Guid? ProcedureStepId { get; set; }
    public StepType Status { get; set; } = StepType.Initialized;
    public string? Notes { get; set; }
    public bool IsHighPriority { get; set; }
    public virtual FlowProcedure? Procedure { get; set; }
    public virtual FlowProcedureStep? ProcedureStep { get; set; }
    public virtual ICollection<TaskParticipant> Participants { get; set; } = new List<TaskParticipant>();
    public virtual ICollection<FlowTaskStep> Steps { get; set; } = new List<FlowTaskStep>();
    public virtual ICollection<TaskTimeline> Timeline { get; set; } = new List<TaskTimeline>();
    
    // Timeline tracking
    public DateTime? StartDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int CurrentStepNumber { get; set; } = 0;
}

public class TaskParticipant : BaseEntity<Guid>
{
    public Guid TaskId { get; set; }
    public Guid ParticipantId { get; set; }
    public string? Role { get; set; }
    public DateTime JoinedDate { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;
    public virtual FlowTask Task { get; set; } = null!;
}
