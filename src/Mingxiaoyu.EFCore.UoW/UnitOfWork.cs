using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Mingxiaoyu.EFCore.UoW;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly IDbContext _context;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Type, object> _repositories = new();
    private IDbContextTransaction _transaction;

    public UnitOfWork(IDbContext context, IServiceProvider serviceProvider)
    {
        _context = context;
        _serviceProvider = serviceProvider;
    }

    public IRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : class
    {
        var type = typeof(TEntity);
        if (!_repositories.ContainsKey(type))
        {
            var repositoryInstance = _serviceProvider.GetRequiredService<IRepository<TEntity, TKey>>();
            _repositories[type] = repositoryInstance;
        }

        return (IRepository<TEntity, TKey>)_repositories[type];
    }
    public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity);
        if (!_repositories.ContainsKey(type))
        {
            var repositoryInstance = _serviceProvider.GetRequiredService<IRepository<TEntity>>();
            _repositories[type] = repositoryInstance;
        }

        return (IRepository<TEntity>)_repositories[type];
    }

    public TRepository ResolveRepository<TRepository>()
        where TRepository : class
    {
        var type = typeof(TRepository);
        if (!_repositories.ContainsKey(type))
        {
            var repositoryInstance = _serviceProvider.GetRequiredService<TRepository>();
            _repositories[type] = repositoryInstance;
        }

        return (TRepository)_repositories[type];
    }

    public async Task BeginTransactionAsync()
    {
        if (_transaction == null)
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await CompleteAsync();
            await _transaction?.CommitAsync();
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        try
        {
            await _transaction?.RollbackAsync();
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
