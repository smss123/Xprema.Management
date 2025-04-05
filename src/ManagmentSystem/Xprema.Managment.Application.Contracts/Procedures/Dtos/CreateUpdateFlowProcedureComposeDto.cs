using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Xprema.Managment.Application.Contracts.Procedures.Dtos;

/// <summary>
/// DTO for creating or updating a FlowProcedureCompose
/// </summary>
public class CreateUpdateFlowProcedureComposeDto
{
    [Required]
    [StringLength(256, MinimumLength = 3)]
    public string ProcedureComposeName { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    public string? Icon { get; set; }
    
    // Steps to be created or updated with this compose
    public List<CreateUpdateFlowProcedureStepDto>? FlowProcedureSteps { get; set; }
} 