namespace Xprema.Framework.Entities.MultiTenancy;

/// <summary>
/// Interface for accessing the current tenant context
/// </summary>
public interface ITenantContextAccessor
{
    /// <summary>
    /// Gets the current tenant identifier
    /// </summary>
    Guid GetCurrentTenantId();
    
    /// <summary>
    /// Gets the current tenant
    /// </summary>
    Task<Tenant?> GetCurrentTenantAsync();
    
    /// <summary>
    /// Sets the current tenant identifier
    /// </summary>
    void SetCurrentTenantId(Guid tenantId);
} 