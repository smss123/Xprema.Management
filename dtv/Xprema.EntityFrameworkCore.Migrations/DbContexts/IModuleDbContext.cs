using Microsoft.EntityFrameworkCore;

namespace Xprema.EntityFrameworkCore.Migrations.DbContexts;

/// <summary>
/// Interface for module-specific DbContexts.
/// Follows ABP.io's approach where each module can define its own DbContext interface.
/// </summary>
public interface IModuleDbContext : IEfCoreDbContext
{
    /// <summary>
    /// Configures the database model for this module
    /// </summary>
    /// <param name="modelBuilder">ModelBuilder instance to configure</param>
    void ConfigureModuleModel(ModelBuilder modelBuilder);
}

/// <summary>
/// Generic module DbContext interface that allows specifying the module name
/// </summary>
/// <typeparam name="TModule">Type representing the module</typeparam>
public interface IModuleDbContext<TModule> : IModuleDbContext
{
    /// <summary>
    /// Gets the module name
    /// </summary>
    string ModuleName { get; }
}