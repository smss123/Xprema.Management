using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Xprema.EntityFrameworkCore.Migrations;

/// <summary>
/// Factory interface for creating DbContext instances specific to each application
/// </summary>
/// <typeparam name="TContext">The DbContext type</typeparam>
public interface IMigrationsDbContextFactory<TContext> where TContext : DbContext
{
    /// <summary>
    /// Creates a new instance of the DbContext
    /// </summary>
    /// <returns>A configured DbContext instance</returns>
    TContext CreateDbContext();
    
    /// <summary>
    /// Creates a new instance of the DbContext with the specified options
    /// </summary>
    /// <param name="options">DbContext options</param>
    /// <returns>A configured DbContext instance</returns>
    TContext CreateDbContext(DbContextOptions<TContext> options);
}

/// <summary>
/// Base factory class for creating DbContext instances during design-time operations
/// </summary>
/// <typeparam name="TContext">The DbContext type</typeparam>
public abstract class MigrationsDbContextFactoryBase<TContext> : IDesignTimeDbContextFactory<TContext>
    where TContext : DbContext
{
    /// <summary>
    /// Creates a DbContext instance with the default configuration
    /// </summary>
    /// <param name="args">Arguments passed to the factory (unused)</param>
    /// <returns>A new DbContext instance</returns>
    public TContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");
        }

        var migrationConfig = new MigrationConfiguration
        {
            ConnectionString = connectionString,
            MigrationsAssembly = typeof(TContext).Assembly.GetName().Name ?? "Xprema.EntityFrameworkCore.Migrations"
        };

        return CreateDbContextCore(migrationConfig);
    }

    /// <summary>
    /// Core method to be implemented by derived factories
    /// </summary>
    /// <param name="configuration">Migration configuration</param>
    /// <returns>A new DbContext instance</returns>
    protected abstract TContext CreateDbContextCore(MigrationConfiguration configuration);
}

/// <summary>
/// Default factory for XpremaManagementMigrationsDbContext
/// </summary>
public class XpremaManagementMigrationsDbContextFactory 
    : MigrationsDbContextFactoryBase<XpremaManagementMigrationsDbContext>
{
    protected override XpremaManagementMigrationsDbContext CreateDbContextCore(MigrationConfiguration configuration)
    {
        var optionsBuilder = new DbContextOptionsBuilder<XpremaManagementMigrationsDbContext>();
        optionsBuilder.ConfigureForMigrations(configuration);

        return new XpremaManagementMigrationsDbContext(optionsBuilder.Options);
    }
} 