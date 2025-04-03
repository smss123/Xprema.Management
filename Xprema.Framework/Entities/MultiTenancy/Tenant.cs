using Xprema.Framework.Entities.Common;

namespace Xprema.Framework.Entities.MultiTenancy;

public class Tenant : BaseEntity<Guid>
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string Identifier { get; set; }
    public string? ConnectionString { get; set; }
    public bool IsActive { get; set; } = true;
    public TenantStorageMode StorageMode { get; set; } = TenantStorageMode.SharedDatabase;
    public Dictionary<string, string> Settings { get; set; } = new();
    
    // Navigation properties
    public ICollection<TenantUser> TenantUsers { get; set; } = new List<TenantUser>();
}

public enum TenantStorageMode
{
    /// <summary>
    /// All tenants share the same database with tenant discriminator column
    /// </summary>
    SharedDatabase,
    
    /// <summary>
    /// Each tenant has its own database schema within the same database
    /// </summary>
    SharedDatabaseSeparateSchema,
    
    /// <summary>
    /// Each tenant has its own database instance
    /// </summary>
    SeparateDatabase
} 