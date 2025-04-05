using System;

namespace Xprema.Managment.Application.Contracts.Procedures.Dtos;

/// <summary>
/// Input DTO for getting a paged list of FlowProcedures
/// </summary>
public class GetFlowProcedureListInput
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
    /// Optional filter for procedure name
    /// </summary>
    public string? Filter { get; set; }
    
    /// <summary>
    /// Filter for IsActive property
    /// </summary>
    public bool? IsActive { get; set; }
    
    /// <summary>
    /// Filter for IsSystem property
    /// </summary>
    public bool? IsSystem { get; set; }
    
    /// <summary>
    /// Sorting string
    /// </summary>
    public string Sorting { get; set; } = "ProcedureName";
    
    /// <summary>
    /// Include details (steps)
    /// </summary>
    public bool IncludeDetails { get; set; } = false;
} 