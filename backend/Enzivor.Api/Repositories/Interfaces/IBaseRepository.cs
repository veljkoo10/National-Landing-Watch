namespace Enzivor.Api.Repositories.Interfaces
{
    /// <summary>
    /// Generic base repository interface defining common CRUD operations.
    /// </summary>
    /// <typeparam name="TEntity">Entity type for this repository.</typeparam>
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Returns all entities from the database.
        /// </summary>
        Task<List<TEntity>> GetAllAsync(CancellationToken ct = default);

        /// <summary>
        /// Returns a single entity by its primary key.
        /// </summary>
        Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default);

        /// <summary>
        /// Adds a single entity to the database context.
        /// </summary>
        Task AddAsync(TEntity entity, CancellationToken ct = default);

        /// <summary>
        /// Adds multiple entities to the database context.
        /// </summary>
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);

        /// <summary>
        /// Removes an entity from the database.
        /// </summary>
        Task DeleteAsync(TEntity entity, CancellationToken ct = default);

        /// <summary>
        /// Commits all pending changes to the database.
        /// </summary>
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}