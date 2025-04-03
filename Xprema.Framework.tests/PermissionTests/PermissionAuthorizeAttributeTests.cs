using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xprema.Framework.Entities.Permission;
using Xunit;

namespace Xprema.Framework.Tests.PermissionTests;

public class PermissionAuthorizeAttributeTests
{
    private readonly Mock<Xprema.Framework.Entities.Permission.IAuthorizationService> _authServiceMock;
    private readonly AuthorizationFilterContext _context;
    private readonly ClaimsPrincipal _user;
    private readonly DefaultHttpContext _httpContext;
    
    public PermissionAuthorizeAttributeTests()
    {
        _authServiceMock = new Mock<Xprema.Framework.Entities.Permission.IAuthorizationService>();
        
        // Create a claims principal for the user
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, "testuser")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        _user = new ClaimsPrincipal(identity);
        
        // Create HTTP context
        _httpContext = new DefaultHttpContext
        {
            User = _user
        };
        
        // Setup service provider with mocked auth service
        var serviceProvider = new ServiceCollection()
            .AddSingleton(_authServiceMock.Object)
            .BuildServiceProvider();
        _httpContext.RequestServices = serviceProvider;
        
        // Create ActionContext
        var actionContext = new ActionContext(
            _httpContext,
            new RouteData(),
            new ActionDescriptor());
            
        // Create filter context with empty filters
        _context = new AuthorizationFilterContext(
            actionContext,
            new List<IFilterMetadata>());
    }
    
    [Fact]
    public async Task OnAuthorizationAsync_WithSinglePermission_ShouldCallIsGrantedAsync()
    {
        // Arrange
        var permissionName = "Test.Permission";
        var attribute = new PermissionAuthorizeAttribute(permissionName);
        
        _authServiceMock
            .Setup(x => x.IsGrantedAsync(permissionName))
            .ReturnsAsync(true);
            
        // Act
        await attribute.OnAuthorizationAsync(_context);
        
        // Assert
        _authServiceMock.Verify(x => x.IsGrantedAsync(permissionName), Times.Once);
        Assert.Null(_context.Result); // No result means authorization passed
    }
    
    [Fact]
    public async Task OnAuthorizationAsync_WithSinglePermissionDenied_ShouldSetForbidResult()
    {
        // Arrange
        var permissionName = "Test.Permission";
        var attribute = new PermissionAuthorizeAttribute(permissionName);
        
        _authServiceMock
            .Setup(x => x.IsGrantedAsync(permissionName))
            .ReturnsAsync(false);
            
        // Act
        await attribute.OnAuthorizationAsync(_context);
        
        // Assert
        _authServiceMock.Verify(x => x.IsGrantedAsync(permissionName), Times.Once);
        Assert.IsType<ForbidResult>(_context.Result);
    }
    
    [Fact]
    public async Task OnAuthorizationAsync_WithMultiplePermissionsAny_ShouldCallIsGrantedAnyAsync()
    {
        // Arrange
        var permissions = new[] { "Test.Permission1", "Test.Permission2" };
        var attribute = new PermissionAuthorizeAttribute(false, permissions);
        
        _authServiceMock
            .Setup(x => x.IsGrantedAnyAsync(permissions))
            .ReturnsAsync(true);
            
        // Act
        await attribute.OnAuthorizationAsync(_context);
        
        // Assert
        _authServiceMock.Verify(x => x.IsGrantedAnyAsync(permissions), Times.Once);
        Assert.Null(_context.Result); // No result means authorization passed
    }
    
    [Fact]
    public async Task OnAuthorizationAsync_WithMultiplePermissionsAll_ShouldCallIsGrantedAllAsync()
    {
        // Arrange
        var permissions = new[] { "Test.Permission1", "Test.Permission2" };
        var attribute = new PermissionAuthorizeAttribute(true, permissions);
        
        _authServiceMock
            .Setup(x => x.IsGrantedAllAsync(permissions))
            .ReturnsAsync(true);
            
        // Act
        await attribute.OnAuthorizationAsync(_context);
        
        // Assert
        _authServiceMock.Verify(x => x.IsGrantedAllAsync(permissions), Times.Once);
        Assert.Null(_context.Result); // No result means authorization passed
    }
    
    [Fact]
    public async Task OnAuthorizationAsync_WhenAnonymousAllowed_ShouldSkipAuthorization()
    {
        // Arrange
        var permissionName = "Test.Permission";
        var attribute = new PermissionAuthorizeAttribute(permissionName);
        
        // Add AllowAnonymous attribute to context
        var actionDescriptor = new ActionDescriptor();
        actionDescriptor.EndpointMetadata = new List<object> { new AllowAnonymousAttribute() };
        
        var actionContext = new ActionContext(
            _httpContext,
            new RouteData(),
            actionDescriptor);
            
        var context = new AuthorizationFilterContext(
            actionContext,
            new List<IFilterMetadata>());
            
        // Act
        await attribute.OnAuthorizationAsync(context);
        
        // Assert
        _authServiceMock.Verify(x => x.IsGrantedAsync(It.IsAny<string>()), Times.Never);
        Assert.Null(context.Result);
    }
    
    [Fact]
    public async Task OnAuthorizationAsync_WhenUserNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        var permissionName = "Test.Permission";
        var attribute = new PermissionAuthorizeAttribute(permissionName);
        
        // Create unauthenticated user
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        var httpContext = new DefaultHttpContext
        {
            User = anonymousUser,
            RequestServices = _httpContext.RequestServices
        };
        
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor());
            
        var context = new AuthorizationFilterContext(
            actionContext,
            new List<IFilterMetadata>());
            
        // Act
        await attribute.OnAuthorizationAsync(context);
        
        // Assert
        _authServiceMock.Verify(x => x.IsGrantedAsync(It.IsAny<string>()), Times.Never);
        Assert.IsType<UnauthorizedResult>(context.Result);
    }
} 