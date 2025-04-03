using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xprema.Framework.Entities.Identity;
using Xprema.Framework.Entities.Permission;
using Xunit;

namespace Xprema.Framework.Tests.PermissionTests;

public class AuthorizationServiceTests : TestBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IPermissionService _permissionService;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly ApplicationUser _testUser;
    private readonly Guid _testUserId;
    
    public AuthorizationServiceTests()
    {
        _permissionService = ServiceProvider.GetRequiredService<IPermissionService>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        
        // Create a test user
        _testUserId = Guid.NewGuid();
        _testUser = new ApplicationUser
        {
            Id = _testUserId,
            Username = "testuser",
            Email = "test@example.com",
            CreatedBy = "system",
            CreatedDate = DateTime.UtcNow
            // Note: Make sure the ApplicationUser properties match what's defined in the actual class
        };
        
        // Setup HTTP context with user claims
        var httpContext = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new Claim(ClaimTypes.Name, "testuser")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        httpContext.User = principal;
        
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        
        // Create a service provider for the authorization service
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(x => x.GetService(typeof(IHttpContextAccessor)))
            .Returns(_httpContextAccessorMock.Object);
        serviceProviderMock
            .Setup(x => x.GetService(typeof(IPermissionService)))
            .Returns(_permissionService);
        
        // Create the authorization service directly
        _authorizationService = new AuthorizationService(_permissionService, serviceProviderMock.Object);
        
        // Save the test user to the database
        DbContext.Set<ApplicationUser>().Add(_testUser);
        DbContext.SaveChanges();
    }
    
    [Fact]
    public async Task IsGrantedAsync_WithPermission_ShouldReturnTrue()
    {
        // Arrange
        var permissionName = "Test.Permission";
        var role = await _permissionService.CreateRoleAsync("TestRole", null, false, "createdBy");
        var permission = await _permissionService.CreatePermissionAsync("Test Permission", permissionName, "Test permission", "TestGroup", "createdBy");
        
        await _permissionService.AssignPermissionToRoleAsync(role.Id, permission.Id, "createdBy");
        await _permissionService.AssignRoleToUserAsync(_testUserId, role.Id, "createdBy");
        
        // Act
        var result = await _authorizationService.IsGrantedAsync(permissionName);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public async Task IsGrantedAsync_WithoutPermission_ShouldReturnFalse()
    {
        // Arrange
        var permissionName = "Test.MissingPermission";
        
        // Act
        var result = await _authorizationService.IsGrantedAsync(permissionName);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public async Task IsGrantedAsync_WithUserId_ShouldCheckSpecificUser()
    {
        // Arrange
        var permissionName = "Test.UserPermission";
        var role = await _permissionService.CreateRoleAsync("UserTestRole", null, false, "createdBy");
        var permission = await _permissionService.CreatePermissionAsync("User Test Permission", permissionName, "Test permission for user", "TestGroup", "createdBy");
        
        await _permissionService.AssignPermissionToRoleAsync(role.Id, permission.Id, "createdBy");
        await _permissionService.AssignRoleToUserAsync(_testUserId, role.Id, "createdBy");
        
        // Act
        var result = await _authorizationService.IsGrantedAsync(_testUserId, permissionName);
        var resultOtherUser = await _authorizationService.IsGrantedAsync(Guid.NewGuid(), permissionName);
        
        // Assert
        Assert.True(result);
        Assert.False(resultOtherUser);
    }
    
    [Fact]
    public async Task IsGrantedAllAsync_WithAllPermissions_ShouldReturnTrue()
    {
        // Arrange
        var permission1 = "Test.AllPermission1";
        var permission2 = "Test.AllPermission2";
        
        var role = await _permissionService.CreateRoleAsync("AllTestRole", null, false, "createdBy");
        var perm1 = await _permissionService.CreatePermissionAsync("Test Permission 1", permission1, "Test permission 1", "TestGroup", "createdBy");
        var perm2 = await _permissionService.CreatePermissionAsync("Test Permission 2", permission2, "Test permission 2", "TestGroup", "createdBy");
        
        await _permissionService.AssignPermissionToRoleAsync(role.Id, perm1.Id, "createdBy");
        await _permissionService.AssignPermissionToRoleAsync(role.Id, perm2.Id, "createdBy");
        await _permissionService.AssignRoleToUserAsync(_testUserId, role.Id, "createdBy");
        
        // Act
        var result = await _authorizationService.IsGrantedAllAsync(permission1, permission2);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public async Task IsGrantedAllAsync_WithMissingPermission_ShouldReturnFalse()
    {
        // Arrange
        var permission1 = "Test.SomePermission1";
        var permission2 = "Test.MissingPermission2";
        
        var role = await _permissionService.CreateRoleAsync("SomeTestRole", null, false, "createdBy");
        var perm1 = await _permissionService.CreatePermissionAsync("Some Permission 1", permission1, "Some permission 1", "TestGroup", "createdBy");
        
        await _permissionService.AssignPermissionToRoleAsync(role.Id, perm1.Id, "createdBy");
        await _permissionService.AssignRoleToUserAsync(_testUserId, role.Id, "createdBy");
        
        // Act
        var result = await _authorizationService.IsGrantedAllAsync(permission1, permission2);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public async Task IsGrantedAnyAsync_WithSomePermissions_ShouldReturnTrue()
    {
        // Arrange
        var permission1 = "Test.AnyPermission1";
        var permission2 = "Test.MissingAnyPermission";
        
        var role = await _permissionService.CreateRoleAsync("AnyTestRole", null, false, "createdBy");
        var perm1 = await _permissionService.CreatePermissionAsync("Any Permission 1", permission1, "Any permission 1", "TestGroup", "createdBy");
        
        await _permissionService.AssignPermissionToRoleAsync(role.Id, perm1.Id, "createdBy");
        await _permissionService.AssignRoleToUserAsync(_testUserId, role.Id, "createdBy");
        
        // Act
        var result = await _authorizationService.IsGrantedAnyAsync(permission1, permission2);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public async Task IsGrantedAnyAsync_WithNoPermissions_ShouldReturnFalse()
    {
        // Arrange
        var permission1 = "Test.MissingAny1";
        var permission2 = "Test.MissingAny2";
        
        // Act
        var result = await _authorizationService.IsGrantedAnyAsync(permission1, permission2);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public async Task GetPermissionsAsync_ShouldReturnUserPermissions()
    {
        // Arrange
        var permissionName = "Test.GetPermission";
        var role = await _permissionService.CreateRoleAsync("GetPermRole", null, false, "createdBy");
        var permission = await _permissionService.CreatePermissionAsync("Get Permission", permissionName, "Get permission test", "TestGroup", "createdBy");
        
        await _permissionService.AssignPermissionToRoleAsync(role.Id, permission.Id, "createdBy");
        await _permissionService.AssignRoleToUserAsync(_testUserId, role.Id, "createdBy");
        
        // Act
        var permissions = await _authorizationService.GetPermissionsAsync();
        
        // Assert
        Assert.NotEmpty(permissions);
        Assert.Contains(permissions, p => p.SystemName == permissionName);
    }
    
    [Fact]
    public async Task GetRolesAsync_ShouldReturnUserRoles()
    {
        // Arrange
        var roleName = "TestGetRole";
        var role = await _permissionService.CreateRoleAsync(roleName, null, false, "createdBy");
        await _permissionService.AssignRoleToUserAsync(_testUserId, role.Id, "createdBy");
        
        // Act
        var roles = await _authorizationService.GetRolesAsync();
        
        // Assert
        Assert.NotEmpty(roles);
        Assert.Contains(roles, r => r.Name == roleName);
    }
} 