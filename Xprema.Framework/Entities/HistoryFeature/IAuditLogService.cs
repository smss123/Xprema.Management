namespace Xprema.Framework.Entities.HistoryFeature;

/// <summary>
/// Service for logging and retrieving audit activities
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Logs an activity
    /// </summary>
    Task LogActivityAsync(string userId, string activity, string? entityType = null, string? entityId = null, string? oldValues = null, string? newValues = null);
    
    /// <summary>
    /// Gets audit logs with filtering and pagination
    /// </summary>
    Task<IEnumerable<AuditLog>> GetAuditLogsAsync(string? userId = null, string? entityType = null, string? entityId = null, DateTime? startDate = null, DateTime? endDate = null, int skip = 0, int take = 50);
} 