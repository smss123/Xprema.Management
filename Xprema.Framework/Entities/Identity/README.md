# Xprema.Framework Authentication and Identity

This module provides a comprehensive authentication and identity system for your ASP.NET Core applications with multi-tenancy and auditing support.

## Features

- **User Management**: Complete user management with email confirmation, forgot password, etc.
- **Sign-in with Remember Me**: Allow users to remain signed in across browser sessions
- **Two-Factor Authentication (2FA)**: Enhance security with TOTP-based 2FA
- **JWT Authentication**: Secure API access with JWT tokens
- **Multi-tenancy Integration**: Built-in integration with the framework's multi-tenancy features
- **Audit Logging**: Track user activities across the application

## Setup

### 1. Register Required Services

Add the authentication services to your application in `Program.cs`:

```csharp
// Add authentication services
builder.Services.AddXpremaAuthentication(builder.Configuration);

// Add permission system (if not already added)
builder.Services.AddXpremaPermissions<YourDbContext>();

// Add audit logging
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
```

### 2. Configure DbContext

Configure your DbContext to include identity entities:

```csharp
public class YourDbContext : DbContext
{
    // Other DbSets...
    
    // Identity entities
    public DbSet<ApplicationUser> Users { get; set; }
    public DbSet<UserToken> UserTokens { get; set; }
    
    // Audit entities
    public DbSet<AuditLog> AuditLogs { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure identity entities
        modelBuilder.AddXpremaIdentityEntities<YourDbContext>();
        
        // Configure tenant entities (if not already configured)
        modelBuilder.Entity<Tenant>()
            .HasMany(t => t.TenantUsers)
            .WithOne(tu => tu.Tenant)
            .HasForeignKey(tu => tu.TenantId);
    }
}
```

### 3. Configure Authentication Middleware

Add authentication middleware to your application in `Program.cs`:

```csharp
app.UseAuthentication();
app.UseAuthorization();

// Add tenant resolution middleware
app.UseTenantResolution(TenantResolutionStrategy.Header);
```

### 4. Add JWT Settings in appsettings.json

```json
{
  "Jwt": {
    "Secret": "your-very-long-secret-key-at-least-32-characters",
    "Issuer": "YourApplication",
    "Audience": "YourApplicationUsers",
    "ExpiryInHours": 1
  }
}
```

## Usage

### Authentication

Inject `IAuthenticationService` into your controllers or services:

```csharp
private readonly IAuthenticationService _authService;

public AuthController(IAuthenticationService authService)
{
    _authService = authService;
}

[HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
{
    var result = await _authService.RegisterUserAsync(request);
    
    if (result.Succeeded)
    {
        return Ok(result);
    }
    
    return BadRequest(result.ErrorMessage);
}

[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    var result = await _authService.AuthenticateAsync(request);
    
    if (result.Succeeded)
    {
        return Ok(result);
    }
    
    if (result.RequiresTwoFactor)
    {
        return Ok(new { requiresTwoFactor = true, userId = result.UserId });
    }
    
    return BadRequest(result.ErrorMessage);
}

[HttpPost("two-factor")]
public async Task<IActionResult> TwoFactorLogin([FromBody] TwoFactorLoginRequest request)
{
    var loginRequest = new LoginRequest
    {
        UsernameOrEmail = request.UsernameOrEmail,
        Password = request.Password,
        TwoFactorCode = request.TwoFactorCode,
        RememberMe = request.RememberMe
    };
    
    var result = await _authService.AuthenticateAsync(loginRequest);
    
    if (result.Succeeded)
    {
        return Ok(result);
    }
    
    return BadRequest(result.ErrorMessage);
}
```

### Using Two-Factor Authentication

```csharp
[HttpPost("enable-two-factor")]
[Authorize]
public async Task<IActionResult> EnableTwoFactor()
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    
    if (!Guid.TryParse(userId, out var userGuid))
    {
        return BadRequest("Invalid user ID");
    }
    
    var result = await _authService.EnableTwoFactorAsync(userGuid);
    
    if (result.Succeeded)
    {
        return Ok(result);
    }
    
    return BadRequest(result.ErrorMessage);
}

[HttpPost("verify-two-factor")]
[Authorize]
public async Task<IActionResult> VerifyTwoFactor([FromBody] VerifyTwoFactorRequest request)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    
    if (!Guid.TryParse(userId, out var userGuid))
    {
        return BadRequest("Invalid user ID");
    }
    
    var isValid = await _authService.ValidateTwoFactorCodeAsync(userGuid, request.Code);
    
    if (isValid)
    {
        // Enable 2FA for the user
        var user = await _dbContext.Users.FindAsync(userGuid);
        user.TwoFactorEnabled = true;
        await _dbContext.SaveChangesAsync();
        
        return Ok(new { success = true });
    }
    
    return BadRequest("Invalid verification code");
}
```

### Creating a New Tenant

```csharp
[HttpPost("tenants")]
[Authorize]
public async Task<IActionResult> CreateTenant([FromBody] TenantCreationRequest request)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    
    if (!Guid.TryParse(userId, out var userGuid))
    {
        return BadRequest("Invalid user ID");
    }
    
    // Set the current user as admin if not specified
    if (!request.AdminUserId.HasValue)
    {
        request.AdminUserId = userGuid;
    }
    
    var result = await _authService.CreateTenantAsync(request);
    
    if (result.Succeeded)
    {
        return Ok(result);
    }
    
    return BadRequest(result.ErrorMessage);
}
```

### Audit Logging

Inject `IAuditLogService` into your controllers or services:

```csharp
private readonly IAuditLogService _auditLogService;

public YourController(IAuditLogService auditLogService)
{
    _auditLogService = auditLogService;
}

public async Task<IActionResult> DoSomething()
{
    // Your business logic...
    
    // Log the activity
    await _auditLogService.LogActivityAsync(
        User.FindFirstValue(ClaimTypes.NameIdentifier),
        "Created new product",
        "Product",
        product.Id.ToString(),
        null,
        JsonSerializer.Serialize(product)
    );
    
    return Ok();
}

[HttpGet("audit-logs")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogFilter filter)
{
    var logs = await _auditLogService.GetAuditLogsAsync(
        filter.UserId,
        filter.EntityType,
        filter.EntityId,
        filter.StartDate,
        filter.EndDate,
        filter.Skip,
        filter.Take
    );
    
    return Ok(logs);
}
```

## Security Considerations

1. **Password Storage**: The framework uses a secure hashing mechanism, but you can replace it with a more sophisticated one if needed.
2. **Token Protection**: JWT tokens are signed but not encrypted. Ensure you're using HTTPS in production.
3. **Remember Me**: The "Remember Me" feature extends session lifetime. Advise users to use it only on trusted devices.
4. **Two-Factor Authentication**: Implement a proper TOTP algorithm for 2FA in production (the current implementation is a placeholder).
5. **Email Confirmation**: Implement email sending to complete the email confirmation flow. 