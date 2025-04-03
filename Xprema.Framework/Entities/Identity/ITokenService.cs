namespace Xprema.Framework.Entities.Identity;

/// <summary>
/// Service for generating JWT tokens for authentication
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT access token for a user
    /// </summary>
    string GenerateAccessToken(ApplicationUser user);
    
    /// <summary>
    /// Generates a refresh token
    /// </summary>
    string GenerateRefreshToken();
    
    /// <summary>
    /// Validates a JWT token
    /// </summary>
    bool ValidateToken(string token, out Dictionary<string, string> claims);
} 