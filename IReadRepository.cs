using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLogic
{
    /// <summary>
    /// Informs an underlying relational data store to return sets of read-only entity instances.
    /// </summary>
    public interface IReadRepository
    {
        /// <summary>
        /// Inform an underlying relational data store to return a set of read-only entity instances.
        /// </summary>
        /// <typeparam name="TEntity">Type of the entity instances that the underlying relational data
        /// store should return.</typeparam>
        /// <returns>IQueryable for set of read-only TEntity instances from an underlying relational
        /// data store.</returns>
        IQueryable<TEntity> Query<TEntity>() where TEntity : EntityBase;

        Task<TEntity> FindAsync<TEntity>(int entityId) where TEntity : EntityBase;

        IQueryable<TResult> ExecuteSqlQuery<TResult>(string sql, params object[] parameters);
    }
}
