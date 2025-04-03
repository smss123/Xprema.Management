using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xprema.Framework.Entities.Common;
using Xprema.Framework.Entities.MultiTenancy;

namespace Xprema.Framework.Entities.HistoryFeature;

public class AuditLogService : IAuditLogService
{
    private readonly DbContext _dbContext;
    private readonly ILogger<AuditLogService> _logger;
    private readonly ITenantContextAccessor _tenantContextAccessor;
    
    public AuditLogService(
        DbContext dbContext,
        ILogger<AuditLogService> logger,
        ITenantContextAccessor tenantContextAccessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _tenantContextAccessor = tenantContextAccessor;
    }
    
    public async Task LogActivityAsync(string userId, string activity, string? entityType = null, string? entityId = null, string? oldValues = null, string? newValues = null)
    {
        try
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Activity = activity,
                EntityType = entityType,
                EntityId = entityId,
                OldValues = oldValues,
                NewValues = newValues,
                Timestamp = DateTime.UtcNow,
                TenantId = _tenantContextAccessor.GetCurrentTenantId()
            };
            
            _dbContext.Add(auditLog);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging audit activity: {Activity}", activity);
        }
    }
    
    public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(string? userId = null, string? entityType = null, string? entityId = null, DateTime? startDate = null, DateTime? endDate = null, int skip = 0, int take = 50)
    {
        var query = _dbContext.Set<AuditLog>().AsQueryable();
        
        // Filter by current tenant
        var tenantId = _tenantContextAccessor.GetCurrentTenantId();
        query = query.Where(a => a.TenantId == tenantId);
        
        // Apply filters
        if (!string.IsNullOrEmpty(userId))
            query = query.Where(a => a.UserId == userId);
            
        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(a => a.EntityType == entityType);
            
        if (!string.IsNullOrEmpty(entityId))
            query = query.Where(a => a.EntityId == entityId);
            
        if (startDate.HasValue)
            query = query.Where(a => a.Timestamp >= startDate.Value);
            
        if (endDate.HasValue)
            query = query.Where(a => a.Timestamp <= endDate.Value);
            
        // Order and paginate
        return await query
            .OrderByDescending(a => a.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }
} 