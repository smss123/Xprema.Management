using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Xprema.Framework.Entities.MultiTenancy;

/// <summary>
/// Implementation of ITenantService for managing tenants
/// </summary>
public class TenantService<TDbContext> : ITenantService
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public TenantService(TDbContext dbContext, ITenantContextAccessor tenantContextAccessor)
    {
        _dbContext = dbContext;
        _tenantContextAccessor = tenantContextAccessor;
    }

    public async Task<Tenant?> GetTenantByIdAsync(Guid tenantId)
    {
        return await _dbContext.Set<Tenant>().FindAsync(tenantId);
    }

    public async Task<Tenant?> GetTenantByIdentifierAsync(string identifier)
    {
        return await _dbContext.Set<Tenant>()
            .FirstOrDefaultAsync(t => t.Identifier == identifier);
    }

    public async Task<IEnumerable<Tenant>> GetAllTenantsAsync(bool includeInactive = false)
    {
        var query = _dbContext.Set<Tenant>().AsQueryable();
        
        if (!includeInactive)
        {
            query = query.Where(t => t.IsActive);
        }
        
        return await query.ToListAsync();
    }

    public async Task<Tenant> CreateTenantAsync(
        string name, 
        string identifier, 
        string? description = null, 
        TenantStorageMode storageMode = TenantStorageMode.SharedDatabase, 
        string? connectionString = null, 
        string createdBy = "system")
    {
        // Check for duplicate identifier
        if (await _dbContext.Set<Tenant>().AnyAsync(t => t.Identifier == identifier))
        {
            throw new ArgumentException($"Tenant with identifier '{identifier}' already exists");
        }
        
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Identifier = identifier,
            ConnectionString = connectionString,
            StorageMode = storageMode,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedDate = DateTime.UtcNow
        };
        
        await _dbContext.Set<Tenant>().AddAsync(tenant);
        await _dbContext.SaveChangesAsync();
        
        return tenant;
    }

    public async Task<Tenant> UpdateTenantAsync(
        Guid tenantId, 
        string name, 
        string? description, 
        bool isActive, 
        TenantStorageMode storageMode, 
        string? connectionString, 
        string modifiedBy)
    {
        var tenant = await _dbContext.Set<Tenant>().FindAsync(tenantId) 
            ?? throw new ArgumentException($"Tenant with ID {tenantId} not found");
        
        tenant.Name = name;
        tenant.Description = description;
        tenant.IsActive = isActive;
        tenant.StorageMode = storageMode;
        tenant.ConnectionString = connectionString;
        tenant.ModifiedBy = modifiedBy;
        tenant.ModifiedDate = DateTime.UtcNow;
        
        await _dbContext.SaveChangesAsync();
        
        return tenant;
    }

    public async Task DeleteTenantAsync(Guid tenantId, string deletedBy)
    {
        var tenant = await _dbContext.Set<Tenant>().FindAsync(tenantId) 
            ?? throw new ArgumentException($"Tenant with ID {tenantId} not found");
        
        tenant.IsDeleted = true;
        tenant.DeletedBy = deletedBy;
        tenant.DeletedDate = DateTime.UtcNow;
        
        await _dbContext.SaveChangesAsync();
    }

    public async Task AddTenantSettingAsync(Guid tenantId, string key, string value, string modifiedBy)
    {
        var tenant = await _dbContext.Set<Tenant>().FindAsync(tenantId) 
            ?? throw new ArgumentException($"Tenant with ID {tenantId} not found");
        
        tenant.Settings[key] = value;
        tenant.ModifiedBy = modifiedBy;
        tenant.ModifiedDate = DateTime.UtcNow;
        
        await _dbContext.SaveChangesAsync();
    }

    public async Task RemoveTenantSettingAsync(Guid tenantId, string key, string modifiedBy)
    {
        var tenant = await _dbContext.Set<Tenant>().FindAsync(tenantId) 
            ?? throw new ArgumentException($"Tenant with ID {tenantId} not found");
        
        if (tenant.Settings.ContainsKey(key))
        {
            tenant.Settings.Remove(key);
            tenant.ModifiedBy = modifiedBy;
            tenant.ModifiedDate = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<TenantUser> AddUserToTenantAsync(Guid tenantId, Guid userId, bool isAdmin, string addedBy)
    {
        var tenant = await _dbContext.Set<Tenant>().FindAsync(tenantId) 
            ?? throw new ArgumentException($"Tenant with ID {tenantId} not found");
        
        // Check if user is already in tenant
        var existingTenantUser = await _dbContext.Set<TenantUser>()
            .FirstOrDefaultAsync(tu => tu.TenantId == tenantId && tu.UserId == userId && !tu.IsDeleted);
            
        if (existingTenantUser != null)
        {
            throw new ArgumentException($"User {userId} is already in tenant {tenantId}");
        }
        
        var tenantUser = new TenantUser
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            IsAdmin = isAdmin,
            CreatedBy = addedBy,
            CreatedDate = DateTime.UtcNow
        };
        
        await _dbContext.Set<TenantUser>().AddAsync(tenantUser);
        await _dbContext.SaveChangesAsync();
        
        return tenantUser;
    }

    public async Task RemoveUserFromTenantAsync(Guid tenantId, Guid userId, string removedBy)
    {
        var tenantUser = await _dbContext.Set<TenantUser>()
            .FirstOrDefaultAsync(tu => tu.TenantId == tenantId && tu.UserId == userId && !tu.IsDeleted) 
            ?? throw new ArgumentException($"User {userId} is not in tenant {tenantId}");
        
        tenantUser.IsDeleted = true;
        tenantUser.DeletedBy = removedBy;
        tenantUser.DeletedDate = DateTime.UtcNow;
        
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<TenantUser>> GetTenantUsersAsync(Guid tenantId)
    {
        return await _dbContext.Set<TenantUser>()
            .Where(tu => tu.TenantId == tenantId && !tu.IsDeleted)
            .ToListAsync();
    }
} 