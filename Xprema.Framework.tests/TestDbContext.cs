using Microsoft.EntityFrameworkCore;
using Xprema.Framework.Entities.HistoryFeature;
using Xprema.Framework.Entities.Identity;
using Xprema.Framework.Entities.MultiTenancy;
using Xprema.Framework.Entities.Permission;

namespace Xprema.Framework.Tests;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }
    
    // Permission entities
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    
    // Tenant entities
    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<TenantUser> TenantUsers { get; set; } = null!;
    
    // Identity entities
    public DbSet<ApplicationUser> Users { get; set; } = null!;
    public DbSet<UserToken> UserTokens { get; set; } = null!;
    public DbSet<UserRoleIdentity> UserRoleIdentities { get; set; } = null!;
    
    // Audit entities
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    
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
            
        // Configure dictionary storage
        modelBuilder.Entity<Tenant>()
            .Property(t => t.Settings)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null!),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions)null) ?? new Dictionary<string, string>());
                
        // Configure Identity entities
        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => u.Username)
            .IsUnique();
            
        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => u.Email)
            .IsUnique();
            
        modelBuilder.Entity<UserToken>()
            .HasOne(t => t.User)
            .WithMany(u => u.UserTokens)
            .HasForeignKey(t => t.UserId);
            
        modelBuilder.Entity<UserToken>()
            .HasOne(t => t.Tenant)
            .WithMany()
            .HasForeignKey(t => t.TenantId);
            
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