using System;
using Xprema.Framework.Entities.Common;
using Xprema.Managment.Domain.ProcedureArea;

namespace Xprema.Managment.Domain.TaskArea;

public class TaskTimeline : BaseEntity<Guid>
{
    public Guid TaskId { get; set; }
    public Guid? TaskStepId { get; set; }
    public DateTime EventDate { get; set; } = DateTime.Now;
    public string EventDescription { get; set; } = string.Empty;
    public StepType? OldStatus { get; set; }
    public StepType? NewStatus { get; set; }
    public Guid? ChangedById { get; set; }
    public string? Comments { get; set; }
    public TimelineEventType EventType { get; set; }
    
    // ABP-inspired auditing fields
    public DateTime CreationTime { get; set; } = DateTime.Now;
    public Guid? CreatorId { get; set; }
    public Guid? TenantId { get; set; }
    
    // Serialized changes for audit logging
    public string? PropertyChanges { get; set; }
    
    // Relations
    public virtual FlowTask Task { get; set; } = null!;
    public virtual FlowTaskStep? TaskStep { get; set; }
}

public enum TimelineEventType
{
    TaskCreated = 1,
    TaskStarted = 2,
    StepStarted = 3,
    StepCompleted = 4,
    StatusChanged = 5,
    AssigneeChanged = 6,
    DueDateChanged = 7,
    TaskCompleted = 8,
    TaskCancelled = 9,
    CommentAdded = 10,
    DocumentAttached = 11,
    
    // Additional event types
    PropertyChanged = 12,
    ReminderSet = 13,
    ReminderTriggered = 14,
    TaskReassigned = 15,
    StepReassigned = 16,
    PriorityChanged = 17,
    TaskReactivated = 18,
    WorkflowDeviated = 19,
    CustomEvent = 20
} 