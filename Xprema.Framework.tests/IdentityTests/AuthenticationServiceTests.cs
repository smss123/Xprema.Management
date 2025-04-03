using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Xprema.Framework.Entities.Identity;
using Xprema.Framework.Entities.MultiTenancy;

namespace Xprema.Framework.Tests.IdentityTests;

public class AuthenticationServiceTests : TestBase
{
    [Fact]
    public async Task RegisterUser_ShouldCreateNewUser()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            Username = "newuser",
            Email = "newuser@example.com",
            Password = "Password123!",
            DisplayName = "New User"
        };
        
        // Act
        var result = await AuthenticationService.RegisterUserAsync(request);
        
        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.UserId);
        Assert.Equal("newuser", result.Username);
        Assert.True(result.RequiresEmailConfirmation);
        
        var user = await DbContext.Users.FirstOrDefaultAsync(u => u.Username == "newuser");
        Assert.NotNull(user);
        Assert.Equal("newuser@example.com", user.Email);
        Assert.Equal("New User", user.DisplayName);
        Assert.NotNull(user.PasswordHash);
        Assert.False(user.EmailConfirmed);
    }
    
    [Fact]
    public async Task RegisterUser_WithExistingUsername_ShouldFail()
    {
        // Arrange
        await CreateTestUserAsync("existinguser", "existing@example.com");
        
        var request = new RegisterUserRequest
        {
            Username = "existinguser",
            Email = "anotheremail@example.com",
            Password = "Password123!"
        };
        
        // Act
        var result = await AuthenticationService.RegisterUserAsync(request);
        
        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("already exists", result.ErrorMessage);
    }
    
    [Fact]
    public async Task RegisterUser_WithTenant_ShouldAssociateWithTenant()
    {
        // Arrange
        var tenant = await CreateTestTenantAsync();
        TenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        var request = new RegisterUserRequest
        {
            Username = "tenantuser",
            Email = "tenant@example.com",
            Password = "Password123!",
            TenantId = tenant.Id
        };
        
        // Act
        var result = await AuthenticationService.RegisterUserAsync(request);
        
        // Assert
        Assert.True(result.Succeeded);
        
        if (!Guid.TryParse(result.UserId, out var userId))
        {
            Assert.Fail("Invalid user ID returned");
        }
        
        var tenantUser = await DbContext.TenantUsers.FirstOrDefaultAsync(tu => 
            tu.TenantId == tenant.Id && tu.UserId == userId);
            
        Assert.NotNull(tenantUser);
    }
    
    [Fact]
    public async Task Authenticate_WithValidCredentials_ShouldSucceed()
    {
        // Arrange
        var user = await CreateTestUserAsync("loginuser", "login@example.com", "Password123!");
        
        var request = new LoginRequest
        {
            UsernameOrEmail = "loginuser",
            Password = "Password123!"
        };
        
        // Act
        var result = await AuthenticationService.AuthenticateAsync(request);
        
        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(user.Id.ToString(), result.UserId);
        Assert.Equal(user.Username, result.Username);
        Assert.NotNull(result.AccessToken);
        Assert.Null(result.RefreshToken); // Not using remember me
    }
    
    [Fact]
    public async Task Authenticate_WithInvalidPassword_ShouldFail()
    {
        // Arrange
        await CreateTestUserAsync("wrongpassuser", "wrongpass@example.com", "Password123!");
        
        var request = new LoginRequest
        {
            UsernameOrEmail = "wrongpassuser",
            Password = "WrongPassword123!"
        };
        
        // Act
        var result = await AuthenticationService.AuthenticateAsync(request);
        
        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Invalid username or password", result.ErrorMessage);
    }
    
    [Fact]
    public async Task Authenticate_WithRememberMe_ShouldProvideRefreshToken()
    {
        // Arrange
        var tenant = await CreateTestTenantAsync();
        TenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        var user = await CreateTestUserAsync("rememberuser", "remember@example.com", "Password123!", tenant.Id);
        
        var request = new LoginRequest
        {
            UsernameOrEmail = "rememberuser",
            Password = "Password123!",
            RememberMe = true,
            TenantId = tenant.Id
        };
        
        // Act
        var result = await AuthenticationService.AuthenticateAsync(request);
        
        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);
        
        // Check if a refresh token was saved
        var refreshToken = await DbContext.UserTokens.FirstOrDefaultAsync(t => 
            t.UserId == user.Id && t.TokenType == "RefreshToken");
            
        Assert.NotNull(refreshToken);
        Assert.Equal(result.RefreshToken, refreshToken.TokenValue);
    }
    
    [Fact]
    public async Task ChangePassword_WithValidCurrentPassword_ShouldSucceed()
    {
        // Arrange
        var user = await CreateTestUserAsync("changepassuser", "changepass@example.com", "OldPassword123!");
        
        // Act
        var result = await AuthenticationService.ChangePasswordAsync(
            user.Id, 
            "OldPassword123!", 
            "NewPassword123!");
            
        // Assert
        Assert.True(result);
        
        // Try logging in with the new password
        var loginRequest = new LoginRequest
        {
            UsernameOrEmail = "changepassuser",
            Password = "NewPassword123!"
        };
        
        var loginResult = await AuthenticationService.AuthenticateAsync(loginRequest);
        Assert.True(loginResult.Succeeded);
    }
    
    [Fact]
    public async Task ForgotPassword_ShouldCreateResetToken()
    {
        // Arrange
        var user = await CreateTestUserAsync("forgotpassuser", "forgotpass@example.com");
        
        // Act
        var result = await AuthenticationService.ForgotPasswordAsync("forgotpass@example.com");
        
        // Assert
        Assert.True(result);
        
        // Check if a reset token was created
        var resetToken = await DbContext.UserTokens.FirstOrDefaultAsync(t => 
            t.UserId == user.Id && t.TokenType == "PasswordReset");
            
        Assert.NotNull(resetToken);
        Assert.NotNull(resetToken.ExpirationDate);
        Assert.False(resetToken.IsUsed);
    }
    
    [Fact]
    public async Task ResetPassword_WithValidToken_ShouldSucceed()
    {
        // Arrange
        var user = await CreateTestUserAsync("resetpassuser", "resetpass@example.com", "OldPassword123!");
        
        // Create a reset token manually
        var resetToken = new UserToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenType = "PasswordReset",
            TokenValue = "ValidResetToken",
            ExpirationDate = DateTime.UtcNow.AddHours(24),
            TenantId = TenantContextAccessor.GetCurrentTenantId()
        };
        
        DbContext.UserTokens.Add(resetToken);
        await DbContext.SaveChangesAsync();
        
        // Act
        var result = await AuthenticationService.ResetPasswordAsync(
            user.Id.ToString(), 
            "ValidResetToken", 
            "NewPassword123!");
            
        // Assert
        Assert.True(result);
        
        // Verify token is marked as used
        await DbContext.Entry(resetToken).ReloadAsync();
        Assert.True(resetToken.IsUsed);
        
        // Try logging in with the new password
        var loginRequest = new LoginRequest
        {
            UsernameOrEmail = "resetpassuser",
            Password = "NewPassword123!"
        };
        
        var loginResult = await AuthenticationService.AuthenticateAsync(loginRequest);
        Assert.True(loginResult.Succeeded);
    }
    
    [Fact]
    public async Task EnableTwoFactor_ShouldSetupTwoFactor()
    {
        // Arrange
        var user = await CreateTestUserAsync("2fauser", "2fa@example.com");
        
        // Act
        var result = await AuthenticationService.EnableTwoFactorAsync(user.Id);
        
        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.SharedKey);
        Assert.NotNull(result.QrCodeUri);
        Assert.NotNull(result.RecoveryCodes);
        Assert.Equal(8, result.RecoveryCodes.Count);
        
        // Check if recovery codes were saved
        var recoveryCodes = await DbContext.UserTokens
            .Where(t => t.UserId == user.Id && t.TokenType == "2FARecovery")
            .ToListAsync();
            
        Assert.Equal(8, recoveryCodes.Count);
        
        // Check if the key was saved on the user
        await DbContext.Entry(user).ReloadAsync();
        Assert.NotNull(user.TwoFactorKey);
    }
    
    [Fact]
    public async Task DisableTwoFactor_ShouldDisableTwoFactor()
    {
        // Arrange
        var user = await CreateTestUserAsync("disable2fa", "disable2fa@example.com");
        
        // Enable 2FA first
        await AuthenticationService.EnableTwoFactorAsync(user.Id);
        
        // Set TwoFactorEnabled to true (normally done after verification)
        user.TwoFactorEnabled = true;
        await DbContext.SaveChangesAsync();
        
        // Act
        var result = await AuthenticationService.DisableTwoFactorAsync(user.Id);
        
        // Assert
        Assert.True(result);
        
        // Check if 2FA is disabled
        await DbContext.Entry(user).ReloadAsync();
        Assert.False(user.TwoFactorEnabled);
        Assert.Null(user.TwoFactorKey);
        
        // Check if recovery codes were removed
        var recoveryCodes = await DbContext.UserTokens
            .Where(t => t.UserId == user.Id && t.TokenType == "2FARecovery")
            .ToListAsync();
            
        Assert.Empty(recoveryCodes);
    }
    
    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldProvideNewTokens()
    {
        // Arrange
        var user = await CreateTestUserAsync("refreshuser", "refresh@example.com");
        
        // Create a refresh token manually
        var refreshToken = new UserToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenType = "RefreshToken",
            TokenValue = "ValidRefreshToken",
            ExpirationDate = DateTime.UtcNow.AddDays(30),
            TenantId = TenantContextAccessor.GetCurrentTenantId()
        };
        
        DbContext.UserTokens.Add(refreshToken);
        await DbContext.SaveChangesAsync();
        
        // Act
        var result = await AuthenticationService.RefreshTokenAsync("ValidRefreshToken");
        
        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);
        Assert.NotEqual("ValidRefreshToken", result.RefreshToken);
        
        // Check if token was updated
        await DbContext.Entry(refreshToken).ReloadAsync();
        Assert.Equal(result.RefreshToken, refreshToken.TokenValue);
    }
    
    [Fact]
    public async Task CreateTenant_ShouldCreateNewTenant()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        
        var request = new TenantCreationRequest
        {
            Name = "New Test Tenant",
            Code = "newtest",
            Description = "New tenant for testing",
            AdminUserId = user.Id
        };
        
        // Act
        var result = await AuthenticationService.CreateTenantAsync(request);
        
        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.TenantId);
        
        // Check if tenant was created
        var tenant = await DbContext.Tenants.FindAsync(result.TenantId);
        Assert.NotNull(tenant);
        Assert.Equal("New Test Tenant", tenant.Name);
        Assert.Equal("newtest", tenant.Identifier);
        
        // Check if user was added as admin
        var tenantUser = await DbContext.TenantUsers.FirstOrDefaultAsync(tu => 
            tu.TenantId == tenant.Id && tu.UserId == user.Id);
            
        Assert.NotNull(tenantUser);
        Assert.True(tenantUser.IsAdmin);
    }
} 