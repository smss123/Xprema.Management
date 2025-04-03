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
        
        // Output all claims for debugging
        foreach (var claim in claims)
        {
            Console.WriteLine($"Claim: {claim.Key} = {claim.Value}");
        }
        
        // Use the exact claim types from JWT
        Assert.True(claims.ContainsKey("nameid") || claims.ContainsKey("sub"), "JWT should contain a subject identifier claim");
        Assert.True(claims.ContainsKey("unique_name") || claims.ContainsKey("name"), "JWT should contain a name claim");
        Assert.True(claims.ContainsKey("email"), "JWT should contain an email claim");
        
        string nameIdValue = claims.ContainsKey("nameid") ? claims["nameid"] : claims["sub"];
        string nameValue = claims.ContainsKey("unique_name") ? claims["unique_name"] : claims["name"];
        
        Assert.Equal(user.Id.ToString(), nameIdValue);
        Assert.Equal(user.Username, nameValue);
        Assert.Equal(user.Email, claims["email"]);
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