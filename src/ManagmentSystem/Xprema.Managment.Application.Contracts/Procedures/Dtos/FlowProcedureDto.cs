using System;
using System.Collections.Generic;

namespace Xprema.Managment.Application.Contracts.Procedures.Dtos;

/// <summary>
/// Data Transfer Object for FlowProcedure
/// </summary>
public class FlowProcedureDto
{
    public Guid Id { get; set; }
    public string ProcedureName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; }
    
    public DateTime CreationTime { get; set; }
    public Guid? CreatorId { get; set; }
    public DateTime? LastModificationTime { get; set; }
    public Guid? LastModifierId { get; set; }
    
    // Extra properties for dynamic extensions
    public Dictionary<string, object> ExtraProperties { get; set; } = new Dictionary<string, object>();
    
    // Navigation properties
    public List<FlowProcedureStepDto> Steps { get; set; } = new List<FlowProcedureStepDto>();
} 