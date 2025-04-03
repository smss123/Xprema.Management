using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Xprema.Framework.Entities.Identity;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Xprema authentication services
    /// </summary>
    public static IServiceCollection AddXpremaAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // Register services
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<ITokenService, TokenService>();
        
        // Configure JWT authentication
        var key = Encoding.ASCII.GetBytes(configuration["Jwt:Secret"] ?? "XpremaDefaultSecretKey12345678901234567890");
        
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = configuration["Jwt:Issuer"] ?? "XpremaFramework",
                ValidateAudience = true,
                ValidAudience = configuration["Jwt:Audience"] ?? "XpremaUsers",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });
        
        return services;
    }
    
    /// <summary>
    /// Adds the Xprema authentication controller to handle authentication endpoints
    /// </summary>
    public static IMvcBuilder AddXpremaAuthenticationController(this IMvcBuilder mvcBuilder)
    {
        // Register authentication controller
        return mvcBuilder;
    }
    
    /// <summary>
    /// Adds database context configuration for identity entities
    /// </summary>
    public static void AddXpremaIdentityEntities<TContext>(this Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder) 
        where TContext : Microsoft.EntityFrameworkCore.DbContext
    {
        // Configure ApplicationUser
        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => u.Username)
            .IsUnique();
            
        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => u.Email)
            .IsUnique();
            
        // Configure UserToken
        modelBuilder.Entity<UserToken>()
            .HasOne(t => t.User)
            .WithMany(u => u.UserTokens)
            .HasForeignKey(t => t.UserId);
            
        modelBuilder.Entity<UserToken>()
            .HasOne(t => t.Tenant)
            .WithMany()
            .HasForeignKey(t => t.TenantId);
            
        // Configure UserRoleIdentity (if used)
        modelBuilder.Entity<UserRoleIdentity>()
            .HasOne(ur => ur.User)
            .WithMany()
            .HasForeignKey(ur => ur.UserId);
            
        modelBuilder.Entity<UserRoleIdentity>()
            .HasOne(ur => ur.Role)
            .WithMany()
            .HasForeignKey(ur => ur.RoleId);
            
        modelBuilder.Entity<UserRoleIdentity>()
            .HasOne(ur => ur.Tenant)
            .WithMany()
            .HasForeignKey(ur => ur.TenantId);
    }
} 