using Microsoft.EntityFrameworkCore;

namespace Mingxiaoyu.EFCore.UoW
{
    public interface IRepository<TEntity>: IRepository<TEntity,Guid> where TEntity : class
    {

    }

    public interface IRepository<TEntity, TKey> where TEntity : class
    {
        DbSet<TEntity> DbSet { get; }

        TEntity GetById(TKey id);
        IEnumerable<TEntity> GetAll();
        void Add(TEntity entity);
        void Update(TEntity entity);
        void Delete(TKey id);

        Task<TEntity> GetByIdAsync(TKey id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task AddAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(TKey id);
    }

}
