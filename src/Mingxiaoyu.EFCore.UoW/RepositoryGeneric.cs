using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mingxiaoyu.EFCore.UoW
{
    public class Repository<TEntity> : Repository<TEntity, Guid>, IRepository<TEntity> where TEntity : class
    {
        public Repository(IDbContext context) : base(context)
        {
        }
    }

    public class Repository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class
    {
        protected readonly IDbContext _context;
        public DbSet<TEntity> DbSet => _context.Set<TEntity>();

        public Repository(IDbContext context)
        {
            _context = context;
        }

        public TEntity GetById(TKey id)
        {
            return DbSet.Find(id);
        }

        public IEnumerable<TEntity> GetAll()
        {
            return DbSet.ToList();
        }

        public void Add(TEntity entity)
        {
            DbSet.Add(entity);
        }

        public void Update(TEntity entity)
        {
            DbSet.Update(entity);
        }

        public void Delete(TKey id)
        {
            var entity = GetById(id);
            if (entity != null)
            {
                DbSet.Remove(entity);
            }
        }

        public async Task<TEntity> GetByIdAsync(TKey id)
        {
            return await DbSet.FindAsync(id);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await DbSet.ToListAsync();
        }

        public async Task AddAsync(TEntity entity)
        {
            await DbSet.AddAsync(entity);
        }

        public async Task UpdateAsync(TEntity entity)
        {
            DbSet.Update(entity);
        }

        public async Task DeleteAsync(TKey id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                DbSet.Remove(entity);
            }
        }
    }

}
