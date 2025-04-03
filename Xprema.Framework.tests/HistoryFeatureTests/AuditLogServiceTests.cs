using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xprema.Framework.Entities.HistoryFeature;
using Xprema.Framework.Entities.Identity;

namespace Xprema.Framework.Tests.HistoryFeatureTests;

public class AuditLogServiceTests : TestBase
{
    [Fact]
    public async Task LogActivity_ShouldCreateAuditLog()
    {
        // Arrange
        var tenant = await CreateTestTenantAsync();
        TenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        var userId = Guid.NewGuid().ToString();
        var activity = "Created product";
        var entityType = "Product";
        var entityId = "123";
        var newValues = "{\"name\":\"Test Product\",\"price\":10.99}";
        
        // Act
        await AuditLogService.LogActivityAsync(userId, activity, entityType, entityId, null, newValues);
        
        // Assert
        var auditLog = await DbContext.AuditLogs.FirstOrDefaultAsync();
        
        Assert.NotNull(auditLog);
        Assert.Equal(userId, auditLog.UserId);
        Assert.Equal(activity, auditLog.Activity);
        Assert.Equal(entityType, auditLog.EntityType);
        Assert.Equal(entityId, auditLog.EntityId);
        Assert.Null(auditLog.OldValues);
        Assert.Equal(newValues, auditLog.NewValues);
        Assert.Equal(tenant.Id, auditLog.TenantId);
    }
    
    [Fact]
    public async Task GetAuditLogs_ShouldReturnFilteredLogs()
    {
        // Arrange
        var tenant = await CreateTestTenantAsync();
        TenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();
        
        // Add some audit logs
        await CreateAuditLogAsync(userId1, "Created product", "Product", "1");
        await CreateAuditLogAsync(userId1, "Updated product", "Product", "1");
        await CreateAuditLogAsync(userId2, "Created product", "Product", "2");
        await CreateAuditLogAsync(userId2, "Deleted product", "Product", "2");
        await CreateAuditLogAsync(userId1, "Created user", "User", "3");
        
        // Act - filter by userId
        var logsForUser1 = await AuditLogService.GetAuditLogsAsync(userId1);
        
        // Assert
        Assert.Equal(3, logsForUser1.Count());
        Assert.All(logsForUser1, log => Assert.Equal(userId1, log.UserId));
        
        // Act - filter by entityType
        var productLogs = await AuditLogService.GetAuditLogsAsync(null, "Product");
        
        // Assert
        Assert.Equal(4, productLogs.Count());
        Assert.All(productLogs, log => Assert.Equal("Product", log.EntityType));
        
        // Act - filter by entityId
        var product1Logs = await AuditLogService.GetAuditLogsAsync(null, "Product", "1");
        
        // Assert
        Assert.Equal(2, product1Logs.Count());
        Assert.All(product1Logs, log => Assert.Equal("1", log.EntityId));
        
        // Act - combined filters
        var user1Product1Logs = await AuditLogService.GetAuditLogsAsync(userId1, "Product", "1");
        
        // Assert
        Assert.Equal(2, user1Product1Logs.Count());
        Assert.All(user1Product1Logs, log => 
        {
            Assert.Equal(userId1, log.UserId);
            Assert.Equal("Product", log.EntityType);
            Assert.Equal("1", log.EntityId);
        });
    }
    
    [Fact]
    public async Task GetAuditLogs_ShouldFilterByDateRange()
    {
        // Arrange
        var tenant = await CreateTestTenantAsync();
        TenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        var userId = Guid.NewGuid().ToString();
        
        // Create logs with specific dates
        var yesterday = DateTime.UtcNow.AddDays(-1);
        var twoDaysAgo = DateTime.UtcNow.AddDays(-2);
        var threeDaysAgo = DateTime.UtcNow.AddDays(-3);
        
        await CreateAuditLogAsync(userId, "Old log", "Test", "1", timestamp: threeDaysAgo);
        await CreateAuditLogAsync(userId, "Medium log", "Test", "2", timestamp: twoDaysAgo);
        await CreateAuditLogAsync(userId, "Recent log", "Test", "3", timestamp: yesterday);
        
        // Act - filter by start date only
        var recentLogs = await AuditLogService.GetAuditLogsAsync(
            startDate: twoDaysAgo.AddHours(-1)); // Include the medium log
        
        // Assert
        Assert.Equal(2, recentLogs.Count());
        
        // Act - filter by end date only
        var oldLogs = await AuditLogService.GetAuditLogsAsync(
            endDate: twoDaysAgo.AddHours(1)); // Include the medium log
        
        // Assert
        Assert.Equal(2, oldLogs.Count());
        
        // Act - filter by date range
        var mediumLogs = await AuditLogService.GetAuditLogsAsync(
            startDate: threeDaysAgo.AddHours(1), // After the old log
            endDate: yesterday.AddHours(-1)); // Before the recent log
        
        // Assert
        Assert.Single(mediumLogs);
        Assert.Equal("Medium log", mediumLogs.First().Activity);
    }
    
    [Fact]
    public async Task GetAuditLogs_ShouldRespectPagination()
    {
        // Arrange
        var tenant = await CreateTestTenantAsync();
        TenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        var userId = Guid.NewGuid().ToString();
        
        // Create 10 logs
        for (int i = 0; i < 10; i++)
        {
            await CreateAuditLogAsync(userId, $"Activity {i}", "Test", i.ToString());
        }
        
        // Act - first page (default 50)
        var allLogs = await AuditLogService.GetAuditLogsAsync();
        
        // Assert
        Assert.Equal(10, allLogs.Count());
        
        // Act - first page with limit
        var firstPage = await AuditLogService.GetAuditLogsAsync(skip: 0, take: 3);
        
        // Assert
        Assert.Equal(3, firstPage.Count());
        
        // Act - second page
        var secondPage = await AuditLogService.GetAuditLogsAsync(skip: 3, take: 3);
        
        // Assert
        Assert.Equal(3, secondPage.Count());
        Assert.Empty(firstPage.Intersect(secondPage));
        
        // Act - last page (partial)
        var lastPage = await AuditLogService.GetAuditLogsAsync(skip: 9, take: 3);
        
        // Assert
        Assert.Single(lastPage);
    }
    
    private async Task<AuditLog> CreateAuditLogAsync(
        string userId, 
        string activity, 
        string entityType, 
        string entityId, 
        string? oldValues = null, 
        string? newValues = null,
        DateTime? timestamp = null)
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
            Timestamp = timestamp ?? DateTime.UtcNow,
            TenantId = TenantContextAccessor.GetCurrentTenantId(),
            CreatedBy = "system",
            CreatedDate = DateTime.UtcNow
        };
        
        DbContext.AuditLogs.Add(auditLog);
        await DbContext.SaveChangesAsync();
        
        return auditLog;
    }
} 