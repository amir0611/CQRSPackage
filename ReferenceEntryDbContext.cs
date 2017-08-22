using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity.Migrations;

namespace DataAccessLogic
{
    public class ReferenceEntryDbContext : DbContext, IWriteRepository
    {
        public ReferenceEntryDbContext() : base("name=ReferenceEntryDbContext")
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            Configuration.AutoDetectChangesEnabled = true;
            Configuration.ValidateOnSaveEnabled = false;
           
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ReferenceEntryDbContext, DbMigrationsConfiguration<ReferenceEntryDbContext>>());
        }      

        public ICreateDbModel ModelCreator { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            ModelCreator = ModelCreator ?? new DefaultDbModelCreator();
            ModelCreator.Create(modelBuilder);
            base.OnModelCreating(modelBuilder);           
        }

        public TEntity Get<TEntity>(object firstKeyValue, params object[] otherKeyValues) where TEntity : EntityBase
        {
            if (firstKeyValue == null)
            {
                throw new ArgumentNullException(nameof(firstKeyValue));
            }
            var keyValues = new List<object> { firstKeyValue };
            if (otherKeyValues != null)
            {
                keyValues.AddRange(otherKeyValues);
            }
           
            return Set<TEntity>().Find(keyValues.ToArray());
        }

        public IQueryable<TEntity> Get<TEntity>() where TEntity : EntityBase
        {
            return new EntitySet<TEntity>(Set<TEntity>(), this);
        }

        public void Create<TEntity>(TEntity entity) where TEntity : EntityBase
        {
            if (Entry(entity).State == EntityState.Detached)
            {
                Set<TEntity>().Add(entity);
            }
        }

        public void Delete<TEntity>(TEntity entity) where TEntity : EntityBase
        {
            if (Entry(entity).State != EntityState.Deleted)
            {
                if (Entry(entity).State == EntityState.Detached)
                {
                    Set<TEntity>().Attach(entity);
                }

                Set<TEntity>().Remove(entity);
            }
        }

        public void ExecuteSqlCommand(string sqlCommand)
        {
            if (string.IsNullOrEmpty(sqlCommand))
            {
                throw new ArgumentException("Argument is null or empty", nameof(sqlCommand));
            }

            Database.ExecuteSqlCommand(sqlCommand);
        }

        public IQueryable<TResult> ExecuteSqlQuery<TResult>(string sql, params object[] parameters)
        {
            return Database.SqlQuery<TResult>(sql, parameters).AsQueryable();
        }

        public void Update<TEntity>(TEntity entity) where TEntity : EntityBase
        {
            //var entry = Entry(entity);
            //entry.CurrentValues.SetValues(entity); 
            var tracked = Set<TEntity>().Find(KeyValuesFor(entity));
            if (tracked != null)
            {
                Entry(tracked).CurrentValues.SetValues(entity);
            }
        }

        public TEntity AddOrUpdate<TEntity>(TEntity entity) where TEntity : EntityBase
        {
            var tracked = Set<TEntity>().Find(KeyValuesFor(entity));
            if (tracked != null)
            {
                Entry(tracked).CurrentValues.SetValues(entity);
                return tracked;
            }

            Set<TEntity>().Add(entity);
            return entity;
        }

        public IQueryable<TEntity> Query<TEntity>() where TEntity : EntityBase
        {
            return new EntitySet<TEntity>(Set<TEntity>().AsNoTracking(), this);
        }

        public Task<TEntity> FindAsync<TEntity>(int entityId) where TEntity : EntityBase
        {
            return Set<TEntity>().FindAsync(entityId);
        }

        public int Save()
        {
            try
            {
                return SaveChanges();
            }
            catch (DbUpdateException e)
            {
                //Log
                return -1;
            }
        }

        public Task<int> SaveAsync()
        {
            try
            {
                return SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                throw e;
            }
        }

        public void DiscardChanges()
        {
            foreach (var entry in ChangeTracker.Entries().Where(x => x != null))
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Modified:
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Deleted:
                        entry.Reload();
                        break;
                }
            }
        }

        private object[] KeyValuesFor(object entity)
        {
            var entry = Entry(entity);
            return KeysFor(entity.GetType())
                .Select(k => entry.Property(k).CurrentValue)
                .ToArray();
        }

        private IEnumerable<string> KeysFor(Type entityType)
        {
            entityType = ObjectContext.GetObjectType(entityType);

            var metadataWorkspace =
                ((IObjectContextAdapter)this).ObjectContext.MetadataWorkspace;
            var objectItemCollection =
                (ObjectItemCollection)metadataWorkspace.GetItemCollection(DataSpace.OSpace);

            var ospaceType = metadataWorkspace
                .GetItems<EntityType>(DataSpace.OSpace)
                .SingleOrDefault(t => objectItemCollection.GetClrType(t) == entityType);

            if (ospaceType == null)
            {
                throw new ArgumentException(
                    string.Format(
                        "The type '{0}' is not mapped as an entity type.",
                        entityType.Name),
                    nameof(entityType));
            }

            return ospaceType.KeyMembers.Select(k => k.Name);
        }
    }
}
