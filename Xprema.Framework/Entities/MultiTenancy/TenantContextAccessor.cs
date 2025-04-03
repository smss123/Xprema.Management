using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Xprema.Framework.Entities.MultiTenancy;

/// <summary>
/// Implementation of ITenantContextAccessor that uses AsyncLocal for tenant context
/// </summary>
public class TenantContextAccessor<TDbContext> : ITenantContextAccessor 
    where TDbContext : DbContext
{
    private static readonly AsyncLocal<TenantContext> _currentTenant = new();
    private readonly IServiceProvider _serviceProvider;

    public TenantContextAccessor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Guid GetCurrentTenantId()
    {
        return _currentTenant.Value?.TenantId ?? Guid.Empty;
    }

    public async Task<Tenant?> GetCurrentTenantAsync()
    {
        var tenantId = GetCurrentTenantId();
        if (tenantId == Guid.Empty)
            return null;

        // Try to get the DbContext directly from the service provider first
        // This will use the same DbContext instance as in tests
        try
        {
            var dbContext = _serviceProvider.GetService<TDbContext>();
            if (dbContext != null)
            {
                var tenant = await dbContext.Set<Tenant>().FindAsync(tenantId);
                if (tenant != null)
                    return tenant;
            }
        }
        catch
        {
            // If we can't get the DbContext directly, create a new scope
        }

        // If the tenant wasn't found or the DbContext couldn't be retrieved directly,
        // create a new scope and try again
        using var scope = _serviceProvider.CreateScope();
        var scopedDbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        return await scopedDbContext.Set<Tenant>().FindAsync(tenantId);
    }

    public void SetCurrentTenantId(Guid tenantId)
    {
        _currentTenant.Value = new TenantContext { TenantId = tenantId };
    }

    private class TenantContext
    {
        public Guid TenantId { get; set; }
    }
} 