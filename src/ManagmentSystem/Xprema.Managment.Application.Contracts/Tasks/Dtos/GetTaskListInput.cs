using System;
using Xprema.Managment.Domain.ProcedureArea;

namespace Xprema.Managment.Application.Contracts.Tasks.Dtos;

/// <summary>
/// Input DTO for getting a paged list of Tasks
/// </summary>
public class GetTaskListInput
{
    /// <summary>
    /// Max result count for a page
    /// </summary>
    public int MaxResultCount { get; set; } = 10;
    
    /// <summary>
    /// Skip count for paging
    /// </summary>
    public int SkipCount { get; set; } = 0;
    
    /// <summary>
    /// Optional filter for task name or description
    /// </summary>
    public string? Filter { get; set; }
    
    /// <summary>
    /// Filter for procedure id
    /// </summary>
    public Guid? ProcedureId { get; set; }
    
    /// <summary>
    /// Filter for assigned user id
    /// </summary>
    public Guid? AssignedToId { get; set; }
    
    /// <summary>
    /// Filter for status
    /// </summary>
    public StepType? Status { get; set; }
    
    /// <summary>
    /// Filter for high priority tasks
    /// </summary>
    public bool? IsHighPriority { get; set; }
    
    /// <summary>
    /// Filter for tasks due before this date
    /// </summary>
    public DateTime? DueBefore { get; set; }
    
    /// <summary>
    /// Filter for tasks created after this date
    /// </summary>
    public DateTime? CreatedAfter { get; set; }
    
    /// <summary>
    /// Sorting string
    /// </summary>
    public string Sorting { get; set; } = "CreatedDate";
    
    /// <summary>
    /// Include details (steps and timeline)
    /// </summary>
    public bool IncludeDetails { get; set; } = false;
} 