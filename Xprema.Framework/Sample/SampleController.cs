using Microsoft.AspNetCore.Mvc;
using Xprema.Framework.Entities.Permission;

namespace Xprema.Framework.Sample;

/// <summary>
/// Sample controller that demonstrates how to use permission-based authorization
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SampleController : ControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    
    public SampleController(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }
    
    /// <summary>
    /// Endpoint that requires a specific permission (Administration.SystemAccess)
    /// </summary>
    [HttpGet("admin")]
    [PermissionAuthorize(DefaultPermissionProvider.SystemAccess)]
    public IActionResult AdminOnlyEndpoint()
    {
        return Ok(new { message = "This is an admin-only endpoint" });
    }
    
    /// <summary>
    /// Endpoint that requires user view permission (UserManagement.UserView)
    /// </summary>
    [HttpGet("users")]
    [PermissionAuthorize(DefaultPermissionProvider.UserView)]
    public IActionResult GetUsers()
    {
        return Ok(new { message = "This endpoint returns users" });
    }
    
    /// <summary>
    /// Endpoint that requires multiple permissions (any of them)
    /// </summary>
    [HttpGet("multi-any")]
    [PermissionAuthorize(false, DefaultPermissionProvider.UserCreate, DefaultPermissionProvider.UserEdit)]
    public IActionResult MultiPermissionAny()
    {
        return Ok(new { message = "This endpoint requires any of the specified permissions" });
    }
    
    /// <summary>
    /// Endpoint that requires multiple permissions (all of them)
    /// </summary>
    [HttpGet("multi-all")]
    [PermissionAuthorize(true, DefaultPermissionProvider.UserCreate, DefaultPermissionProvider.UserEdit)]
    public IActionResult MultiPermissionAll()
    {
        return Ok(new { message = "This endpoint requires all of the specified permissions" });
    }
    
    /// <summary>
    /// Endpoint that checks permissions programmatically
    /// </summary>
    [HttpGet("programmatic")]
    public async Task<IActionResult> ProgrammaticCheck()
    {
        // Check if user has a specific permission
        if (await _authorizationService.IsGrantedAsync(DefaultPermissionProvider.UserCreate))
        {
            // User has permission to create users
            return Ok(new { message = "You can create users" });
        }
        
        // Check if user has any of the specified permissions
        if (await _authorizationService.IsGrantedAnyAsync(
            DefaultPermissionProvider.UserEdit,
            DefaultPermissionProvider.UserDelete))
        {
            // User has permission to edit or delete users
            return Ok(new { message = "You can edit or delete users" });
        }
        
        // Get all user permissions
        var permissions = await _authorizationService.GetPermissionsAsync();
        
        // Get all user roles
        var roles = await _authorizationService.GetRolesAsync();
        
        return Ok(new 
        { 
            message = "You don't have specific permissions", 
            permissions, 
            roles 
        });
    }
} 