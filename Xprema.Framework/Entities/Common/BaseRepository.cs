using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Xprema.Framework.Entities.HistoryFeature;

namespace Xprema.Framework.Entities.Common;

        public abstract class BaseRepository<TEntity, TKey>(DbContext context)
            where TEntity : BaseEntity<TKey>, IEntityHistory
        {
            public virtual async Task AddAsync(TEntity entity, string changedBy)
            {
                entity.CreatedBy = changedBy;
                entity.CreatedDate = DateTime.UtcNow;
                entity.AddVersionRecord(changedBy, "Created");
                await context.Set<TEntity>().AddAsync(entity);
                await context.SaveChangesAsync();
            }

            public virtual async Task UpdateAsync(TEntity entity, string changedBy)
            {
                // Get original entity from database for comparison
                var originalEntity = await context.Set<TEntity>().AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id!.Equals(entity.Id));
                
                if (originalEntity == null)
                    throw new InvalidOperationException($"Entity with ID {entity.Id} not found");
                
                // Update audit properties
                entity.ModifiedBy = changedBy;
                entity.ModifiedDate = DateTime.UtcNow;
                
                // Track property changes
                var propertyChanges = DetectChanges(originalEntity, entity);
                
                entity.AddVersionRecord(changedBy, "Updated", propertyChanges);
                context.Set<TEntity>().Update(entity);
                await context.SaveChangesAsync();
            }

            public virtual async Task DeleteAsync(TEntity entity, string changedBy)
            {
                entity.IsDeleted = true;
                entity.DeletedBy = changedBy;
                entity.DeletedDate = DateTime.UtcNow;
                entity.AddVersionRecord(changedBy, "Deleted");
                context.Set<TEntity>().Update(entity);
                await context.SaveChangesAsync();
            }

            protected virtual IQueryable<TEntity> GetAll(bool includeHistory = false)
            {
                var query = context.Set<TEntity>().AsNoTracking();
                if (!includeHistory)
                    query = query.Where(e => !e.IsDeleted);
                return query;
            }

            public virtual IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>>? predicate, bool includeHistory = false)
            {
                var query = GetAll(includeHistory);
                if (predicate is null)
                    return query;
                return query.Where(predicate);
            }

            public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, bool includeHistory = false)
            {
                var query = GetAll(includeHistory);
                return await query.FirstOrDefaultAsync(predicate);
            }
            
            /// <summary>
            /// Gets an entity at a specific point in time
            /// </summary>
            public virtual async Task<TEntity?> GetEntityVersionAsync(TKey id, DateTime pointInTime)
            {
                var currentEntity = await context.Set<TEntity>()
                    .Include(e => e.HistoryRecords)
                    .ThenInclude(h => h.PropertyChanges)
                    .FirstOrDefaultAsync(e => e.Id!.Equals(id));
                
                if (currentEntity == null)
                    return null;
                    
                return currentEntity.GetVersion<TEntity>(pointInTime);
            }
            
            /// <summary>
            /// Gets the history of changes for an entity
            /// </summary>
            public virtual async Task<List<EntityHistoryRecord>> GetEntityHistoryAsync(TKey id)
            {
                var entity = await context.Set<TEntity>()
                    .Include(e => e.HistoryRecords)
                    .ThenInclude(h => h.PropertyChanges)
                    .FirstOrDefaultAsync(e => e.Id!.Equals(id));
                
                return entity?.HistoryRecords.OrderByDescending(h => h.ChangeDate).ToList() ?? new List<EntityHistoryRecord>();
            }
            
            /// <summary>
            /// Detects changes between original and modified entity
            /// </summary>
            private Dictionary<string, (object? OldValue, object? NewValue)> DetectChanges(TEntity original, TEntity modified)
            {
                var changes = new Dictionary<string, (object? OldValue, object? NewValue)>();
                var properties = typeof(TEntity).GetProperties()
                    .Where(p => p.CanRead && p.CanWrite && 
                               !p.Name.Equals(nameof(IEntityHistory.HistoryRecords)) && 
                               !p.Name.Equals(nameof(BaseEntity<TKey>.ModifiedBy)) &&
                               !p.Name.Equals(nameof(BaseEntity<TKey>.ModifiedDate)));
                
                foreach (var property in properties)
                {
                    var originalValue = property.GetValue(original);
                    var modifiedValue = property.GetValue(modified);
                    
                    // Check if values are different
                    if (!Equals(originalValue, modifiedValue))
                    {
                        changes.Add(property.Name, (originalValue, modifiedValue));
                    }
                }
                
                return changes;
            }
        }
