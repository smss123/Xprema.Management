using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Xprema.EntityFrameworkCore.Migrations;

/// <summary>
/// A tool class to apply migrations programmatically
/// </summary>
public static class MigrationTool
{
    /// <summary>
    /// Apply pending migrations to the database with any DbContext type
    /// </summary>
    /// <typeparam name="TContext">The DbContext type</typeparam>
    /// <param name="serviceProvider">The service provider to get required services</param>
    /// <param name="logger">Optional logger for logging migration operations</param>
    public static void ApplyMigrations<TContext>(IServiceProvider serviceProvider, ILogger? logger = null)
        where TContext : DbContext
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

        logger?.LogInformation("Starting database migration with {ContextType}", typeof(TContext).Name);
        
        try
        {
            dbContext.Database.Migrate();
            logger?.LogInformation("Database migration completed successfully");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "An error occurred during database migration");
            throw;
        }
    }

    /// <summary>
    /// Configure services for migrations with a specific DbContext type
    /// </summary>
    /// <typeparam name="TContext">The DbContext type</typeparam>
    /// <param name="services">The service collection to add services to</param>
    /// <param name="connectionString">Database connection string</param>
    /// <param name="migrationsAssembly">Optional migrations assembly name, defaults to the current assembly</param>
    public static IServiceCollection AddXpremaMigrations<TContext>(
        this IServiceCollection services, 
        string connectionString,
        string? migrationsAssembly = null)
        where TContext : DbContext
    {
        var configuration = new MigrationConfiguration
        {
            ConnectionString = connectionString,
            MigrationsAssembly = migrationsAssembly ?? typeof(MigrationTool).Assembly.GetName().Name ?? "Xprema.EntityFrameworkCore.Migrations"
        };
        
        return services.AddDbContextForMigrations<TContext>(configuration);
    }
} 