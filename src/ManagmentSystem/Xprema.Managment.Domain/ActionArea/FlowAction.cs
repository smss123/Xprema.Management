using System;
using Xprema.Framework.Entities.Common;

namespace Xprema.Managment.Domain.ActionArea;

public class FlowAction:BaseEntity<Guid>
{    public required string ActionName { get; set; }
    public ActionType ActionType { get; set; }
    public string? Description { get; set; }
public string? Icon { get; set; }
}
