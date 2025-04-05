using System;
using System.Collections.Generic;
using Xprema.Managment.Domain.ProcedureArea;

namespace Xprema.Managment.Application.Contracts.Tasks.Dtos;

/// <summary>
/// DTO for Task entity
/// </summary>
public class TaskDto
{
    public Guid Id { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? AssignedToId { get; set; }
    public Guid? ProcedureId { get; set; }
    public Guid? ProcedureStepId { get; set; }
    public StepType Status { get; set; }
    public string? Notes { get; set; }
    public bool IsHighPriority { get; set; }
    
    // Navigation property names for reference
    public string? ProcedureName { get; set; }
    public string? AssignedToName { get; set; }
    
    // Timeline tracking
    public DateTime? StartDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int CurrentStepNumber { get; set; }
    
    // Collections
    public List<TaskStepDto> Steps { get; set; } = new List<TaskStepDto>();
    public List<TaskTimelineDto> Timeline { get; set; } = new List<TaskTimelineDto>();
    public List<TaskParticipantDto> Participants { get; set; } = new List<TaskParticipantDto>();
} 