using System.ComponentModel.DataAnnotations;

namespace Xprema.Framework.Entities.Identity;

public class RegisterUserRequest
{
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = null!;
    
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = null!;
    
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = null!;
    
    [MaxLength(256)]
    public string? DisplayName { get; set; }
    
    [MaxLength(100)]
    public string? PhoneNumber { get; set; }
    
    public Guid? TenantId { get; set; }
}

public class LoginRequest
{
    [Required]
    [MaxLength(100)]
    public string UsernameOrEmail { get; set; } = null!;
    
    [Required]
    public string Password { get; set; } = null!;
    
    public bool RememberMe { get; set; }
    
    public Guid? TenantId { get; set; }
    
    public string? TwoFactorCode { get; set; }
    
    public string? TwoFactorRecoveryCode { get; set; }
}

public class AuthResult
{
    public bool Succeeded { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? UserId { get; set; }
    public string? Username { get; set; }
    public bool RequiresTwoFactor { get; set; }
    public bool IsLockedOut { get; set; }
    public bool RequiresEmailConfirmation { get; set; }
    public string? ErrorMessage { get; set; }
}

public class TwoFactorSetupResult
{
    public bool Succeeded { get; set; }
    public string? SharedKey { get; set; }
    public string? QrCodeUri { get; set; }
    public List<string>? RecoveryCodes { get; set; }
    public string? ErrorMessage { get; set; }
}

public class TenantCreationRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;
    
    [Required]
    [MaxLength(100)]
    public string Code { get; set; } = null!;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public Guid? AdminUserId { get; set; }
    
    public string? StorageMode { get; set; }
    
    public string? ConnectionString { get; set; }
}

public class TenantCreationResult
{
    public bool Succeeded { get; set; }
    public Guid? TenantId { get; set; }
    public string? ErrorMessage { get; set; }
} 