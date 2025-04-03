using System.Collections.Generic;

public class Task
{
    public int TaskID { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public int AssignedEmployeeID { get; set; }
    public Employee AssignedEmployee { get; set; }
    public ICollection<Action> Actions { get; set; }
}
