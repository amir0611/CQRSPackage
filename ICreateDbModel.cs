using System.Data.Entity;

namespace DataAccessLogic
{
    public interface ICreateDbModel
    {
        void Create(DbModelBuilder modelBuilder);
    }
}
