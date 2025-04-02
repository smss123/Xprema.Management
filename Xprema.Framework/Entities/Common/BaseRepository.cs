using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Xprema.Framework.Entities.HistoryFeature;

namespace Xprema.Framework.Entities.Common;

        public abstract class BaseRepository<TEntity, TKey>(DbContext context)
            where TEntity : BaseEntity<TKey>, IEntityHistory
        {
            public virtual async Task AddAsync(TEntity entity, string changedBy)
            {
                entity.AddHistoryRecord(changedBy, "Created");
                await context.Set<TEntity>().AddAsync(entity);
                await context.SaveChangesAsync();
            }

            public virtual async Task UpdateAsync(TEntity entity, string changedBy)
            {
                entity.AddHistoryRecord(changedBy, "Updated");
                context.Set<TEntity>().Update(entity);
                await context.SaveChangesAsync();
            }

            public virtual async Task DeleteAsync(TEntity entity, string changedBy)
            {
                entity.IsDeleted = true;
                entity.DeletedBy = changedBy;
                entity.DeletedDate = DateTime.UtcNow;
                entity.AddHistoryRecord(changedBy, "Deleted");
                context.Set<TEntity>().Update(entity);
                await context.SaveChangesAsync();
            }

            protected virtual IQueryable<TEntity> GetAll(bool includeHistory = false)
            {
                var query = context.Set<TEntity>().AsNoTracking();

                if (!includeHistory)
                {
                    query = query.Where(e => !e.IsDeleted);
                }

                return query;
            }

            public virtual IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>>? predicate, bool includeHistory = false)
            {
                var query = GetAll(includeHistory);
                if (predicate is null)
                {
                    return query;
                }
                return query.Where(predicate);
            }

            public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, bool includeHistory = false)
            {
                var query = GetAll(includeHistory);
                return await query.FirstOrDefaultAsync(predicate);
            }
        }
