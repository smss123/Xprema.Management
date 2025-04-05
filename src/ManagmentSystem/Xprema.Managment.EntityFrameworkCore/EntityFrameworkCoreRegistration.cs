using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Xprema.Managment.EntityFrameworkCore;

/// <summary>
/// EntityFrameworkCore registration extension methods
/// </summary>
public static class EntityFrameworkCoreRegistration
{
    /// <summary>
    /// Adds EntityFrameworkCore services to the DI container
    /// </summary>
    public static IServiceCollection AddEntityFrameworkCore(this IServiceCollection services, string connectionString)
    {
        // Register DbContext
        services.AddDbContext<ManagmentDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });
        
        return services;
    }
} 