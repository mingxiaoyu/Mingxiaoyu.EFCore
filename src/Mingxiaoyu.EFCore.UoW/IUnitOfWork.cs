using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mingxiaoyu.EFCore.UoW
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : class;
        IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
        TRepository ResolveRepository<TRepository>() where TRepository : class;
        Task<int> CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }

}
