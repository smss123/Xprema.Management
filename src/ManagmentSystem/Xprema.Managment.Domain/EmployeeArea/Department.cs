using System;
using Xprema.Framework.Entities.Common;

namespace Xprema.Managment.Domain.EmployeeArea;

public class Department:BaseEntity<Guid>
{
    public string Name { get; set; }
    public string Description { get; set; }
  
  public ICollection<Employee> Employees { get; set; }
}
