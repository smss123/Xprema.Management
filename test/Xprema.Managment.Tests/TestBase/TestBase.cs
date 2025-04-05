using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xprema.Managment.EntityFrameworkCore;

namespace Xprema.Managment.Tests.TestBase;

/// <summary>
/// Base class for all tests
/// </summary>
public abstract class TestBase
{
    protected IServiceProvider ServiceProvider { get; }

    protected TestBase()
    {
        ServiceProvider = CreateServiceProvider();
    }

    protected virtual IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        
        // Add database context with in-memory provider
        services.AddDbContext<ManagmentDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        
        // Add any additional services here
        ConfigureServices(services);
        
        return services.BuildServiceProvider();
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        // Override in derived classes to add additional services
    }
    
    protected T GetService<T>() where T : notnull
    {
        return ServiceProvider.GetRequiredService<T>();
    }
    
    protected ManagmentDbContext GetDbContext()
    {
        return GetService<ManagmentDbContext>();
    }
} 