using AgriBuy.Contracts;
using AgriBuy.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AgriBuy.MySql
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly DefaultDbContext DbContext;
        private readonly DbSet<TEntity> DbSet;

        public Repository(DefaultDbContext defaultDbContext)
        {
            DbContext = defaultDbContext;
            DbSet = DbContext.Set<TEntity>();
        }

        public async Task AddAsync(TEntity model)
        {
            await DbSet.AddAsync(model);
        }

        public async Task AddAsync(TEntity[] models)
        {
            await DbSet.AddRangeAsync(models);
        }

        public IQueryable<TEntity> All()
        {
            return DbSet.AsNoTracking();
        }

        public IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return DbSet.Where(predicate);
        }

        public void Update(TEntity model)
        {
            DbSet.Update(model);
        }

        public void Update(TEntity[] models)
        {
            DbSet.UpdateRange(models);
        }

        public void Delete(TEntity model)
        {
            DbSet.Remove(model);
        }

        public void Delete(TEntity[] models)
        {
            DbSet.RemoveRange(models);
        }

        public async Task SaveChangesAsync()
        {
            await DbContext.SaveChangesAsync();
        }

        public void DetachTrackedEntity(Guid id)
        {
            var tracked = DbSet.Local.FirstOrDefault(e => EF.Property<Guid>(e, "Id") == id);
            if (tracked != null)
                DbContext.Entry(tracked).State = EntityState.Detached; 
        }
    }
}
