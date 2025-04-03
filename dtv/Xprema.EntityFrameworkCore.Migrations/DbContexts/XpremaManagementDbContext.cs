using Microsoft.EntityFrameworkCore;

namespace Xprema.EntityFrameworkCore.Migrations.DbContexts;

/// <summary>
/// Module representing Xprema Management
/// </summary>
public class XpremaManagementModule
{
}

/// <summary>
/// DbContext for Xprema Management module.
/// Following ABP.io's approach with module-specific DbContexts.
/// </summary>
public class XpremaManagementDbContext : ModuleDbContextBase<XpremaManagementDbContext, XpremaManagementModule>
{
    /// <summary>
    /// DbSets for entities in the Xprema Management module
    /// </summary>
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<Action> Actions { get; set; } = null!;
    public DbSet<Task> Tasks { get; set; } = null!;
    public DbSet<Process> Processes { get; set; } = null!;
    public DbSet<Report> Reports { get; set; } = null!;

    public XpremaManagementDbContext(DbContextOptions<XpremaManagementDbContext> options)
        : base(options)
    {
    }

    public XpremaManagementDbContext(DbContextOptions<XpremaManagementDbContext> options, IServiceProvider serviceProvider)
        : base(options, serviceProvider)
    {
    }

    /// <summary>
    /// Configure the model specifically for the Xprema Management module
    /// </summary>
    /// <param name="modelBuilder">ModelBuilder instance</param>
    public override void ConfigureModuleModel(ModelBuilder modelBuilder)
    {
        // Configure entity relationships for Xprema Management
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