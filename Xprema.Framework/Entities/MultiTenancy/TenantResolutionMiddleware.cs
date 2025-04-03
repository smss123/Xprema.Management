using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Xprema.Framework.Entities.MultiTenancy;

/// <summary>
/// Middleware for resolving the current tenant from the request
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;
    private readonly TenantResolutionStrategy _resolutionStrategy;

    public TenantResolutionMiddleware(
        RequestDelegate next,
        ILogger<TenantResolutionMiddleware> logger,
        TenantResolutionStrategy resolutionStrategy = TenantResolutionStrategy.Host)
    {
        _next = next;
        _logger = logger;
        _resolutionStrategy = resolutionStrategy;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var tenantService = context.RequestServices.GetRequiredService<ITenantService>();
            var tenantContextAccessor = context.RequestServices.GetRequiredService<ITenantContextAccessor>();

            string? tenantIdentifier = null;

            // Resolve tenant based on strategy
            switch (_resolutionStrategy)
            {
                case TenantResolutionStrategy.Host:
                    tenantIdentifier = context.Request.Host.Host.Split('.')[0];
                    break;
                case TenantResolutionStrategy.Header:
                    if (context.Request.Headers.TryGetValue("X-Tenant", out var headerValues))
                    {
                        tenantIdentifier = headerValues.FirstOrDefault();
                    }
                    break;
                case TenantResolutionStrategy.QueryString:
                    tenantIdentifier = context.Request.Query["tenant"].ToString();
                    break;
                case TenantResolutionStrategy.Route:
                    // Get route data from route value provider
                    var routeData = context.GetRouteData();
                    if (routeData?.Values != null && routeData.Values.TryGetValue("tenant", out var tenantValue))
                    {
                        tenantIdentifier = tenantValue?.ToString();
                    }
                    break;
                case TenantResolutionStrategy.Cookie:
                    tenantIdentifier = context.Request.Cookies["tenant"];
                    break;
                case TenantResolutionStrategy.Claim:
                    tenantIdentifier = context.User.FindFirst("tenant")?.Value;
                    break;
            }

            if (!string.IsNullOrEmpty(tenantIdentifier))
            {
                var tenant = await tenantService.GetTenantByIdentifierAsync(tenantIdentifier);
                if (tenant != null && tenant.IsActive)
                {
                    tenantContextAccessor.SetCurrentTenantId(tenant.Id);
                    _logger.LogInformation("Resolved tenant: {TenantId} ({TenantName})", tenant.Id, tenant.Name);
                }
                else
                {
                    _logger.LogWarning("Tenant not found or inactive: {TenantIdentifier}", tenantIdentifier);
                }
            }
            else
            {
                _logger.LogWarning("Could not resolve tenant identifier");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving tenant");
        }

        // Call the next middleware in the pipeline
        await _next(context);
    }
}

/// <summary>
/// Extension methods for the tenant resolution middleware
/// </summary>
public static class TenantResolutionMiddlewareExtensions
{
    /// <summary>
    /// Use tenant resolution in the application pipeline
    /// </summary>
    public static IApplicationBuilder UseTenantResolution(
        this IApplicationBuilder app,
        TenantResolutionStrategy strategy = TenantResolutionStrategy.Host)
    {
        return app.UseMiddleware<TenantResolutionMiddleware>(strategy);
    }
}

/// <summary>
/// Strategies for resolving the current tenant from the request
/// </summary>
public enum TenantResolutionStrategy
{
    /// <summary>
    /// Resolve tenant from the host name (e.g., tenant1.example.com)
    /// </summary>
    Host,
    
    /// <summary>
    /// Resolve tenant from the HTTP header (X-Tenant)
    /// </summary>
    Header,
    
    /// <summary>
    /// Resolve tenant from the query string (?tenant=tenant1)
    /// </summary>
    QueryString,
    
    /// <summary>
    /// Resolve tenant from the route parameter (/{tenant}/...)
    /// </summary>
    Route,
    
    /// <summary>
    /// Resolve tenant from a cookie
    /// </summary>
    Cookie,
    
    /// <summary>
    /// Resolve tenant from a user claim
    /// </summary>
    Claim
} 