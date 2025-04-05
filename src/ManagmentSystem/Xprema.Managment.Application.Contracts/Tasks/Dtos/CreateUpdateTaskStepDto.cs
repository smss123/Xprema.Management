using System;
using System.ComponentModel.DataAnnotations;
using Xprema.Managment.Domain.ProcedureArea;

namespace Xprema.Managment.Application.Contracts.Tasks.Dtos;

/// <summary>
/// DTO for creating or updating task steps
/// </summary>
public class CreateUpdateTaskStepDto
{
    public Guid? Id { get; set; } // Optional for updates, null for new steps
    
    public Guid TaskId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int StepNumber { get; set; }
    
    [Required]
    [StringLength(256, MinimumLength = 3)]
    public string StepName { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    public DateTime? DueDate { get; set; }
    
    public StepType Status { get; set; }
    
    // Assignment info
    public bool IsDepartmentAssigned { get; set; }
    
    public Guid? DepartmentId { get; set; }
    
    public Guid? EmployeeId { get; set; }
    
    // Step connections
    public Guid? PreviousStepId { get; set; }
    
    public Guid? NextStepId { get; set; }
    
    // Additional info
    [StringLength(1000)]
    public string? Notes { get; set; }
} 