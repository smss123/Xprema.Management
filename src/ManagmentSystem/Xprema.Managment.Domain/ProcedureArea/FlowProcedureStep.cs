using Xprema.Framework.Entities.Common;
using Xprema.Managment.Domain.ActionArea;

namespace Xprema.Managment.Domain.ProcedureArea;

public class FlowProcedureStep:BaseEntity<Guid>
{
    public Guid? ComposeId { get; set; }
    public Guid? ProcedureId { get; set; }
    public Guid? ActionId { get; set; }
    public int Step { get; set; }
    public FlowProcedureCompose? ProcedureCompose { get; set; }
    public FlowProcedure? FlowProcedure { get; set; }
    public FlowAction? Action { get; set; }
    public bool? IsDepartment { get; set; }
    public bool? IsSystem { get; set; }
    public string? UserInfo { get; set; }

}
