namespace Xprema.Framework.Entities.MultiTenancy;

/// <summary>
/// Interface for entities that belong to a tenant
/// </summary>
public interface ITenantEntity
{
    Guid TenantId { get; set; }
} 