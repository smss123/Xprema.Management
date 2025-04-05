using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xprema.Managment.EntityFrameworkCore;

namespace Xprema.Managment.HttpApi.Host.DbMigrations;

/// <summary>
/// Utility class to apply database migrations
/// </summary>
public class DbMigrationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DbMigrationService> _logger;

    public DbMigrationService(
        IServiceProvider serviceProvider,
        ILogger<DbMigrationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Applies any pending migrations to the database
    /// </summary>
    public async Task MigrateAsync()
    {
        _logger.LogInformation("Migrating database...");
        
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ManagmentDbContext>();
            
            await dbContext.Database.MigrateAsync();
            
            _logger.LogInformation("Database migration completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while migrating the database.");
            throw;
        }
    }
    
    /// <summary>
    /// Seeds initial data to the database
    /// </summary>
    public async Task SeedAsync()
    {
        _logger.LogInformation("Seeding database...");
        
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ManagmentDbContext>();
            
            // Check if database is empty
            if (!await dbContext.FlowProcedures.AnyAsync())
            {
                _logger.LogInformation("Seeding initial data...");
                
                // Seed initial data here
                // Example:
                // await dbContext.FlowProcedures.AddAsync(new FlowProcedure { ... });
                
                await dbContext.SaveChangesAsync();
                
                _logger.LogInformation("Seed completed successfully.");
            }
            else
            {
                _logger.LogInformation("Database already has data, skipping seed.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
} 