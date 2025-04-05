using Xprema.Framework.Entities.Common;

namespace Xprema.Managment.Domain.EmployeeArea;

public class Employee:BaseEntity<Guid>
{
    public string? Name { get; set; }
}
