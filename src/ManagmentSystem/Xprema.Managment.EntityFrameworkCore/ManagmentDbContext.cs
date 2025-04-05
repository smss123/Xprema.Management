using Microsoft.EntityFrameworkCore;
using Xprema.Managment.Domain.ActionArea;
using Xprema.Managment.Domain.EmployeeArea;
using Xprema.Managment.Domain.ProcedureArea;
using Xprema.Managment.Domain.TaskArea;

namespace Xprema.Managment.EntityFrameworkCore;

public class ManagmentDbContext : DbContext
{
    public ManagmentDbContext(DbContextOptions<ManagmentDbContext> options) : base(options)
    {
    }

    // Procedure Area
    public DbSet<FlowProcedure> FlowProcedures { get; set; }
    public DbSet<FlowProcedureStep> FlowProcedureSteps { get; set; }
    public DbSet<FlowProcedureCompose> FlowProcedureComposes { get; set; }
    
    // Task Area
    public DbSet<FlowTask> FlowTasks { get; set; }
    public DbSet<FlowTaskStep> FlowTaskSteps { get; set; }
    public DbSet<TaskTimeline> TaskTimelines { get; set; }
    public DbSet<TaskParticipant> TaskParticipants { get; set; }
    
    // Action Area
    public DbSet<FlowAction> FlowActions { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure entities and relationships
        ConfigureProcedureArea(modelBuilder);
        ConfigureTaskArea(modelBuilder);
        ConfigureActionArea(modelBuilder);
        
        // Apply global query filters for soft-delete and multi-tenancy
        ApplyGlobalFilters(modelBuilder);
    }
    
    private void ConfigureProcedureArea(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FlowProcedure>(b =>
        {
            b.ToTable("FlowProcedures");
            b.HasKey(x => x.Id);
            b.Property(x => x.ProcedureName).IsRequired().HasMaxLength(256);
            b.Property(x => x.ConcurrencyStamp).IsConcurrencyToken();
            
            // Set up relationships
            b.HasMany(x => x.Steps)
                .WithOne(x => x.FlowProcedure)
                .HasForeignKey(x => x.ProcedureId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<FlowProcedureStep>(b =>
        {
            b.ToTable("FlowProcedureSteps");
            b.HasKey(x => x.Id);
            
            // Set up relationships
            b.HasOne(x => x.FlowProcedure)
                .WithMany()
                .HasForeignKey(x => x.ProcedureId)
                .OnDelete(DeleteBehavior.Restrict);
                
            b.HasOne(x => x.ProcedureCompose)
                .WithMany(x => x.FlowProcedureSteps)
                .HasForeignKey(x => x.ComposeId)
                .OnDelete(DeleteBehavior.Restrict);
                
            b.HasOne(x => x.Action)
                .WithMany()
                .HasForeignKey(x => x.ActionId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<FlowProcedureCompose>(b =>
        {
            b.ToTable("FlowProcedureComposes");
            b.HasKey(x => x.Id);
            b.Property(x => x.ProcedureComposeName).IsRequired().HasMaxLength(256);
            
            // Set up relationships
            b.HasMany(x => x.FlowProcedureSteps)
                .WithOne(x => x.ProcedureCompose)
                .HasForeignKey(x => x.ComposeId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
    
    private void ConfigureTaskArea(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FlowTask>(b =>
        {
            b.ToTable("FlowTasks");
            b.HasKey(x => x.Id);
            b.Property(x => x.TaskName).IsRequired().HasMaxLength(256);
            
            // Set up relationships
            b.HasOne(x => x.Procedure)
                .WithMany(x => x.Tasks)
                .HasForeignKey(x => x.ProcedureId)
                .OnDelete(DeleteBehavior.Restrict);
                
            b.HasOne(x => x.ProcedureStep)
                .WithMany()
                .HasForeignKey(x => x.ProcedureStepId)
                .OnDelete(DeleteBehavior.Restrict);
                
            b.HasMany(x => x.Steps)
                .WithOne(x => x.Task)
                .HasForeignKey(x => x.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
                
            b.HasMany(x => x.Timeline)
                .WithOne(x => x.Task)
                .HasForeignKey(x => x.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
                
            b.HasMany(x => x.Participants)
                .WithOne(x => x.Task)
                .HasForeignKey(x => x.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<FlowTaskStep>(b =>
        {
            b.ToTable("FlowTaskSteps");
            b.HasKey(x => x.Id);
            b.Property(x => x.StepName).IsRequired().HasMaxLength(256);
            
            // Set up relationships
            b.HasOne(x => x.Task)
                .WithMany(x => x.Steps)
                .HasForeignKey(x => x.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
                
            b.HasOne(x => x.PreviousStep)
                .WithOne(x => x.NextStep)
                .HasForeignKey<FlowTaskStep>(x => x.PreviousStepId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<TaskTimeline>(b =>
        {
            b.ToTable("TaskTimelines");
            b.HasKey(x => x.Id);
            
            // Set up relationships
            b.HasOne(x => x.Task)
                .WithMany(x => x.Timeline)
                .HasForeignKey(x => x.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
                
            b.HasOne(x => x.TaskStep)
                .WithMany()
                .HasForeignKey(x => x.TaskStepId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<TaskParticipant>(b =>
        {
            b.ToTable("TaskParticipants");
            b.HasKey(x => x.Id);
            
            // Set up relationships
            b.HasOne(x => x.Task)
                .WithMany(x => x.Participants)
                .HasForeignKey(x => x.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
    
    private void ConfigureActionArea(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FlowAction>(b =>
        {
            b.ToTable("FlowActions");
            b.HasKey(x => x.Id);
        });
    }
    
    private void ApplyGlobalFilters(ModelBuilder modelBuilder)
    {
        // Apply soft-delete filter to all relevant entities
        modelBuilder.Entity<FlowProcedure>().HasQueryFilter(e => !e.IsDeleted);
        
        // Multi-tenancy filters can be added here too
        // modelBuilder.Entity<FlowProcedure>().HasQueryFilter(e => e.TenantId == CurrentTenant.Id);
    }
} 