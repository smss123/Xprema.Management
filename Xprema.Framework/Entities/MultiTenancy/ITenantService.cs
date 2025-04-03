namespace Xprema.Framework.Entities.MultiTenancy;

/// <summary>
/// Service for managing tenants
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Get a tenant by ID
    /// </summary>
    Task<Tenant?> GetTenantByIdAsync(Guid tenantId);
    
    /// <summary>
    /// Get a tenant by identifier
    /// </summary>
    Task<Tenant?> GetTenantByIdentifierAsync(string identifier);
    
    /// <summary>
    /// Get all tenants
    /// </summary>
    Task<IEnumerable<Tenant>> GetAllTenantsAsync(bool includeInactive = false);
    
    /// <summary>
    /// Create a new tenant
    /// </summary>
    Task<Tenant> CreateTenantAsync(string name, string identifier, string? description = null, TenantStorageMode storageMode = TenantStorageMode.SharedDatabase, string? connectionString = null, string createdBy = "system");
    
    /// <summary>
    /// Update a tenant
    /// </summary>
    Task<Tenant> UpdateTenantAsync(Guid tenantId, string name, string? description, bool isActive, TenantStorageMode storageMode, string? connectionString, string modifiedBy);
    
    /// <summary>
    /// Delete a tenant
    /// </summary>
    Task DeleteTenantAsync(Guid tenantId, string deletedBy);
    
    /// <summary>
    /// Add a setting to a tenant
    /// </summary>
    Task AddTenantSettingAsync(Guid tenantId, string key, string value, string modifiedBy);
    
    /// <summary>
    /// Remove a setting from a tenant
    /// </summary>
    Task RemoveTenantSettingAsync(Guid tenantId, string key, string modifiedBy);
    
    /// <summary>
    /// Add a user to a tenant
    /// </summary>
    Task<TenantUser> AddUserToTenantAsync(Guid tenantId, Guid userId, bool isAdmin, string addedBy);
    
    /// <summary>
    /// Remove a user from a tenant
    /// </summary>
    Task RemoveUserFromTenantAsync(Guid tenantId, Guid userId, string removedBy);
    
    /// <summary>
    /// Get all users for a tenant
    /// </summary>
    Task<IEnumerable<TenantUser>> GetTenantUsersAsync(Guid tenantId);
} 