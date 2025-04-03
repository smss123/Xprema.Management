namespace Xprema.Framework.Entities.Identity;

public interface IAuthenticationService
{
    /// <summary>
    /// Registers a new user 
    /// </summary>
    Task<AuthResult> RegisterUserAsync(RegisterUserRequest request);
    
    /// <summary>
    /// Authenticates a user with username/email and password
    /// </summary>
    Task<AuthResult> AuthenticateAsync(LoginRequest request);
    
    /// <summary>
    /// Logs out the current user
    /// </summary>
    Task LogoutAsync(Guid userId);
    
    /// <summary>
    /// Confirms a user's email
    /// </summary>
    Task<bool> ConfirmEmailAsync(string userId, string token);
    
    /// <summary>
    /// Sends a forgot password email
    /// </summary>
    Task<bool> ForgotPasswordAsync(string email);
    
    /// <summary>
    /// Resets a user's password
    /// </summary>
    Task<bool> ResetPasswordAsync(string userId, string token, string newPassword);
    
    /// <summary>
    /// Change user password
    /// </summary>
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    
    /// <summary>
    /// Enables two-factor authentication for a user
    /// </summary>
    Task<TwoFactorSetupResult> EnableTwoFactorAsync(Guid userId);
    
    /// <summary>
    /// Validates a two-factor code
    /// </summary>
    Task<bool> ValidateTwoFactorCodeAsync(Guid userId, string code);
    
    /// <summary>
    /// Disables two-factor authentication for a user
    /// </summary>
    Task<bool> DisableTwoFactorAsync(Guid userId);
    
    /// <summary>
    /// Gets two-factor recovery codes for a user
    /// </summary>
    Task<IEnumerable<string>> GetTwoFactorRecoveryCodesAsync(Guid userId);
    
    /// <summary>
    /// Creates a new tenant
    /// </summary>
    Task<TenantCreationResult> CreateTenantAsync(TenantCreationRequest request);
    
    /// <summary>
    /// Refreshes an authentication token
    /// </summary>
    Task<AuthResult> RefreshTokenAsync(string refreshToken);
} 