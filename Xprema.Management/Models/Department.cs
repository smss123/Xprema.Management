using System.Collections.Generic;

public class Department
{
    public int DepartmentID { get; set; }
    public string Name { get; set; }
    public ICollection<Employee> Employees { get; set; }
}
