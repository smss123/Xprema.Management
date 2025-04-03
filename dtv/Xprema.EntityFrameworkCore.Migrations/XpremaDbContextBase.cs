using Microsoft.EntityFrameworkCore;

namespace Xprema.EntityFrameworkCore.Migrations;

/// <summary>
/// Base DbContext class that can be extended by any project
/// </summary>
/// <typeparam name="TContext">The specific DbContext implementation type</typeparam>
public abstract class XpremaDbContextBase<TContext> : DbContext, IXpremaDbContext 
    where TContext : DbContext
{
    protected XpremaDbContextBase(DbContextOptions<TContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Configure the model - to be implemented by derived contexts
    /// </summary>
    /// <param name="modelBuilder">ModelBuilder instance</param>
    public abstract void ConfigureModel(ModelBuilder modelBuilder);

    /// <summary>
    /// OnModelCreating is sealed and calls ConfigureModel to ensure
    /// consistent model configuration across all derived contexts
    /// </summary>
    /// <param name="modelBuilder">ModelBuilder instance</param>
    protected sealed override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Call the abstract ConfigureModel method which will be implemented
        // by the specific context in each project
        ConfigureModel(modelBuilder);
    }
}