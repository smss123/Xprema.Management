using Xprema.Framework.Bussiness.DataTypes;
using Xprema.Framework.Entities.Common;

namespace Xprema.Managment.Domain.EmployeeArea;

public class Employee:BaseEntity<Guid>
{
    public FullName? Name { get; set; }
    public EmailAddress? Email { get; set; }
    public PhoneNumber? PhoneNumber { get; set; }
    public ICollection<Department> Departments { get; set; }
     = new List<Department>();
}
