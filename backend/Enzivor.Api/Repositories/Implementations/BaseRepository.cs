using Enzivor.Api.Data;
using Enzivor.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Enzivor.Api.Repositories.Implementations
{
    /// <summary>
    /// Generic repository base class providing common CRUD operations.
    /// </summary>
    /// <typeparam name="TEntity">Entity type managed by this repository.</typeparam>
    public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
    {
        protected readonly AppDbContext _db;
        protected readonly DbSet<TEntity> _set;

        protected BaseRepository(AppDbContext db)
        {
            _db = db;
            _set = db.Set<TEntity>();
        }

        public virtual async Task<List<TEntity>> GetAllAsync(CancellationToken ct = default)
        {
            return await _set.AsNoTracking().ToListAsync(ct);
        }

        public virtual async Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _set.FindAsync(new object[] { id }, ct);
        }

        public virtual async Task AddAsync(TEntity entity, CancellationToken ct = default)
        {
            await _set.AddAsync(entity, ct);
        }

        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
        {
            await _set.AddRangeAsync(entities, ct);
        }

        public virtual Task DeleteAsync(TEntity entity, CancellationToken ct = default)
        {
            _set.Remove(entity);
            return Task.CompletedTask;
        }

        public virtual async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _db.SaveChangesAsync(ct);
        }
    }
}