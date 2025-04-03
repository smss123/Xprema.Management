using Microsoft.EntityFrameworkCore;

namespace Xprema.EntityFrameworkCore.Migrations;

/// <summary>
/// Generic migrations DbContext that can be used with any model
/// </summary>
/// <typeparam name="TImplementation">The specific implementation type of the context</typeparam>
public abstract class XpremaMigrationsDbContext<TImplementation> : XpremaDbContextBase<TImplementation>
    where TImplementation : DbContext
{
    protected XpremaMigrationsDbContext(DbContextOptions<TImplementation> options) 
        : base(options)
    {
    }
}

/// <summary>
/// Default implementation of XpremaMigrationsDbContext for the Xprema Management application
/// This can be replaced with a custom implementation for other applications
/// </summary>
public class XpremaManagementMigrationsDbContext : XpremaMigrationsDbContext<XpremaManagementMigrationsDbContext>
{
    // Import all DbSets from the original context
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<Action> Actions { get; set; } = null!;
    public DbSet<Task> Tasks { get; set; } = null!;
    public DbSet<Process> Processes { get; set; } = null!;
    public DbSet<Report> Reports { get; set; } = null!;

    public XpremaManagementMigrationsDbContext(DbContextOptions<XpremaManagementMigrationsDbContext> options) 
        : base(options)
    {
    }

    public override void ConfigureModel(ModelBuilder modelBuilder)
    {
        // Copy the entity configurations from the original DbContext
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