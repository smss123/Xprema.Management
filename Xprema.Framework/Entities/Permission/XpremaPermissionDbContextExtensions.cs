using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Xprema.Framework.Entities.Permission;

/// <summary>
/// Extension methods for configuring permission database services
/// </summary>
public static class XpremaPermissionDbContextExtensions
{
    /// <summary>
    /// Adds the permission database to the service collection.
    /// This allows using a separate database for permissions.
    /// </summary>
    public static IServiceCollection AddXpremaPermissionDb(
        this IServiceCollection services,
        string connectionString,
        Action<DbContextOptionsBuilder>? optionsAction = null)
    {
        return services.AddDbContext<XpremaPermissionDbContext>(options =>
        {
            if (optionsAction != null)
            {
                optionsAction(options);
            }
            else
            {
                // Use a configurable database provider
                options.UseSqlServer(connectionString);
            }
        });
    }
    
    /// <summary>
    /// Adds in-memory permission database for testing
    /// </summary>
    public static IServiceCollection AddXpremaPermissionDbInMemory(
        this IServiceCollection services,
        string databaseName = "XpremaPermissionDb")
    {
        return services.AddDbContext<XpremaPermissionDbContext>(options =>
        {
            options.UseInMemoryDatabase(databaseName);
        });
    }
    
    /// <summary>
    /// Registers permission services that work with the separate permission database.
    /// Call this after registering the XpremaPermissionDbContext.
    /// </summary>
    public static IServiceCollection AddXpremaPermissionServices(this IServiceCollection services)
    {
        // Replace with the existing PermissionService implementation that works with XpremaPermissionDbContext
        services.AddScoped<IPermissionService, PermissionService<XpremaPermissionDbContext>>();
        
        return services;
    }
} 