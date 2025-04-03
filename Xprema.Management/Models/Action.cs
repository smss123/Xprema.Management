public class Action
{
    public int ActionID { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    public int TaskID { get; set; }
    public Task Task { get; set; }
    public int EmployeeID { get; set; }
    public Employee Employee { get; set; }
    public bool SeenFlag { get; set; }
    public int? ForwardToEmployeeID { get; set; }
    public Employee ForwardToEmployee { get; set; }
}
