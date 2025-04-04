using System;
using Xprema.Framework.Entities.Common;

namespace Xprema.Managment.Domain.ProcedureArea;

public class FlowProcedureCompose:BaseEntity<Guid>
{
    public required string ProcedureComposeName { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
     
    
}
