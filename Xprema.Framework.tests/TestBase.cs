using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using Xprema.Framework.Entities.HistoryFeature;
using Xprema.Framework.Entities.Identity;
using Xprema.Framework.Entities.MultiTenancy;
using Xprema.Framework.Entities.Permission;

namespace Xprema.Framework.Tests;

public abstract class TestBase : IDisposable
{
    protected readonly TestDbContext DbContext;
    protected readonly ITenantContextAccessor TenantContextAccessor;
    protected readonly ITenantService TenantService;
    protected readonly IPermissionService PermissionService;
    protected readonly IAuthenticationService AuthenticationService;
    protected readonly IAuditLogService AuditLogService;
    protected readonly ITokenService TokenService;
    protected readonly ServiceProvider ServiceProvider;
    protected readonly IConfiguration Configuration;
    
    protected TestBase()
    {
        // Create configuration
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Jwt:Secret"] = "TestSecretKey12345678901234567890TestSecretKey",
                ["Jwt:Issuer"] = "XpremaTest",
                ["Jwt:Audience"] = "XpremaTestUsers"
            })
            .Build();
            
        Configuration = configuration;
        
        // Create a service collection
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging();
        
        // Add the in-memory database
        services.AddDbContext<TestDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
            
        // Register DbContext as both TestDbContext and DbContext for service resolution
        services.AddScoped<DbContext>(provider => provider.GetRequiredService<TestDbContext>());
        
        // Add HttpContextAccessor mock
        var httpContextAccessorMock = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        services.AddSingleton(httpContextAccessorMock.Object);
        
        // Add services
        services.AddSingleton<IConfiguration>(configuration);
        services.AddScoped<ITenantContextAccessor, TenantContextAccessor<TestDbContext>>();
        services.AddScoped<ITenantService, TenantService<TestDbContext>>();
        services.AddScoped<IPermissionService, PermissionService<TestDbContext>>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IAuthenticationService>(provider => 
            new AuthenticationService(
                provider.GetRequiredService<TestDbContext>(),
                provider.GetRequiredService<ILogger<AuthenticationService>>(),
                provider.GetRequiredService<ITenantService>(),
                provider.GetRequiredService<ITenantContextAccessor>(),
                provider.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
                provider.GetRequiredService<ITokenService>()
            ));
        
        // Build the service provider
        ServiceProvider = services.BuildServiceProvider();
        
        // Get services
        DbContext = ServiceProvider.GetRequiredService<TestDbContext>();
        TenantContextAccessor = ServiceProvider.GetRequiredService<ITenantContextAccessor>();
        TenantService = ServiceProvider.GetRequiredService<ITenantService>();
        PermissionService = ServiceProvider.GetRequiredService<IPermissionService>();
        AuthenticationService = ServiceProvider.GetRequiredService<IAuthenticationService>();
        AuditLogService = ServiceProvider.GetRequiredService<IAuditLogService>();
        TokenService = ServiceProvider.GetRequiredService<ITokenService>();
    }
    
    public void Dispose()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Dispose();
        ServiceProvider.Dispose();
    }
    
    protected async Task<Tenant> CreateTestTenantAsync(string name = "Test Tenant", string identifier = "test")
    {
        return await TenantService.CreateTenantAsync(
            name: name,
            identifier: identifier,
            description: "Test tenant description",
            storageMode: TenantStorageMode.SharedDatabase,
            connectionString: null,
            createdBy: "system");
    }
    
    protected async Task<(Tenant, Guid)> CreateTestTenantWithUserAsync(string name = "Test Tenant", string identifier = "test")
    {
        var tenant = await CreateTestTenantAsync(name, identifier);
        var userId = Guid.NewGuid();
        
        // Set current tenant
        TenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        // Add user to tenant
        await TenantService.AddUserToTenantAsync(tenant.Id, userId, isAdmin: true, "system");
        
        return (tenant, userId);
    }
    
    protected async Task<(Tenant, Guid, Role, Permission)> CreateTestTenantWithUserAndPermissionAsync(
        string name = "Test Tenant", 
        string identifier = "test",
        string roleName = "Admin",
        string permissionName = "TestPermission",
        string permissionSystemName = "Test.Permission")
    {
        var (tenant, userId) = await CreateTestTenantWithUserAsync(name, identifier);
        
        // Set current tenant
        TenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        // Create role
        var role = await PermissionService.CreateRoleAsync(roleName, "Test role", isSystemRole: true, "system");
        
        // Create permission
        var permission = await PermissionService.CreatePermissionAsync(
            permissionName, 
            permissionSystemName, 
            "Test permission", 
            "Test", 
            "system");
        
        // Assign permission to role
        await PermissionService.AssignPermissionToRoleAsync(role.Id, permission.Id, "system");
        
        // Assign role to user
        await PermissionService.AssignRoleToUserAsync(userId, role.Id, "system");
        
        return (tenant, userId, role, permission);
    }
    
    protected async Task<ApplicationUser> CreateTestUserAsync(
        string username = "testuser",
        string email = "test@example.com",
        string password = "Password123!",
        Guid? tenantId = null)
    {
        // Register user
        var request = new RegisterUserRequest
        {
            Username = username,
            Email = email,
            Password = password,
            DisplayName = "Test User",
            TenantId = tenantId
        };
        
        var result = await AuthenticationService.RegisterUserAsync(request);
        
        if (!result.Succeeded)
        {
            throw new Exception($"Failed to create test user: {result.ErrorMessage}");
        }
        
        // Get the user
        if (!Guid.TryParse(result.UserId, out var userId))
        {
            throw new Exception("Invalid user ID returned");
        }
        
        var user = await DbContext.Users.FindAsync(userId);
        
        if (user == null)
        {
            throw new Exception("User not found after registration");
        }
        
        // Make email confirmed by default for testing
        user.EmailConfirmed = true;
        await DbContext.SaveChangesAsync();
        
        return user;
    }
} 