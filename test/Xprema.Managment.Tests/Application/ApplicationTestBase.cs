using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Xprema.Managment.Application.Mapping;
using Xprema.Managment.Tests.TestBase;

namespace Xprema.Managment.Tests.Application;

/// <summary>
/// Base class for application service tests
/// </summary>
public abstract class ApplicationTestBase : TestBase
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        
        // Add AutoMapper with our mapping profile
        services.AddAutoMapper(typeof(ManagmentMappingProfile));
        
        // Register services
        RegisterServices(services);
    }

    protected virtual void RegisterServices(IServiceCollection services)
    {
        // Override in derived classes to register additional services
    }
    
    protected IMapper GetMapper()
    {
        return GetService<IMapper>();
    }
} 