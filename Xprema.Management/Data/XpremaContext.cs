 
using Microsoft.EntityFrameworkCore;

public class XpremaContext : DbContext
{
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Action> Actions { get; set; }
    public DbSet<Task> Tasks { get; set; }
    public DbSet<Process> Processes { get; set; }
    public DbSet<Report> Reports { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("YourConnectionStringHere");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(em =>
        {
            em.HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentID);
            
           em.HasOne(e => e.UserAccount)
               .WithOne()
               .HasForeignKey<Employee>(e => e.UserAccountID); 
        });
       
          

        modelBuilder.Entity<Action>()
            .HasOne(a => a.Task)
            .WithMany(t => t.Actions)
            .HasForeignKey(a => a.TaskID);

        modelBuilder.Entity<Action>()
            .HasOne(a => a.Employee)
            .WithMany(e => e.Actions)
            .HasForeignKey(a => a.EmployeeID);

        modelBuilder.Entity<Action>()
            .HasOne(a => a.ForwardToEmployee)
            .WithMany()
            .HasForeignKey(a => a.ForwardToEmployeeID);

        modelBuilder.Entity<Task>()
            .HasOne(t => t.AssignedEmployee)
            .WithMany(e => e.Tasks)
            .HasForeignKey(t => t.AssignedEmployeeID);

        modelBuilder.Entity<Report>()
            .HasOne(r => r.Employee)
            .WithMany()
            .HasForeignKey(r => r.EmployeeID);
    }
}
