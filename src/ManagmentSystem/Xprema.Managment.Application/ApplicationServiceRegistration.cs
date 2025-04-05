using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Xprema.Managment.Application.Mapping;
using Xprema.Managment.Application.Procedures;
using Xprema.Managment.Application.Contracts.Procedures;
using Xprema.Managment.Application.Contracts.Tasks;
using Xprema.Managment.Application.Tasks;

namespace Xprema.Managment.Application;

/// <summary>
/// Application service registration extension methods
/// </summary>
public static class ApplicationServiceRegistration
{
    /// <summary>
    /// Adds application services to the DI container
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register AutoMapper
        services.AddAutoMapper(typeof(ManagmentMappingProfile));
        
        // Register application services
        services.AddScoped<IFlowProcedureAppService, FlowProcedureAppService>();
        services.AddScoped<IFlowTaskAppService, FlowTaskAppService>();
        
        // Register workflow services
        services.AddScoped<TaskWorkflowService>();
        
        // Register MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        
        return services;
    }
} 