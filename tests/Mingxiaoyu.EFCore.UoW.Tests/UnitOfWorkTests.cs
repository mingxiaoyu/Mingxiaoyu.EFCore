using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data.Common;
using System.Threading.Tasks;
using Xunit;

namespace Mingxiaoyu.EFCore.UoW.Tests
{
    public class UnitOfWorkTests : IDisposable
    {
        private readonly DbConnection _connection;
        private readonly DbContextOptions<UowTestDbContext> _contextOptions;

        public UnitOfWorkTests()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            _contextOptions = new DbContextOptionsBuilder<UowTestDbContext>()
                .UseSqlite(_connection)
                .Options;

            using var context = new UowTestDbContext(_contextOptions);
            context.Database.EnsureCreated();
            SeedData(context);
        }

        private void SeedData(UowTestDbContext context)
        {
            context.Products.Add(new Product { Id = Guid.NewGuid(), ProductName = "Product1" });
            context.SaveChanges();
        }

        private IServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();

            // Register DbContext
            services.AddScoped<UowTestDbContext>(_ => new UowTestDbContext(_contextOptions));

            // Register IDbContext if needed
            services.AddScoped<IDbContext>(provider => provider.GetRequiredService<UowTestDbContext>());

            // Register repositories
            services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(ProductRepository));

            services.AddUnitOfWork();

            return services.BuildServiceProvider();
        }

        private IUnitOfWork CreateUow()
        {
            var serviceProvider = BuildServiceProvider();
            var dbContext = serviceProvider.GetRequiredService<UowTestDbContext>();
            return new UnitOfWork(dbContext, serviceProvider);
        }

        [Fact]
        public async Task Can_Begin_And_Commit_Transaction()
        {
            var unitOfWork = CreateUow();
            // Act
            await unitOfWork.BeginTransactionAsync();
            var repo = unitOfWork.GetRepository<Product>();

            var newProduct = new Product { Id = Guid.NewGuid(), ProductName = "Product2" };
            repo.Add(newProduct);
            await unitOfWork.CompleteAsync();

            await unitOfWork.CommitTransactionAsync();

            // Assert
            Assert.Equal(2, await repo.DbSet.CountAsync());

        }

        [Fact]
        public async Task Can_Rollback_Transaction()
        {
            var unitOfWork = CreateUow();

            // Act
            await unitOfWork.BeginTransactionAsync();
            var repo = unitOfWork.GetRepository<Product>();

            var newProduct = new Product { Id = Guid.NewGuid(), ProductName = "Product2" };
            repo.Add(newProduct);
            await unitOfWork.CompleteAsync();

            await unitOfWork.RollbackTransactionAsync();

            // Assert
            Assert.Equal(1, await repo.DbSet.CountAsync());
        }

        [Fact]
        public void Can_Get_Generic_Repository()
        {
            var unitOfWork = CreateUow();

            // Act
            var repo = unitOfWork.GetRepository<Order, int>();

            // Assert
            Assert.NotNull(repo);
        }

        [Fact]
        public void Can_Get_Generic_Guid_Repository()
        {
            var unitOfWork = CreateUow();

            // Act
            var repo = unitOfWork.GetRepository<Product>();

            // Assert
            Assert.NotNull(repo);
        }

        [Fact]
        public void Can_Get_Specific_Repository()
        {
            var unitOfWork = CreateUow();

            // Act
            var repo = unitOfWork.ResolveRepository<ProductRepository>();

            // Assert
            Assert.NotNull(repo);
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
