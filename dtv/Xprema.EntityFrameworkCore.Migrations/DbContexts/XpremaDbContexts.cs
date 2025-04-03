using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xprema.EntityFrameworkCore.Migrations.DbMigrations;

namespace Xprema.EntityFrameworkCore.Migrations.DbContexts;

/// <summary>
/// Static class for configuring Xprema DbContexts.
/// Follows ABP.io's approach for centralized DbContext configuration.
/// </summary>
public static class XpremaDbContexts
{
    /// <summary>
    /// Adds all Xprema DbContexts to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddXpremaDbContexts(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Get connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");
        }
        
        // Add module DbContexts
        services.AddXpremaManagementDbContext(configuration);
        
        // Add migrations DbContext
        services.AddDbContext<XpremaMigrationsDbContext>((serviceProvider, options) =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(XpremaMigrationsDbContext).Assembly.GetName().Name);
            });
        });
        
        return services;
    }
    
    /// <summary>
    /// Adds Xprema Management DbContext to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddXpremaManagementDbContext(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<XpremaManagementDbContext>((serviceProvider, options) =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                // Use the migrations assembly for all modules
                sqlOptions.MigrationsAssembly(typeof(XpremaMigrationsDbContext).Assembly.GetName().Name);
            });
        });
        
        return services;
    }
} 