using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AgriBuy.Contracts
{
    public interface IRepository<TEntity>
    {
        Task AddAsync(TEntity model);

        Task AddAsync(TEntity[] models);

        IQueryable<TEntity> All();

        IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);

        void Update(TEntity model);

        void Update(TEntity[] models);

        void Delete(TEntity model);

        void Delete(TEntity[] models);

        Task SaveChangesAsync();

        void DetachTrackedEntity(Guid id);
    }
}
