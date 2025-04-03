using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using Xprema.EntityFrameworkCore.Migrations.DbContexts;
using Xprema.EntityFrameworkCore.Migrations.DbMigrations;

namespace Xprema.EntityFrameworkCore.Migrator;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // Create host
        var host = CreateHostBuilder(args).Build();
        
        // Get logger and environment
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        var env = host.Services.GetRequiredService<IHostEnvironment>();
        
        logger.LogInformation("Starting Xprema.EntityFrameworkCore.Migrator in {Environment} environment", env.EnvironmentName);

        // Create root command
        var rootCommand = new RootCommand("Xprema DB Migrator");
        
        // Add migrate command
        var migrateCommand = new Command("migrate", "Migrates the database");
        migrateCommand.SetHandler(async () =>
        {
            try
            {
                logger.LogInformation("Running database migration");
                
                var migrationService = host.Services.GetRequiredService<DbMigrationService>();
                await migrationService.MigrateAsync();
                
                logger.LogInformation("Database migration completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during database migration");
                Environment.ExitCode = 1;
            }
        });
        
        // Add seed command
        var seedCommand = new Command("seed", "Seeds initial data");
        seedCommand.SetHandler(async () =>
        {
            try
            {
                logger.LogInformation("Running data seeding");
                
                var migrationService = host.Services.GetRequiredService<DbMigrationService>();
                await migrationService.SeedAsync();
                
                logger.LogInformation("Data seeding completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during data seeding");
                Environment.ExitCode = 1;
            }
        });
        
        // Add all commands to root
        rootCommand.AddCommand(migrateCommand);
        rootCommand.AddCommand(seedCommand);
        
        // Parse command line
        return await rootCommand.InvokeAsync(args);
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                // Add DbContexts
                services.AddXpremaDbContexts(hostContext.Configuration);
                
                // Add migration service
                services.AddTransient<DbMigrationService>();
            });
    }
}
