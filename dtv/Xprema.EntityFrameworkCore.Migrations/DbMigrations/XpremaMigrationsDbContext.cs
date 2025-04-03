using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Xprema.EntityFrameworkCore.Migrations.DbContexts;

namespace Xprema.EntityFrameworkCore.Migrations.DbMigrations;

/// <summary>
/// Main DbContext used for database migrations.
/// Follows ABP.io's approach with a dedicated migrations context.
/// </summary>
public class XpremaMigrationsDbContext : EfCoreDbContextBase<XpremaMigrationsDbContext>
{
    /// <summary>
    /// Collection of module DbContext types
    /// </summary>
    protected List<Type> ModuleDbContextTypes { get; }

    /// <summary>
    /// Collection of module DbContext instances
    /// </summary>
    protected List<IModuleDbContext> ModuleDbContexts { get; }

    public XpremaMigrationsDbContext(DbContextOptions<XpremaMigrationsDbContext> options, IServiceProvider serviceProvider)
        : base(options, serviceProvider)
    {
        ModuleDbContextTypes = new List<Type>();
        ModuleDbContexts = new List<IModuleDbContext>();
        
        // Initialize module DbContexts by scanning for all IModuleDbContext implementations
        RegisterModuleDbContextTypes();
        CreateModuleDbContexts();
    }

    /// <summary>
    /// Registers all module DbContext types
    /// </summary>
    protected virtual void RegisterModuleDbContextTypes()
    {
        // In a real application, we would scan all assemblies for IModuleDbContext implementations
        // For now, let's register known module context types
        // e.g. ModuleDbContextTypes.Add(typeof(IdentityModuleDbContext));
    }

    /// <summary>
    /// Creates module DbContext instances
    /// </summary>
    protected virtual void CreateModuleDbContexts()
    {
        if (ServiceProvider == null)
            return;

        foreach (var contextType in ModuleDbContextTypes)
        {
            if (ServiceProvider.GetService(contextType) is IModuleDbContext moduleDbContext)
            {
                ModuleDbContexts.Add(moduleDbContext);
            }
        }
    }

    /// <summary>
    /// Gets model builder configuration actions from all modules
    /// </summary>
    /// <returns>List of configuration actions</returns>
    protected override List<Action<ModelBuilder>> GetModelBuilderConfigurationActions()
    {
        var actions = base.GetModelBuilderConfigurationActions();
        
        // Add configurations from module DbContexts
        foreach (var moduleDbContext in ModuleDbContexts)
        {
            actions.Add(moduleDbContext.ConfigureModuleModel);
        }
        
        return actions;
    }
} 