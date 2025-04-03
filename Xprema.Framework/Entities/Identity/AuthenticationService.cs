using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xprema.Framework.Entities.MultiTenancy;

namespace Xprema.Framework.Entities.Identity;

public class AuthenticationService : IAuthenticationService
{
    private readonly DbContext _dbContext;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly ITenantService _tenantService;
    private readonly ITenantContextAccessor _tenantContextAccessor;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITokenService _tokenService;
    
    public AuthenticationService(
        DbContext dbContext, 
        ILogger<AuthenticationService> logger,
        ITenantService tenantService,
        ITenantContextAccessor tenantContextAccessor,
        IHttpContextAccessor httpContextAccessor,
        ITokenService tokenService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _tenantService = tenantService;
        _tenantContextAccessor = tenantContextAccessor;
        _httpContextAccessor = httpContextAccessor;
        _tokenService = tokenService;
    }
    
    public async Task<AuthResult> RegisterUserAsync(RegisterUserRequest request)
    {
        try
        {
            // Check if username or email already exists
            var existingUser = await _dbContext.Set<ApplicationUser>()
                .FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Email);
                
            if (existingUser != null)
            {
                return new AuthResult 
                { 
                    Succeeded = false,
                    ErrorMessage = $"User with username '{request.Username}' or email '{request.Email}' already exists."
                };
            }
            
            // Create the user
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                DisplayName = request.DisplayName ?? request.Username,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = HashPassword(request.Password),
                SecurityStamp = Guid.NewGuid().ToString("N"),
                EmailConfirmed = false,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow
            };
            
            _dbContext.Add(user);
            await _dbContext.SaveChangesAsync();
            
            // Associate with tenant if provided
            if (request.TenantId.HasValue)
            {
                await _tenantService.AddUserToTenantAsync(
                    request.TenantId.Value, 
                    user.Id, 
                    isAdmin: false, 
                    "system");
            }
            
            // TODO: Send confirmation email
            
            return new AuthResult
            {
                Succeeded = true,
                UserId = user.Id.ToString(),
                Username = user.Username,
                RequiresEmailConfirmation = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user: {Message}", ex.Message);
            return new AuthResult { Succeeded = false, ErrorMessage = "An error occurred during registration." };
        }
    }
    
    public async Task<AuthResult> AuthenticateAsync(LoginRequest request)
    {
        try
        {
            // Find user by username or email
            var user = await _dbContext.Set<ApplicationUser>()
                .FirstOrDefaultAsync(u => 
                    u.Username == request.UsernameOrEmail || 
                    u.Email == request.UsernameOrEmail);
                
            if (user == null)
            {
                return new AuthResult { Succeeded = false, ErrorMessage = "Invalid username or password." };
            }
            
            // Check if user is locked out
            if (user.IsLockedOut && user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            {
                return new AuthResult { 
                    Succeeded = false, 
                    IsLockedOut = true,
                    ErrorMessage = $"Account is locked out until {user.LockoutEnd}."
                };
            }
            
            // Check password
            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                // Increment failed attempts
                user.AccessFailedCount++;
                
                // Lock account after 5 failed attempts
                if (user.AccessFailedCount >= 5)
                {
                    user.IsLockedOut = true;
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                }
                
                await _dbContext.SaveChangesAsync();
                return new AuthResult { Succeeded = false, ErrorMessage = "Invalid username or password." };
            }
            
            // Reset failed attempts on successful login
            user.AccessFailedCount = 0;
            user.LastLoginDate = DateTime.UtcNow;
            
            // Check if 2FA is enabled
            if (user.TwoFactorEnabled)
            {
                // If 2FA code provided, validate it
                if (!string.IsNullOrEmpty(request.TwoFactorCode))
                {
                    var isValid = await ValidateTwoFactorCodeAsync(user.Id, request.TwoFactorCode);
                    if (!isValid)
                    {
                        return new AuthResult { 
                            Succeeded = false, 
                            RequiresTwoFactor = true,
                            UserId = user.Id.ToString(),
                            ErrorMessage = "Invalid two-factor code."
                        };
                    }
                }
                // If recovery code provided, validate it
                else if (!string.IsNullOrEmpty(request.TwoFactorRecoveryCode))
                {
                    var isValidRecovery = await ValidateRecoveryCodeAsync(user.Id, request.TwoFactorRecoveryCode);
                    if (!isValidRecovery)
                    {
                        return new AuthResult { 
                            Succeeded = false, 
                            RequiresTwoFactor = true,
                            UserId = user.Id.ToString(),
                            ErrorMessage = "Invalid recovery code."
                        };
                    }
                }
                // If no code provided, return requiring 2FA
                else
                {
                    return new AuthResult { 
                        Succeeded = false, 
                        RequiresTwoFactor = true,
                        UserId = user.Id.ToString()
                    };
                }
            }
            
            // Check if email confirmation is required and not confirmed
            if (!user.EmailConfirmed)
            {
                return new AuthResult { 
                    Succeeded = false, 
                    RequiresEmailConfirmation = true,
                    UserId = user.Id.ToString(),
                    ErrorMessage = "Email not confirmed. Please check your email for confirmation instructions."
                };
            }
            
            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddHours(1);
            
            // Save refresh token if remember me is enabled
            if (request.RememberMe)
            {
                var userToken = new UserToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    TokenType = "RefreshToken",
                    TokenValue = refreshToken,
                    ExpirationDate = DateTime.UtcNow.AddDays(30),
                    TenantId = request.TenantId ?? _tenantContextAccessor.GetCurrentTenantId(),
                    CreatedBy = "system",
                    CreatedDate = DateTime.UtcNow
                };
                
                _dbContext.Add(userToken);
            }
            
            await _dbContext.SaveChangesAsync();
            
            return new AuthResult
            {
                Succeeded = true,
                AccessToken = accessToken,
                RefreshToken = request.RememberMe ? refreshToken : null,
                ExpiresAt = expiresAt,
                UserId = user.Id.ToString(),
                Username = user.Username
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating user: {Message}", ex.Message);
            return new AuthResult { Succeeded = false, ErrorMessage = "An error occurred during authentication." };
        }
    }
    
    public async Task LogoutAsync(Guid userId)
    {
        // Remove all refresh tokens for the user
        var refreshTokens = await _dbContext.Set<UserToken>()
            .Where(t => t.UserId == userId && t.TokenType == "RefreshToken")
            .ToListAsync();
            
        _dbContext.RemoveRange(refreshTokens);
        await _dbContext.SaveChangesAsync();
    }
    
    public async Task<bool> ConfirmEmailAsync(string userId, string token)
    {
        if (!Guid.TryParse(userId, out var userGuid))
        {
            return false;
        }
        
        var user = await _dbContext.Set<ApplicationUser>().FindAsync(userGuid);
        if (user == null)
        {
            return false;
        }
        
        // Validate token
        var confirmationToken = await _dbContext.Set<UserToken>()
            .FirstOrDefaultAsync(t => 
                t.UserId == userGuid && 
                t.TokenType == "EmailConfirmation" && 
                t.TokenValue == token &&
                !t.IsUsed &&
                t.ExpirationDate > DateTime.UtcNow);
                
        if (confirmationToken == null)
        {
            return false;
        }
        
        user.EmailConfirmed = true;
        confirmationToken.IsUsed = true;
        
        await _dbContext.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> ForgotPasswordAsync(string email)
    {
        var user = await _dbContext.Set<ApplicationUser>()
            .FirstOrDefaultAsync(u => u.Email == email);
            
        if (user == null)
        {
            // Return true even if user doesn't exist to prevent email enumeration
            return true;
        }
        
        // Generate token
        var token = Guid.NewGuid().ToString("N");
        
        // Save token
        var resetToken = new UserToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenType = "PasswordReset",
            TokenValue = token,
            ExpirationDate = DateTime.UtcNow.AddHours(24),
            TenantId = _tenantContextAccessor.GetCurrentTenantId(),
            CreatedBy = "system",
            CreatedDate = DateTime.UtcNow
        };
        
        _dbContext.Add(resetToken);
        await _dbContext.SaveChangesAsync();
        
        // TODO: Send password reset email
        
        return true;
    }
    
    public async Task<bool> ResetPasswordAsync(string userId, string token, string newPassword)
    {
        if (!Guid.TryParse(userId, out var userGuid))
        {
            return false;
        }
        
        var user = await _dbContext.Set<ApplicationUser>().FindAsync(userGuid);
        if (user == null)
        {
            return false;
        }
        
        // Validate token
        var resetToken = await _dbContext.Set<UserToken>()
            .FirstOrDefaultAsync(t => 
                t.UserId == userGuid && 
                t.TokenType == "PasswordReset" && 
                t.TokenValue == token &&
                !t.IsUsed &&
                t.ExpirationDate > DateTime.UtcNow);
                
        if (resetToken == null)
        {
            return false;
        }
        
        // Update password
        user.PasswordHash = HashPassword(newPassword);
        user.SecurityStamp = Guid.NewGuid().ToString("N");
        resetToken.IsUsed = true;
        
        // Remove all refresh tokens for this user
        var refreshTokens = await _dbContext.Set<UserToken>()
            .Where(t => t.UserId == userGuid && t.TokenType == "RefreshToken")
            .ToListAsync();
        
        _dbContext.RemoveRange(refreshTokens);
        await _dbContext.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _dbContext.Set<ApplicationUser>().FindAsync(userId);
        if (user == null)
        {
            return false;
        }
        
        // Verify current password
        if (!VerifyPassword(currentPassword, user.PasswordHash))
        {
            return false;
        }
        
        // Update password
        user.PasswordHash = HashPassword(newPassword);
        user.SecurityStamp = Guid.NewGuid().ToString("N");
        
        await _dbContext.SaveChangesAsync();
        return true;
    }
    
    public async Task<TwoFactorSetupResult> EnableTwoFactorAsync(Guid userId)
    {
        var user = await _dbContext.Set<ApplicationUser>().FindAsync(userId);
        if (user == null)
        {
            return new TwoFactorSetupResult 
            { 
                Succeeded = false, 
                ErrorMessage = "User not found." 
            };
        }
        
        // Generate a random key
        var key = GenerateRandomKey();
        user.TwoFactorKey = key;
        
        // Generate QR code URI
        var qrCodeUri = $"otpauth://totp/Xprema:{user.Email}?secret={key}&issuer=Xprema";
        
        // Generate recovery codes
        var recoveryCodes = new List<string>();
        for (int i = 0; i < 8; i++)
        {
            recoveryCodes.Add(GenerateRandomKey(8));
        }
        
        // Save recovery codes
        foreach (var code in recoveryCodes)
        {
            var userToken = new UserToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenType = "2FARecovery",
                TokenValue = code,
                TenantId = _tenantContextAccessor.GetCurrentTenantId(),
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow
            };
            
            _dbContext.Add(userToken);
        }
        
        await _dbContext.SaveChangesAsync();
        
        return new TwoFactorSetupResult
        {
            Succeeded = true,
            SharedKey = key,
            QrCodeUri = qrCodeUri,
            RecoveryCodes = recoveryCodes
        };
    }
    
    public async Task<bool> ValidateTwoFactorCodeAsync(Guid userId, string code)
    {
        var user = await _dbContext.Set<ApplicationUser>().FindAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.TwoFactorKey))
        {
            return false;
        }
        
        // TODO: Implement TOTP validation
        // For now, just check if code is "123456" (for demo purposes)
        return code == "123456";
    }
    
    private async Task<bool> ValidateRecoveryCodeAsync(Guid userId, string code)
    {
        var recoveryToken = await _dbContext.Set<UserToken>()
            .FirstOrDefaultAsync(t => 
                t.UserId == userId && 
                t.TokenType == "2FARecovery" && 
                t.TokenValue == code &&
                !t.IsUsed);
                
        if (recoveryToken == null)
        {
            return false;
        }
        
        // Mark recovery code as used
        recoveryToken.IsUsed = true;
        await _dbContext.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<bool> DisableTwoFactorAsync(Guid userId)
    {
        var user = await _dbContext.Set<ApplicationUser>().FindAsync(userId);
        if (user == null)
        {
            return false;
        }
        
        user.TwoFactorEnabled = false;
        user.TwoFactorKey = null;
        
        // Remove all recovery codes
        var recoveryCodes = await _dbContext.Set<UserToken>()
            .Where(t => t.UserId == userId && t.TokenType == "2FARecovery")
            .ToListAsync();
            
        _dbContext.RemoveRange(recoveryCodes);
        await _dbContext.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<IEnumerable<string>> GetTwoFactorRecoveryCodesAsync(Guid userId)
    {
        var recoveryCodes = await _dbContext.Set<UserToken>()
            .Where(t => t.UserId == userId && t.TokenType == "2FARecovery" && !t.IsUsed)
            .Select(t => t.TokenValue)
            .ToListAsync();
            
        return recoveryCodes;
    }
    
    public async Task<TenantCreationResult> CreateTenantAsync(TenantCreationRequest request)
    {
        try
        {
            // Create tenant
            var tenant = await _tenantService.CreateTenantAsync(
                request.Name,
                request.Code,
                request.Description,
                ParseStorageMode(request.StorageMode),
                request.ConnectionString,
                "system");
                
            // If an admin user is specified, make them an admin of this tenant
            if (request.AdminUserId.HasValue)
            {
                await _tenantService.AddUserToTenantAsync(
                    tenant.Id,
                    request.AdminUserId.Value,
                    isAdmin: true,
                    "system");
            }
            
            return new TenantCreationResult
            {
                Succeeded = true,
                TenantId = tenant.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tenant: {Message}", ex.Message);
            return new TenantCreationResult
            {
                Succeeded = false,
                ErrorMessage = $"Error creating tenant: {ex.Message}"
            };
        }
    }
    
    private TenantStorageMode ParseStorageMode(string? storageMode)
    {
        if (string.IsNullOrEmpty(storageMode))
        {
            return TenantStorageMode.SharedDatabase;
        }
        
        return storageMode.ToLowerInvariant() switch
        {
            "shareddatabaseseparateschema" => TenantStorageMode.SharedDatabaseSeparateSchema,
            "separatedatabase" => TenantStorageMode.SeparateDatabase,
            _ => TenantStorageMode.SharedDatabase
        };
    }
    
    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            // Find token
            var token = await _dbContext.Set<UserToken>()
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => 
                    t.TokenType == "RefreshToken" && 
                    t.TokenValue == refreshToken &&
                    !t.IsUsed &&
                    t.ExpirationDate > DateTime.UtcNow);
                    
            if (token == null)
            {
                return new AuthResult 
                { 
                    Succeeded = false, 
                    ErrorMessage = "Invalid or expired refresh token." 
                };
            }
            
            var user = token.User;
            
            // Generate new tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddHours(1);
            
            // Update refresh token
            token.TokenValue = newRefreshToken;
            token.ExpirationDate = DateTime.UtcNow.AddDays(30);
            
            await _dbContext.SaveChangesAsync();
            
            return new AuthResult
            {
                Succeeded = true,
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = expiresAt,
                UserId = user.Id.ToString(),
                Username = user.Username
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token: {Message}", ex.Message);
            return new AuthResult 
            { 
                Succeeded = false, 
                ErrorMessage = "An error occurred while refreshing the token." 
            };
        }
    }
    
    #region Helper Methods
    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var hashBytes = sha256.ComputeHash(passwordBytes);
        return Convert.ToBase64String(hashBytes);
    }
    
    private bool VerifyPassword(string password, string? passwordHash)
    {
        if (string.IsNullOrEmpty(passwordHash))
        {
            return false;
        }
        
        var newHash = HashPassword(password);
        return newHash == passwordHash;
    }
    
    private string GenerateRandomKey(int length = 16)
    {
        var buffer = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(buffer);
        return Convert.ToBase64String(buffer).Replace("+", "").Replace("/", "").Replace("=", "").Substring(0, length);
    }
    #endregion
} 