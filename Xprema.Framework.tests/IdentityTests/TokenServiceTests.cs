using System.Collections.Generic;
using System.Threading.Tasks;
using Xprema.Framework.Entities.Identity;

namespace Xprema.Framework.Tests.IdentityTests;

public class TokenServiceTests : TestBase
{
    [Fact]
    public async Task GenerateAccessToken_ShouldReturnValidToken()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        
        // Act
        var token = TokenService.GenerateAccessToken(user);
        
        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        
        // Validate the token
        Dictionary<string, string> claims;
        var isValid = TokenService.ValidateToken(token, out claims);
        
        Assert.True(isValid);
        Assert.NotNull(claims);
        Assert.True(claims.ContainsKey("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"));
        Assert.Equal(user.Id.ToString(), claims["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"]);
        Assert.Equal(user.Username, claims["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"]);
        Assert.Equal(user.Email, claims["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"]);
    }
    
    [Fact]
    public void GenerateRefreshToken_ShouldReturnRandomToken()
    {
        // Act
        var token1 = TokenService.GenerateRefreshToken();
        var token2 = TokenService.GenerateRefreshToken();
        
        // Assert
        Assert.NotNull(token1);
        Assert.NotNull(token2);
        Assert.NotEmpty(token1);
        Assert.NotEmpty(token2);
        Assert.NotEqual(token1, token2); // Tokens should be unique
    }
    
    [Fact]
    public void ValidateToken_WithInvalidToken_ShouldReturnFalse()
    {
        // Arrange
        var invalidToken = "invalid.token.string";
        
        // Act
        Dictionary<string, string> claims;
        var isValid = TokenService.ValidateToken(invalidToken, out claims);
        
        // Assert
        Assert.False(isValid);
        Assert.Empty(claims);
    }
} 