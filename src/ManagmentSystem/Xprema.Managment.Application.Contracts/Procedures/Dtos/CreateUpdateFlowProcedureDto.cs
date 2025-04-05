using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Xprema.Managment.Application.Contracts.Procedures.Dtos;

/// <summary>
/// DTO for creating or updating a FlowProcedure
/// </summary>
public class CreateUpdateFlowProcedureDto
{
    [Required]
    [StringLength(256, MinimumLength = 3)]
    public string ProcedureName { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    public string? Icon { get; set; }
    
    public bool IsSystem { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Concurrency token
    public string? ConcurrencyStamp { get; set; }
    
    // Extra properties for dynamic extensions
    public Dictionary<string, object> ExtraProperties { get; set; } = new Dictionary<string, object>();
    
    // Steps to be created or updated with this procedure
    public List<CreateUpdateFlowProcedureStepDto>? Steps { get; set; }
} 