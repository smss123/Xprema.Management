using Microsoft.EntityFrameworkCore;
using Xprema.Framework.Entities.MultiTenancy;

namespace Xprema.Framework.Entities.Permission;

public class PermissionService<TDbContext> : IPermissionService 
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public PermissionService(TDbContext dbContext, ITenantContextAccessor tenantContextAccessor)
    {
        _dbContext = dbContext;
        _tenantContextAccessor = tenantContextAccessor;
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string permissionSystemName)
    {
        var tenantId = _tenantContextAccessor.GetCurrentTenantId();
        
        return await _dbContext.Set<Role>()
            .Where(r => r.TenantId == tenantId && r.UserRoles.Any(ur => ur.UserId == userId && ur.TenantId == tenantId))
            .SelectMany(r => r.RolePermissions.Where(rp => rp.TenantId == tenantId))
            .AnyAsync(rp => rp.Permission.SystemName == permissionSystemName && rp.Permission.TenantId == tenantId);
    }

    public async Task<bool> HasAnyPermissionAsync(Guid userId, IEnumerable<string> permissionSystemNames)
    {
        var tenantId = _tenantContextAccessor.GetCurrentTenantId();
        var systemNames = permissionSystemNames.ToList();
        
        return await _dbContext.Set<Role>()
            .Where(r => r.TenantId == tenantId && r.UserRoles.Any(ur => ur.UserId == userId && ur.TenantId == tenantId))
            .SelectMany(r => r.RolePermissions.Where(rp => rp.TenantId == tenantId))
            .AnyAsync(rp => systemNames.Contains(rp.Permission.SystemName) && rp.Permission.TenantId == tenantId);
    }

    public async Task<bool> HasAllPermissionsAsync(Guid userId, IEnumerable<string> permissionSystemNames)
    {
        var tenantId = _tenantContextAccessor.GetCurrentTenantId();
        var systemNames = permissionSystemNames.ToList();
        
        var userPermissions = await _dbContext.Set<Role>()
            .Where(r => r.TenantId == tenantId && r.UserRoles.Any(ur => ur.UserId == userId && ur.TenantId == tenantId))
            .SelectMany(r => r.RolePermissions.Where(rp => rp.TenantId == tenantId))
            .Select(rp => rp.Permission)
            .Where(p => p.TenantId == tenantId)
            .Select(p => p.SystemName)
            .Distinct()
            .ToListAsync();
            
        return systemNames.All(name => userPermissions.Contains(name));
    }

    public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(Guid userId)
    {
        var tenantId = _tenantContextAccessor.GetCurrentTenantId();
        
        return await _dbContext.Set<Role>()
            .Where(r => r.TenantId == tenantId && r.UserRoles.Any(ur => ur.UserId == userId && ur.TenantId == tenantId))
            .SelectMany(r => r.RolePermissions.Where(rp => rp.TenantId == tenantId))
            .Select(rp => rp.Permission)
            .Where(p => p.TenantId == tenantId)
            .Distinct()
            .ToListAsync();
    }

    public async Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId)
    {
        var tenantId = _tenantContextAccessor.GetCurrentTenantId();
        
        return await _dbContext.Set<Role>()
            .Where(r => r.TenantId == tenantId && r.UserRoles.Any(ur => ur.UserId == userId && ur.TenantId == tenantId))
            .ToListAsync();
    }

    public async Task AssignRoleToUserAsync(Guid userId, Guid roleId, string assignedBy)
    {
        var tenantId = _tenantContextAccessor.GetCurrentTenantId();
        
        var role = await _dbContext.Set<Role>().FirstOrDefaultAsync(r => r.Id == roleId && r.TenantId == tenantId) 
            ?? throw new ArgumentException($"Role with ID {roleId} not found in the current tenant");
            
        var userRole = new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoleId = roleId,
            TenantId = tenantId,
            CreatedBy = assignedBy,
            CreatedDate = DateTime.UtcNow
        };
        
        await _dbContext.Set<UserRole>().AddAsync(userRole);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RemoveRoleFromUserAsync(Guid userId, Guid roleId, string removedBy)
    {
        var tenantId = _tenantContextAccessor.GetCurrentTenantId();
        
        var userRole = await _dbContext.Set<UserRole>()
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId && ur.TenantId == tenantId && !ur.IsDeleted) 
            ?? throw new ArgumentException($"User {userId} does not have role {roleId} in the current tenant");
            
        userRole.DeletedBy = removedBy;
        userRole.DeletedDate = DateTime.UtcNow;
        userRole.IsDeleted = true;
        
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Role> CreateRoleAsync(string name, string? description, bool isSystemRole, string createdBy)
    {
        var tenantId = _tenantContextAccessor.GetCurrentTenantId();
        
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            IsSystemRole = isSystemRole,
            TenantId = tenantId,
            CreatedBy = createdBy,
            CreatedDate = DateTime.UtcNow
        };
        
        await _dbContext.Set<Role>().AddAsync(role);
        await _dbContext.SaveChangesAsync();
        
        return role;
    }

    public async Task<Permission> CreatePermissionAsync(string name, string systemName, string? description, string? group, string createdBy)
    {
        var tenantId = _tenantContextAccessor.GetCurrentTenantId();
        
        // Check if the permission already exists in this tenant
        var existingPermission = await _dbContext.Set<Permission>()
            .FirstOrDefaultAsync(p => p.SystemName == systemName && p.TenantId == tenantId && !p.IsDeleted);
            
        if (existingPermission != null)
        {
            throw new ArgumentException($"Permission with system name '{systemName}' already exists in the current tenant");
        }
        
        var permission = new Permission
        {
            Id = Guid.NewGuid(),
            Name = name,
            SystemName = systemName,
            Description = description,
            Group = group,
            TenantId = tenantId,
            CreatedBy = createdBy,
            CreatedDate = DateTime.UtcNow
        };
        
        await _dbContext.Set<Permission>().AddAsync(permission);
        await _dbContext.SaveChangesAsync();
        
        return permission;
    }

    public async Task AssignPermissionToRoleAsync(Guid roleId, Guid permissionId, string assignedBy)
    {
        var tenantId = _tenantContextAccessor.GetCurrentTenantId();
        
        var role = await _dbContext.Set<Role>().FirstOrDefaultAsync(r => r.Id == roleId && r.TenantId == tenantId) 
            ?? throw new ArgumentException($"Role with ID {roleId} not found in the current tenant");
            
        var permission = await _dbContext.Set<Permission>().FirstOrDefaultAsync(p => p.Id == permissionId && p.TenantId == tenantId)
            ?? throw new ArgumentException($"Permission with ID {permissionId} not found in the current tenant");
            
        // Check if the role already has this permission
        var existingRolePermission = await _dbContext.Set<RolePermission>()
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId && rp.TenantId == tenantId && !rp.IsDeleted);
            
        if (existingRolePermission != null)
        {
            throw new ArgumentException($"Role {roleId} already has permission {permissionId} in the current tenant");
        }
            
        var rolePermission = new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            PermissionId = permissionId,
            TenantId = tenantId,
            CreatedBy = assignedBy,
            CreatedDate = DateTime.UtcNow
        };
        
        await _dbContext.Set<RolePermission>().AddAsync(rolePermission);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId, string removedBy)
    {
        var tenantId = _tenantContextAccessor.GetCurrentTenantId();
        
        var rolePermission = await _dbContext.Set<RolePermission>()
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId && rp.TenantId == tenantId && !rp.IsDeleted)
            ?? throw new ArgumentException($"Role {roleId} does not have permission {permissionId} in the current tenant");
            
        rolePermission.DeletedBy = removedBy;
        rolePermission.DeletedDate = DateTime.UtcNow;
        rolePermission.IsDeleted = true;
        
        await _dbContext.SaveChangesAsync();
    }
} 