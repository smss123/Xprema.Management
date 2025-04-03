using Xprema.Framework.Entities.MultiTenancy;

namespace Xprema.Framework.Tests;

public class TenantContextAccessorTests : TestBase
{
    [Fact]
    public void GetCurrentTenantId_ShouldReturnEmptyGuidWhenNoTenantIsSet()
    {
        // Act
        var tenantId = TenantContextAccessor.GetCurrentTenantId();
        
        // Assert
        Assert.Equal(Guid.Empty, tenantId);
    }
    
    [Fact]
    public void SetCurrentTenantId_ShouldSetTenantId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        
        // Act
        TenantContextAccessor.SetCurrentTenantId(tenantId);
        var currentTenantId = TenantContextAccessor.GetCurrentTenantId();
        
        // Assert
        Assert.Equal(tenantId, currentTenantId);
    }
    
    [Fact]
    public async Task GetCurrentTenantAsync_ShouldReturnNullWhenNoTenantIsSet()
    {
        // Act
        var tenant = await TenantContextAccessor.GetCurrentTenantAsync();
        
        // Assert
        Assert.Null(tenant);
    }
    
    [Fact]
    public async Task GetCurrentTenantAsync_ShouldReturnCorrectTenant()
    {
        // Arrange
        var createdTenant = await CreateTestTenantAsync();
        
        // Make sure the tenant is in the database
        await DbContext.SaveChangesAsync();
        
        // Verify the tenant can be found directly from the DbContext
        var directlyFoundTenant = await DbContext.Tenants.FindAsync(createdTenant.Id);
        Assert.NotNull(directlyFoundTenant);
        
        // Set current tenant ID
        TenantContextAccessor.SetCurrentTenantId(createdTenant.Id);
        
        // Act - Now use the TenantContextAccessor to get the tenant
        var tenant = await TenantContextAccessor.GetCurrentTenantAsync();
        
        // Assert
        Assert.NotNull(tenant);
        Assert.Equal(createdTenant.Id, tenant.Id);
        Assert.Equal(createdTenant.Name, tenant.Name);
    }
    
    [Fact]
    public void TenantContext_ShouldBeIsolatedBetweenThreads()
    {
        // Arrange
        var tenantId1 = Guid.NewGuid();
        var tenantId2 = Guid.NewGuid();
        var manualResetEvent1 = new ManualResetEventSlim(false);
        var manualResetEvent2 = new ManualResetEventSlim(false);
        Guid? thread1TenantId = null;
        Guid? thread2TenantId = null;
        
        // Act
        var thread1 = new Thread(() =>
        {
            TenantContextAccessor.SetCurrentTenantId(tenantId1);
            manualResetEvent1.Set(); // Signal thread1 has set the tenant ID
            manualResetEvent2.Wait(); // Wait for thread2 to set its tenant ID
            thread1TenantId = TenantContextAccessor.GetCurrentTenantId();
        });
        
        var thread2 = new Thread(() =>
        {
            manualResetEvent1.Wait(); // Wait for thread1 to set its tenant ID
            TenantContextAccessor.SetCurrentTenantId(tenantId2);
            manualResetEvent2.Set(); // Signal thread2 has set the tenant ID
            thread2TenantId = TenantContextAccessor.GetCurrentTenantId();
        });
        
        thread1.Start();
        thread2.Start();
        
        thread1.Join();
        thread2.Join();
        
        // Assert
        Assert.Equal(tenantId1, thread1TenantId);
        Assert.Equal(tenantId2, thread2TenantId);
        Assert.NotEqual(thread1TenantId, thread2TenantId);
    }
} 