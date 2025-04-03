using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Xprema.EntityFrameworkCore.Migrations;

/// <summary>
/// Configuration options for the migration system
/// </summary>
public class MigrationConfiguration
{
    /// <summary>
    /// Name of the migration assembly
    /// </summary>
    public string MigrationsAssembly { get; set; } = "Xprema.EntityFrameworkCore.Migrations";
    
    /// <summary>
    /// Connection string for the database
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
    
    /// <summary>
    /// Database provider to use (SqlServer, PostgreSQL, etc.)
    /// </summary>
    public string DatabaseProvider { get; set; } = "SqlServer";
}

/// <summary>
/// Static class for configuring migrations
/// </summary>
public static class MigrationConfigurationExtensions
{
    /// <summary>
    /// Configures the DbContextOptionsBuilder for migrations
    /// </summary>
    /// <typeparam name="TContext">The DbContext type</typeparam>
    /// <param name="optionsBuilder">The options builder</param>
    /// <param name="configuration">Migration configuration</param>
    /// <returns>The configured options builder</returns>
    public static DbContextOptionsBuilder<TContext> ConfigureForMigrations<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        MigrationConfiguration configuration) 
        where TContext : DbContext
    {
        switch (configuration.DatabaseProvider.ToLowerInvariant())
        {
            case "sqlserver":
                optionsBuilder.UseSqlServer(configuration.ConnectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(configuration.MigrationsAssembly);
                });
                break;
            
            // Add support for other providers as needed
            // case "postgresql":
            //     optionsBuilder.UseNpgsql(configuration.ConnectionString, npgsqlOptions =>
            //     {
            //         npgsqlOptions.MigrationsAssembly(configuration.MigrationsAssembly);
            //     });
            //     break;
            
            default:
                throw new NotSupportedException($"Database provider '{configuration.DatabaseProvider}' is not supported.");
        }
        
        return optionsBuilder;
    }
    
    /// <summary>
    /// Adds DbContext configuration for migrations
    /// </summary>
    /// <typeparam name="TContext">The DbContext type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Migration configuration</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddDbContextForMigrations<TContext>(
        this IServiceCollection services,
        MigrationConfiguration configuration)
        where TContext : DbContext
    {
        services.AddDbContext<TContext>(options =>
        {
            options.ConfigureForMigrations(configuration);
        });
        
        return services;
    }
} 