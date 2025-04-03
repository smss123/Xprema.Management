using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

public class EmployeeService
{
    private readonly XpremaContext _context;

    public EmployeeService(XpremaContext context)
    {
        _context = context;
    }

    public IEnumerable<Employee> GetAllEmployees()
    {
        return _context.Employees.Include(e => e.Department).ToList();
    }

    public Employee GetEmployeeById(int id)
    {
        return _context.Employees.Include(e => e.Department).FirstOrDefault(e => e.EmployeeID == id);
    }

    public void AddEmployee(Employee employee)
    {
        _context.Employees.Add(employee);
        _context.SaveChanges();
    }

    public void UpdateEmployee(Employee employee)
    {
        _context.Employees.Update(employee);
        _context.SaveChanges();
    }

    public void DeleteEmployee(int id)
    {
        var employee = _context.Employees.Find(id);
        if (employee != null)
        {
            _context.Employees.Remove(employee);
            _context.SaveChanges();
        }
    }
}
