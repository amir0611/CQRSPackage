using System.Data.Entity;

namespace DataAccessLogic
{
    public interface IDbContext
    {
        IDbSet<TEntity> Set<TEntity>() where TEntity : EntityBase;
        int SaveChanges();
    }
}
