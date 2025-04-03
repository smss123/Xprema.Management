using Microsoft.EntityFrameworkCore;
using Xprema.Framework.Entities.MultiTenancy;

namespace Xprema.Framework.Tests;

public class TenantServiceTests : TestBase
{
    [Fact]
    public async Task CreateTenant_ShouldReturnValidTenant()
    {
        // Arrange
        string tenantName = "Test Tenant";
        string tenantIdentifier = "test-tenant";
        string description = "Test tenant description";
        
        // Act
        var tenant = await TenantService.CreateTenantAsync(
            tenantName, 
            tenantIdentifier, 
            description, 
            TenantStorageMode.SharedDatabase, 
            null, 
            "system");
        
        // Assert
        Assert.NotNull(tenant);
        Assert.Equal(tenantName, tenant.Name);
        Assert.Equal(tenantIdentifier, tenant.Identifier);
        Assert.Equal(description, tenant.Description);
        Assert.Equal(TenantStorageMode.SharedDatabase, tenant.StorageMode);
        Assert.True(tenant.IsActive);
        Assert.Equal("system", tenant.CreatedBy);
        Assert.NotEqual(Guid.Empty, tenant.Id);
    }
    
    [Fact]
    public async Task GetTenantById_ShouldReturnCorrectTenant()
    {
        // Arrange
        var tenant = await CreateTestTenantAsync();
        
        // Act
        var retrievedTenant = await TenantService.GetTenantByIdAsync(tenant.Id);
        
        // Assert
        Assert.NotNull(retrievedTenant);
        Assert.Equal(tenant.Id, retrievedTenant.Id);
        Assert.Equal(tenant.Name, retrievedTenant.Name);
    }
    
    [Fact]
    public async Task GetTenantByIdentifier_ShouldReturnCorrectTenant()
    {
        // Arrange
        string identifier = "unique-identifier";
        var tenant = await CreateTestTenantAsync(identifier: identifier);
        
        // Act
        var retrievedTenant = await TenantService.GetTenantByIdentifierAsync(identifier);
        
        // Assert
        Assert.NotNull(retrievedTenant);
        Assert.Equal(tenant.Id, retrievedTenant.Id);
        Assert.Equal(tenant.Name, retrievedTenant.Name);
        Assert.Equal(identifier, retrievedTenant.Identifier);
    }
    
    [Fact]
    public async Task GetAllTenants_ShouldReturnAllActiveTenants()
    {
        // Arrange
        await CreateTestTenantAsync(name: "Tenant 1", identifier: "tenant1");
        await CreateTestTenantAsync(name: "Tenant 2", identifier: "tenant2");
        await CreateTestTenantAsync(name: "Tenant 3", identifier: "tenant3");
        
        // Act
        var tenants = await TenantService.GetAllTenantsAsync();
        
        // Assert
        Assert.NotNull(tenants);
        Assert.Equal(3, tenants.Count());
    }
    
    [Fact]
    public async Task UpdateTenant_ShouldUpdateTenantProperties()
    {
        // Arrange
        var tenant = await CreateTestTenantAsync();
        string newName = "Updated Tenant Name";
        string newDescription = "Updated tenant description";
        bool newIsActive = false;
        string connectionString = "Server=localhost;Database=UpdatedTenant;";
        
        // Act
        var updatedTenant = await TenantService.UpdateTenantAsync(
            tenant.Id,
            newName,
            newDescription,
            newIsActive,
            TenantStorageMode.SeparateDatabase,
            connectionString,
            "admin");
        
        // Assert
        Assert.NotNull(updatedTenant);
        Assert.Equal(tenant.Id, updatedTenant.Id);
        Assert.Equal(newName, updatedTenant.Name);
        Assert.Equal(newDescription, updatedTenant.Description);
        Assert.Equal(newIsActive, updatedTenant.IsActive);
        Assert.Equal(TenantStorageMode.SeparateDatabase, updatedTenant.StorageMode);
        Assert.Equal(connectionString, updatedTenant.ConnectionString);
        Assert.Equal("admin", updatedTenant.ModifiedBy);
        Assert.NotNull(updatedTenant.ModifiedDate);
    }
    
    [Fact]
    public async Task DeleteTenant_ShouldMarkTenantAsDeleted()
    {
        // Arrange
        var tenant = await CreateTestTenantAsync();
        
        // Act
        await TenantService.DeleteTenantAsync(tenant.Id, "admin");
        
        // Verify the tenant is marked as deleted but still exists in the database
        var deletedTenant = await DbContext.Tenants.FindAsync(tenant.Id);
        Assert.NotNull(deletedTenant);
        Assert.True(deletedTenant.IsDeleted);
        Assert.Equal("admin", deletedTenant.DeletedBy);
        Assert.NotNull(deletedTenant.DeletedDate);
    }
    
    [Fact]
    public async Task AddTenantSetting_ShouldAddSettingToTenant()
    {
        // Arrange
        var tenant = await CreateTestTenantAsync();
        string key = "theme";
        string value = "dark";
        
        // Act
        await TenantService.AddTenantSettingAsync(tenant.Id, key, value, "admin");
        
        // Assert
        var updatedTenant = await TenantService.GetTenantByIdAsync(tenant.Id);
        Assert.NotNull(updatedTenant);
        Assert.True(updatedTenant.Settings.ContainsKey(key));
        Assert.Equal(value, updatedTenant.Settings[key]);
    }
    
    [Fact]
    public async Task RemoveTenantSetting_ShouldRemoveSettingFromTenant()
    {
        // Arrange
        var tenant = await CreateTestTenantAsync();
        string key = "theme";
        string value = "dark";
        await TenantService.AddTenantSettingAsync(tenant.Id, key, value, "admin");
        
        // Act
        await TenantService.RemoveTenantSettingAsync(tenant.Id, key, "admin");
        
        // Assert
        var updatedTenant = await TenantService.GetTenantByIdAsync(tenant.Id);
        Assert.NotNull(updatedTenant);
        Assert.False(updatedTenant.Settings.ContainsKey(key));
    }
    
    [Fact]
    public async Task AddUserToTenant_ShouldCreateTenantUserAssociation()
    {
        // Arrange
        var tenant = await CreateTestTenantAsync();
        var userId = Guid.NewGuid();
        
        // Act
        var tenantUser = await TenantService.AddUserToTenantAsync(tenant.Id, userId, true, "admin");
        
        // Assert
        Assert.NotNull(tenantUser);
        Assert.Equal(tenant.Id, tenantUser.TenantId);
        Assert.Equal(userId, tenantUser.UserId);
        Assert.True(tenantUser.IsAdmin);
        
        // Verify the association is in the database
        var users = await TenantService.GetTenantUsersAsync(tenant.Id);
        Assert.Single(users);
        Assert.Equal(userId, users.First().UserId);
    }
    
    [Fact]
    public async Task RemoveUserFromTenant_ShouldMarkTenantUserAsDeleted()
    {
        // Arrange
        var tenant = await CreateTestTenantAsync();
        var userId = Guid.NewGuid();
        await TenantService.AddUserToTenantAsync(tenant.Id, userId, false, "admin");
        
        // Act
        await TenantService.RemoveUserFromTenantAsync(tenant.Id, userId, "admin");
        
        // Assert
        var users = await TenantService.GetTenantUsersAsync(tenant.Id);
        Assert.Empty(users); // Should not return deleted tenant-user associations
        
        // Verify the association is marked as deleted but still exists in the database
        var deletedTenantUser = await DbContext.TenantUsers
            .FirstOrDefaultAsync(tu => tu.TenantId == tenant.Id && tu.UserId == userId);
        Assert.NotNull(deletedTenantUser);
        Assert.True(deletedTenantUser.IsDeleted);
        Assert.Equal("admin", deletedTenantUser.DeletedBy);
        Assert.NotNull(deletedTenantUser.DeletedDate);
    }
} 