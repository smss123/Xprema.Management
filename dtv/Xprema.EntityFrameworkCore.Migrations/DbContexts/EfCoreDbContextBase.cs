using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;

namespace Xprema.EntityFrameworkCore.Migrations.DbContexts;

/// <summary>
/// Base class for all DbContext classes in the application.
/// Follows ABP.io's approach for database contexts.
/// </summary>
/// <typeparam name="TDbContext">Type of the DbContext</typeparam>
public abstract class EfCoreDbContextBase<TDbContext> : DbContext, IEfCoreDbContext
    where TDbContext : DbContext
{
    /// <summary>
    /// Service provider reference for resolving services at runtime
    /// </summary>
    protected IServiceProvider? ServiceProvider { get; private set; }

    protected EfCoreDbContextBase(DbContextOptions<TDbContext> options)
        : base(options)
    {
    }

    protected EfCoreDbContextBase(DbContextOptions<TDbContext> options, IServiceProvider serviceProvider)
        : base(options)
    {
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Configure all entity configurations from all modules
    /// </summary>
    /// <param name="modelBuilder">Model builder</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure all modules' entity configurations
        foreach (var configureModelAction in GetModelBuilderConfigurationActions())
        {
            configureModelAction(modelBuilder);
        }
    }

    /// <summary>
    /// Override to add default values when saving entities
    /// </summary>
    public override int SaveChanges()
    {
        ApplyXpremaConceptsForAddedEntities();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override to add default values when saving entities async
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyXpremaConceptsForAddedEntities();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Apply Xprema-specific conventions for added entities
    /// (auditing, multi-tenancy, soft delete, etc.)
    /// </summary>
    protected virtual void ApplyXpremaConceptsForAddedEntities()
    {
        foreach (var entry in ChangeTracker.Entries().ToList())
        {
            ApplyConcepts(entry);
        }
    }

    /// <summary>
    /// Apply concepts for a specific entity entry
    /// </summary>
    /// <param name="entry">Entry to apply concepts to</param>
    protected virtual void ApplyConcepts(EntityEntry entry)
    {
        switch (entry.State)
        {
            case EntityState.Added:
                SetCreationAuditProperties(entry);
                break;
            case EntityState.Modified:
                SetModificationAuditProperties(entry);
                break;
            case EntityState.Deleted:
                HandleSoftDelete(entry);
                break;
        }
    }

    /// <summary>
    /// Get all model builder configuration actions from modules
    /// </summary>
    /// <returns>List of configuration actions</returns>
    protected virtual List<Action<ModelBuilder>> GetModelBuilderConfigurationActions()
    {
        var actions = new List<Action<ModelBuilder>>();
        
        // If a module wants to add entity configuration, it should be added here
        // This follows ABP.io's approach where each module contributes to the model configuration
        
        return actions;
    }

    /// <summary>
    /// Set creation audit properties for entity
    /// </summary>
    /// <param name="entry">Entity entry</param>
    protected virtual void SetCreationAuditProperties(EntityEntry entry)
    {
        // Implementation can set CreationTime, CreatorId etc.
    }

    /// <summary>
    /// Set modification audit properties for entity
    /// </summary>
    /// <param name="entry">Entity entry</param>
    protected virtual void SetModificationAuditProperties(EntityEntry entry)
    {
        // Implementation can set LastModificationTime, LastModifierId etc.
    }

    /// <summary>
    /// Handle soft delete pattern if entity supports it
    /// </summary>
    /// <param name="entry">Entity entry</param>
    protected virtual void HandleSoftDelete(EntityEntry entry)
    {
        // Implementation can set IsDeleted flag instead of physically deleting
    }
} 