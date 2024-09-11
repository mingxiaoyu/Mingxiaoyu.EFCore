using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Mingxiaoyu.EFCore.UoW.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        private readonly IServiceCollection _services;

        public ServiceCollectionExtensionsTests()
        {
            _services = new ServiceCollection();

            // Register DbContext with SQLite in-memory database
            _services.AddDbContext<UowTestDbContext>(options =>
                options.UseSqlite("Filename=:memory:"));

            // Register IDbContext to use UowTestDbContext
            _services.AddScoped<IDbContext>(provider => provider.GetRequiredService<UowTestDbContext>());
        }

        private IServiceProvider BuildServiceProvider()
        {
            return _services.BuildServiceProvider();
        }

        [Fact]
        public void AddRepositories_Should_Register_Generic_Repositories()
        {
            // Arrange
            var assemblies = new[] { typeof(ServiceCollectionExtensionsTests).Assembly };
            _services.AddRepositories(assemblies);

            // Act
            var serviceProvider = BuildServiceProvider();

            // Assert
            var orderRepository = serviceProvider.GetService<IRepository<Order, int>>();
            Assert.NotNull(orderRepository);
            Assert.IsType<Repository<Order, int>>(orderRepository);

            var productRepository = serviceProvider.GetService<IRepository<Product>>();
            Assert.NotNull(productRepository);
            Assert.IsType<Repository<Product>>(productRepository);

            var specificProductRepository = serviceProvider.GetService<ProductRepository>();
            Assert.NotNull(specificProductRepository);
            Assert.IsType<ProductRepository>(specificProductRepository);
        }

        [Fact]
        public void AddDbContext_Should_Register_DbContext_As_IDbContext()
        {
            // Act
            var serviceProvider = BuildServiceProvider();

            // Assert
            var dbContext = serviceProvider.GetService<IDbContext>();
            Assert.NotNull(dbContext);
            Assert.IsType<UowTestDbContext>(dbContext);
        }

        [Fact]
        public void AddUnitOfWork_Should_Register_UnitOfWork()
        {
            // Arrange
            _services.AddUnitOfWork();

            // Act
            var serviceProvider = BuildServiceProvider();

            // Assert
            var unitOfWork = serviceProvider.GetService<IUnitOfWork>();
            Assert.NotNull(unitOfWork);
            Assert.IsType<UnitOfWork>(unitOfWork);
        }
    }
}
