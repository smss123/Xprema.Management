using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Xprema.EntityFrameworkCore.Migrations.DbMigrations;

/// <summary>
/// Factory for creating XpremaMigrationsDbContext instances for migrations.
/// Follows ABP.io's approach with a dedicated migrations DbContext factory.
/// </summary>
public class XpremaMigrationsDbContextFactory : IDesignTimeDbContextFactory<XpremaMigrationsDbContext>
{
    public XpremaMigrationsDbContext CreateDbContext(string[] args)
    {
        // Create a service collection for DI
        var serviceCollection = new ServiceCollection();
        
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        // Get connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");
        }
        
        // Configure and register all module contexts
        ConfigureDbContext(serviceCollection, configuration);
        
        // Build service provider
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        // Create options for migrations context
        var optionsBuilder = new DbContextOptionsBuilder<XpremaMigrationsDbContext>();
        ConfigureMigrationsContext(optionsBuilder, connectionString);
        
        // Create and return the migrations context
        return new XpremaMigrationsDbContext(optionsBuilder.Options, serviceProvider);
    }
    
    /// <summary>
    /// Configure the main migrations DbContext
    /// </summary>
    protected virtual void ConfigureMigrationsContext(
        DbContextOptionsBuilder<XpremaMigrationsDbContext> optionsBuilder,
        string connectionString)
    {
        optionsBuilder.UseSqlServer(connectionString, options =>
        {
            options.MigrationsAssembly(GetType().Assembly.GetName().Name);
        });
    }
    
    /// <summary>
    /// Configure all module DbContexts
    /// </summary>
    protected virtual void ConfigureDbContext(
        IServiceCollection services,
        IConfiguration configuration)
    {
        // Register all module DbContexts here
        // This is typically done by a dedicated method in each module
        
        // Example:
        // services.AddXpremaManagementDbContext(configuration);
    }
} 