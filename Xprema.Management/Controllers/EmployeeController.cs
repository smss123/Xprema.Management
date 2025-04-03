using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly EmployeeService _employeeService;

    public EmployeeController(EmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public IEnumerable<Employee> Get()
    {
        return _employeeService.GetAllEmployees();
    }

    [HttpGet("{id}")]
    public Employee Get(int id)
    {
        return _employeeService.GetEmployeeById(id);
    }

    [HttpPost]
    public void Post([FromBody] Employee employee)
    {
        _employeeService.AddEmployee(employee);
    }

    [HttpPut("{id}")]
    public void Put(int id, [FromBody] Employee employee)
    {
        employee.EmployeeID = id;
        _employeeService.UpdateEmployee(employee);
    }

    [HttpDelete("{id}")]
    public void Delete(int id)
    {
        _employeeService.DeleteEmployee(id);
    }
}
