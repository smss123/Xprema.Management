using Microsoft.EntityFrameworkCore;

namespace Xprema.EntityFrameworkCore.Migrations.DbContexts;

/// <summary>
/// Base interface for all Entity Framework Core DbContext interfaces.
/// Following ABP.io's modular approach for database contexts.
/// </summary>
public interface IEfCoreDbContext
{
    /// <summary>
    /// Saves changes to database.
    /// </summary>
    /// <returns>Number of affected entities</returns>
    int SaveChanges();
    
    /// <summary>
    /// Saves changes to database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of affected entities</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets DbSet for given entity.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <returns>DbSet for the entity type</returns>
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    
    /// <summary>
    /// Gets a database entity by primary key.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="keyValues">Primary key values</param>
    /// <returns>Found entity or null</returns>
    TEntity? Find<TEntity>(params object[] keyValues) where TEntity : class;
    
    /// <summary>
    /// Gets a database entity by primary key asynchronously.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="keyValues">Primary key values</param>
    /// <returns>Found entity or null</returns>
    ValueTask<TEntity?> FindAsync<TEntity>(params object[] keyValues) where TEntity : class;
} 