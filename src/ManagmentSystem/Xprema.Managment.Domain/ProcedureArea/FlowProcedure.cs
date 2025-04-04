using System;
using Xprema.Framework.Entities.Common;
using Xprema.Managment.Domain.ActionArea;

namespace Xprema.Managment.Domain.ProcedureArea;

/// <summary>
///    /// Represents a flow procedure in the system.
/// </summary>
public class FlowProcedure:BaseEntity<Guid>
{
    public required string ProcedureName { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public bool IsSystem { get; set; }
}
