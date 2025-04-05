using System;

namespace Xprema.Managment.Application.Contracts.Procedures.Dtos;

/// <summary>
/// Data Transfer Object for FlowProcedureStep
/// </summary>
public class FlowProcedureStepDto
{
    public Guid Id { get; set; }
    public Guid? ComposeId { get; set; }
    public Guid? ProcedureId { get; set; }
    public Guid? ActionId { get; set; }
    public int Step { get; set; }
    public bool? IsDepartment { get; set; }
    public bool? IsSystem { get; set; }
    public string? UserInfo { get; set; }
    
    // Navigation property names for reference
    public string? ProcedureComposeName { get; set; }
    public string? FlowProcedureName { get; set; }
    public string? ActionName { get; set; }
} 