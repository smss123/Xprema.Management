using System;
using Xprema.Managment.Domain.ProcedureArea;

namespace Xprema.Managment.Application.Contracts.Tasks.Dtos;

/// <summary>
/// DTO for task steps
/// </summary>
public class TaskStepDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public int StepNumber { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public StepType Status { get; set; }
    
    // Assignment info
    public bool IsDepartmentAssigned { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? EmployeeId { get; set; }
    public string? AssignedToName { get; set; }
    
    // Step connections
    public Guid? PreviousStepId { get; set; }
    public Guid? NextStepId { get; set; }
    
    // Additional info
    public string? Notes { get; set; }
} 