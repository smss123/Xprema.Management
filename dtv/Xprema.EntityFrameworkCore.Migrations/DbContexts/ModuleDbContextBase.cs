using Microsoft.EntityFrameworkCore;

namespace Xprema.EntityFrameworkCore.Migrations.DbContexts;

/// <summary>
/// Base class for module-specific DbContexts.
/// Follows ABP.io's approach where each module can define its own DbContext.
/// </summary>
/// <typeparam name="TDbContext">Type of the DbContext</typeparam>
public abstract class ModuleDbContextBase<TDbContext> : EfCoreDbContextBase<TDbContext>, IModuleDbContext
    where TDbContext : DbContext
{
    protected ModuleDbContextBase(DbContextOptions<TDbContext> options)
        : base(options)
    {
    }

    protected ModuleDbContextBase(DbContextOptions<TDbContext> options, IServiceProvider serviceProvider)
        : base(options, serviceProvider)
    {
    }

    /// <summary>
    /// Configures the database model for this module
    /// </summary>
    /// <param name="modelBuilder">ModelBuilder instance to configure</param>
    public abstract void ConfigureModuleModel(ModelBuilder modelBuilder);

    /// <summary>
    /// Override to call module-specific model configuration
    /// </summary>
    /// <returns>List of model configuration actions</returns>
    protected override List<Action<ModelBuilder>> GetModelBuilderConfigurationActions()
    {
        var actions = base.GetModelBuilderConfigurationActions();
        
        // Add this module's own configuration
        actions.Add(ConfigureModuleModel);
        
        return actions;
    }
}

/// <summary>
/// Base class for module-specific DbContexts with module type information
/// </summary>
/// <typeparam name="TDbContext">Type of the DbContext</typeparam>
/// <typeparam name="TModule">Type representing the module</typeparam>
public abstract class ModuleDbContextBase<TDbContext, TModule> : ModuleDbContextBase<TDbContext>, IModuleDbContext<TModule>
    where TDbContext : DbContext
{
    /// <summary>
    /// Gets the module name
    /// </summary>
    public virtual string ModuleName => typeof(TModule).Name;

    protected ModuleDbContextBase(DbContextOptions<TDbContext> options)
        : base(options)
    {
    }

    protected ModuleDbContextBase(DbContextOptions<TDbContext> options, IServiceProvider serviceProvider)
        : base(options, serviceProvider)
    {
    }
} 