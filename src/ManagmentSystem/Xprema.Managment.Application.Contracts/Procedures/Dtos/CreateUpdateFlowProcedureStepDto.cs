using System;
using System.ComponentModel.DataAnnotations;

namespace Xprema.Managment.Application.Contracts.Procedures.Dtos;

/// <summary>
/// DTO for creating or updating a FlowProcedureStep
/// </summary>
public class CreateUpdateFlowProcedureStepDto
{
    public Guid? Id { get; set; } // Optional for updates, null for new steps
    
    public Guid? ComposeId { get; set; }
    
    public Guid? ProcedureId { get; set; }
    
    public Guid? ActionId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int Step { get; set; }
    
    public bool? IsDepartment { get; set; }
    
    public bool? IsSystem { get; set; }
    
    public string? UserInfo { get; set; }
} 