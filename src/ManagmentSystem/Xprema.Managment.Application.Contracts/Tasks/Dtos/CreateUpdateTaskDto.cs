using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xprema.Managment.Domain.ProcedureArea;

namespace Xprema.Managment.Application.Contracts.Tasks.Dtos;

/// <summary>
/// DTO for creating or updating a Task
/// </summary>
public class CreateUpdateTaskDto
{
    [Required]
    [StringLength(256, MinimumLength = 3)]
    public string TaskName { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    public DateTime? DueDate { get; set; }
    
    public Guid? AssignedToId { get; set; }
    
    public Guid? ProcedureId { get; set; }
    
    public Guid? ProcedureStepId { get; set; }
    
    public StepType Status { get; set; }
    
    public string? Notes { get; set; }
    
    public bool IsHighPriority { get; set; }
    
    // Participants to add to the task
    public List<TaskParticipantDto>? Participants { get; set; }
    
    // Steps to create with the task
    public List<CreateUpdateTaskStepDto>? Steps { get; set; }
} 