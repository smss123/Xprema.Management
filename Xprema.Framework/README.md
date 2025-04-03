# Xprema.Framework Permission System

This module provides a flexible role-based permission system for ASP.NET Core applications with multi-tenancy support.

## Features

- Role-based permission system
- Attribute-based authorization for controllers and actions
- Soft delete support for all entities
- History tracking for all changes
- Easy integration with ASP.NET Core dependency injection
- **Multi-tenancy support** with different resolution strategies

## Setup

1. Register the permission system in your `Program.cs` or `Startup.cs`:

```csharp
// Add the permission system with multi-tenancy support
services.AddXpremaPermissions<YourDbContext>();

// Configure MVC to use permission authorization
services.AddControllers(options => 
{
    options.AddPermissionAuthorization();
});

// In Configure method, add tenant resolution middleware
app.UseTenantResolution(TenantResolutionStrategy.Header); // or any other strategy
```

2. Add permission and tenant entities to your DbContext:

```csharp
public class YourDbContext : DbContext
{
    // Other DbSets...
    
    // Permission entities
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    
    // Tenant entities
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantUser> TenantUsers { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure entity relationships for permission entities
        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId);
            
        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId);
            
        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);
            
        // Configure entity relationships for tenant entities
        modelBuilder.Entity<TenantUser>()
            .HasOne(tu => tu.Tenant)
            .WithMany(t => t.TenantUsers)
            .HasForeignKey(tu => tu.TenantId);
            
        // Optional: Configure your user entity
        // modelBuilder.Entity<YourUserEntity>()
        //     .HasMany(u => u.UserRoles)
        //     .WithOne()
        //     .HasForeignKey(ur => ur.UserId);
        //
        // modelBuilder.Entity<YourUserEntity>()
        //     .HasMany(u => u.TenantUsers)
        //     .WithOne()
        //     .HasForeignKey(tu => tu.UserId);
    }
}
```

3. Make your user entity implement the `IAuthorizable` interface:

```csharp
public class YourUserEntity : BaseEntity<Guid>, IAuthorizable
{
    // Your user properties...
    
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<TenantUser> TenantUsers { get; set; } = new List<TenantUser>();
}
```

## Multi-Tenancy

### Tenant Resolution Strategies

The framework supports multiple ways to resolve the current tenant:

1. **Host**: Resolve tenant from the hostname (e.g., `tenant1.example.com`)
2. **Header**: Resolve tenant from an HTTP header (`X-Tenant`)
3. **QueryString**: Resolve tenant from a query parameter (`?tenant=tenant1`)
4. **Route**: Resolve tenant from a route parameter (`/{tenant}/...`)
5. **Cookie**: Resolve tenant from a cookie
6. **Claim**: Resolve tenant from a user claim

Configure the desired strategy when setting up the middleware:

```csharp
app.UseTenantResolution(TenantResolutionStrategy.Header);
```

### Tenant Storage Modes

The framework supports multiple storage modes for tenants:

1. **SharedDatabase**: All tenants share the same database with a tenant discriminator column
2. **SharedDatabaseSeparateSchema**: Each tenant has its own schema within the same database
3. **SeparateDatabase**: Each tenant has its own database instance

### Using Tenant Services

Inject the `ITenantService` and `ITenantContextAccessor` into your controllers or services:

```csharp
private readonly ITenantService _tenantService;
private readonly ITenantContextAccessor _tenantContextAccessor;

public YourController(ITenantService tenantService, ITenantContextAccessor tenantContextAccessor)
{
    _tenantService = tenantService;
    _tenantContextAccessor = tenantContextAccessor;
}

public async Task<IActionResult> DoSomething()
{
    // Get current tenant ID
    var tenantId = _tenantContextAccessor.GetCurrentTenantId();
    
    // Get current tenant
    var tenant = await _tenantContextAccessor.GetCurrentTenantAsync();
    
    // Create a new tenant
    var newTenant = await _tenantService.CreateTenantAsync(
        "Tenant 1", 
        "tenant1", 
        "First tenant", 
        TenantStorageMode.SharedDatabase, 
        null, 
        "system");
        
    // Add a user to a tenant
    await _tenantService.AddUserToTenantAsync(tenantId, userId, isAdmin: false, "system");
    
    // Add tenant-specific setting
    await _tenantService.AddTenantSettingAsync(tenantId, "theme", "dark", "system");
    
    // Get all tenant users
    var tenantUsers = await _tenantService.GetTenantUsersAsync(tenantId);
    
    // The rest of your logic...
}
```

### Tenant-Specific Data

All permission entities (Role, Permission, etc.) automatically include the TenantId and queries are automatically filtered by the current tenant.

## Permission Usage

### Using the Permission Service

Inject the `IPermissionService` into your controllers or services:

```csharp
private readonly IPermissionService _permissionService;

public YourController(IPermissionService permissionService)
{
    _permissionService = permissionService;
}

public async Task<IActionResult> DoSomething(Guid userId)
{
    // Check if user has a specific permission
    bool canDoThis = await _permissionService.HasPermissionAsync(userId, "Products.Create");
    
    // Check if user has any of these permissions
    bool canDoAny = await _permissionService.HasAnyPermissionAsync(userId, 
        new[] { "Products.Create", "Products.Edit" });
        
    // Check if user has all of these permissions
    bool canDoAll = await _permissionService.HasAllPermissionsAsync(userId,
        new[] { "Products.View", "Products.Edit" });
        
    // Get all permissions for a user
    var permissions = await _permissionService.GetUserPermissionsAsync(userId);
    
    // Get all roles for a user
    var roles = await _permissionService.GetUserRolesAsync(userId);
    
    // Create a new role
    var role = await _permissionService.CreateRoleAsync("Admin", "Administrator role", true, "system");
    
    // Create a new permission
    var permission = await _permissionService.CreatePermissionAsync(
        "Create Products", 
        "Products.Create", 
        "Permission to create new products", 
        "Products", 
        "system");
    
    // Assign a permission to a role
    await _permissionService.AssignPermissionToRoleAsync(role.Id, permission.Id, "system");
    
    // Assign a role to a user
    await _permissionService.AssignRoleToUserAsync(userId, role.Id, "system");
}
```

### Using Attribute-Based Authorization

Add the `[RequirePermission]` attribute to controllers or actions:

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    // Require permission for all actions in this controller
    [RequirePermission("Products.View")]
    [HttpGet]
    public IActionResult GetProducts()
    {
        return Ok("Products list");
    }
    
    // Require specific permission for this action
    [RequirePermission("Products.Create")]
    [HttpPost]
    public IActionResult CreateProduct()
    {
        return Ok("Product created");
    }
    
    // Require multiple permissions
    [RequirePermission("Products.Edit")]
    [RequirePermission("Products.View")]
    [HttpPut("{id}")]
    public IActionResult UpdateProduct(int id)
    {
        return Ok($"Product {id} updated");
    }
}
```

## Best Practices

- Use consistent naming conventions for permissions, such as `Area.Action` (e.g., `Products.Create`, `Users.View`)
- Organize permissions into logical groups
- Create system roles for common permission sets
- Always check permissions in business logic, not just at the controller level
- Use the `IPermissionService` for programmatic permission checks
- Create a standard set of permissions per tenant during tenant creation
- Use tenant settings for tenant-specific configuration 