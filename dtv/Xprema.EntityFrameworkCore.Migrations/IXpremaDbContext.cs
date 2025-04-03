using Microsoft.EntityFrameworkCore;

namespace Xprema.EntityFrameworkCore.Migrations;

/// <summary>
/// Interface for the Xprema DbContext, allowing any application to implement
/// their own DbContext while still using the migration infrastructure
/// </summary>
public interface IXpremaDbContext
{
    /// <summary>
    /// Configure entity relationships and database schema
    /// </summary>
    /// <param name="modelBuilder">ModelBuilder instance</param>
    void ConfigureModel(ModelBuilder modelBuilder);
} 