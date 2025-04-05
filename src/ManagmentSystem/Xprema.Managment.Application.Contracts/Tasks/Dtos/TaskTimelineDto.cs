using System;
using Xprema.Managment.Domain.ProcedureArea;
using Xprema.Managment.Domain.TaskArea;

namespace Xprema.Managment.Application.Contracts.Tasks.Dtos;

/// <summary>
/// DTO for task timeline events
/// </summary>
public class TaskTimelineDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid? TaskStepId { get; set; }
    public DateTime EventDate { get; set; }
    public string EventDescription { get; set; } = string.Empty;
    public StepType? OldStatus { get; set; }
    public StepType? NewStatus { get; set; }
    public Guid? ChangedById { get; set; }
    public string? ChangedByName { get; set; }
    public string? Comments { get; set; }
    public TimelineEventType EventType { get; set; }
    
    // Additional fields
    public string? PropertyChanges { get; set; }
} 