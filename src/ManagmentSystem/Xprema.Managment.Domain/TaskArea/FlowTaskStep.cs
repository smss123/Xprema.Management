using System;
using Xprema.Framework.Entities.Common;
using Xprema.Managment.Domain.ProcedureArea;

namespace Xprema.Managment.Domain.TaskArea;

public class FlowTaskStep : BaseEntity<Guid>
{
    public Guid TaskId { get; set; }
    public int StepNumber { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public StepType Status { get; set; } = StepType.Initialized;
    
    // Assignment can be to Department or Employee
    public bool IsDepartmentAssigned { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? EmployeeId { get; set; }
    
    // Relations
    public virtual FlowTask Task { get; set; } = null!;
    
    // Previous and next steps for timeline
    public Guid? PreviousStepId { get; set; }
    public Guid? NextStepId { get; set; }
    public virtual FlowTaskStep? PreviousStep { get; set; }
    public virtual FlowTaskStep? NextStep { get; set; }
    
    // Comments and notes
    public string? Notes { get; set; }
} 