using System;
using System.Collections.Generic;

namespace Xprema.Managment.Application.Contracts.Procedures.Dtos;

/// <summary>
/// Data Transfer Object for FlowProcedureCompose
/// </summary>
public class FlowProcedureComposeDto
{
    public Guid Id { get; set; }
    public string ProcedureComposeName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    
    // Navigation properties
    public List<FlowProcedureStepDto> FlowProcedureSteps { get; set; } = new List<FlowProcedureStepDto>();
} 