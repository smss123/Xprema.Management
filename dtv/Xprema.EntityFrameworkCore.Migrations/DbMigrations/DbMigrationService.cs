using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Xprema.EntityFrameworkCore.Migrations.DbMigrations;

/// <summary>
/// Service for database migrations.
/// Follows ABP.io's approach for centralized migration management.
/// </summary>
public class DbMigrationService
{
    protected IServiceProvider ServiceProvider { get; }
    protected ILogger<DbMigrationService> Logger { get; }

    public DbMigrationService(IServiceProvider serviceProvider, ILogger<DbMigrationService> logger)
    {
        ServiceProvider = serviceProvider;
        Logger = logger;
    }

    /// <summary>
    /// Migrates the database
    /// </summary>
    public virtual async Task MigrateAsync()
    {
        Logger.LogInformation("Starting database migration");
        
        try
        {
            await MigrateAllDatabasesAsync();
            Logger.LogInformation("Database migration completed successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred during database migration");
            throw;
        }
    }

    /// <summary>
    /// Migrates all databases
    /// </summary>
    protected virtual async Task MigrateAllDatabasesAsync()
    {
        // Get the migrations DbContext
        using var scope = ServiceProvider.CreateScope();
        var migrationsDbContext = scope.ServiceProvider.GetRequiredService<XpremaMigrationsDbContext>();
        
        // Apply migrations
        await migrationsDbContext.Database.MigrateAsync();
    }

    /// <summary>
    /// Seeds initial data
    /// </summary>
    public virtual async Task SeedAsync()
    {
        Logger.LogInformation("Seeding initial data");
        
        try
        {
            await SeedDataAsync();
            Logger.LogInformation("Data seeding completed successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred during data seeding");
            throw;
        }
    }

    /// <summary>
    /// Seeds data for all modules
    /// </summary>
    protected virtual Task SeedDataAsync()
    {
        // Seed initial data
        // This would typically call into module-specific seeding methods
        
        return Task.CompletedTask;
    }
} 