using System.Collections.Generic;

public class Employee
{
    public int EmployeeID { get; set; }
    public string Name { get; set; }
    public string PersonalInfo { get; set; }
    public int? DepartmentID { get; set; }
    public Department Department { get; set; }
    public int UserAccountID { get; set; }
    public UserAccount UserAccount { get; set; }
    public ICollection<Task> Tasks { get; set; }
    public ICollection<Action> Actions { get; set; }
}

public class UserAccount
{
}
