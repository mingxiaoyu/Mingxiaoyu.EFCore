using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Mingxiaoyu.EFCore.UoW.Tests
{
    public class RepositoryTests : IDisposable
    {
        private readonly DbConnection _connection;
        private readonly DbContextOptions<UowTestDbContext> _contextOptions;

        public RepositoryTests()
        {
            // Initialize the SQLite in-memory database
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            _contextOptions = new DbContextOptionsBuilder<UowTestDbContext>()
                .UseSqlite(_connection)
                //.UseLazyLoadingProxies()
                .Options;

            // Ensure the database is created and seed initial data
            using var context = new UowTestDbContext(_contextOptions);
            context.Database.EnsureCreated();
            SeedData(context);
        }

        private void SeedData(UowTestDbContext context)
        {
            // Seed products
            var product1 = new Product { Id = Guid.NewGuid(), ProductName = "Product1" };
            var product2 = new Product { Id = Guid.NewGuid(), ProductName = "Product2" };
            context.Products.AddRange(product1, product2);
            context.SaveChanges();

            // Seed orders related to the products
            context.Orders.AddRange(
                new Order { Id = 1, Name = "Order1", ProductId = product1.Id },
                new Order { Id = 2, Name = "Order2", ProductId = product2.Id }
            );
            context.SaveChanges();
        }

        private UowTestDbContext CreateContext() => new UowTestDbContext(_contextOptions);
        public void Dispose() => _connection.Dispose();

        private Repository<Product> CreateProductRepository(UowTestDbContext context) => new Repository<Product>(context);
        private Repository<Order, int> CreateOrderRepository(UowTestDbContext context) => new Repository<Order, int>(context);

        private void ResetDatabase(UowTestDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            SeedData(context);
        }

        [Fact]
        public void Add_Should_Add_Product()
        {
            using var context = CreateContext();
            ResetDatabase(context);
            var repository = CreateProductRepository(context);

            var newProduct = new Product { Id = Guid.NewGuid(), ProductName = "Product3" };
            repository.Add(newProduct);
            context.SaveChanges();

            var result = context.Products.Find(newProduct.Id);
            Assert.NotNull(result);
            Assert.Equal("Product3", result.ProductName);
        }

        [Fact]
        public void GetById_Should_Return_Product()
        {
            using var context = CreateContext();
            ResetDatabase(context);
            var repository = CreateProductRepository(context);

            var product = context.Products.First();
            var result = repository.GetById(product.Id);

            Assert.NotNull(result);
            Assert.Equal(product.ProductName, result.ProductName);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Product()
        {
            using var context = CreateContext();
            ResetDatabase(context);
            var repository = CreateProductRepository(context);

            var product = context.Products.First();
            var result = await repository.GetByIdAsync(product.Id);

            Assert.NotNull(result);
            Assert.Equal(product.ProductName, result.ProductName);
        }

        [Fact]
        public void Delete_Should_Remove_Product()
        {
            using var context = CreateContext();
            ResetDatabase(context);
            var repository = CreateProductRepository(context);

            var product = context.Products.First();
            repository.Delete(product.Id);
            context.SaveChanges();

            var result = context.Products.Find(product.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_Product()
        {
            using var context = CreateContext();
            ResetDatabase(context);
            var repository = CreateProductRepository(context);

            var product = context.Products.First();
            await repository.DeleteAsync(product.Id);
            await context.SaveChangesAsync();

            var result = context.Products.Find(product.Id);
            Assert.Null(result);
        }

        [Fact]
        public void GetAll_Should_Return_All_Products()
        {
            using var context = CreateContext();
            ResetDatabase(context);
            var repository = CreateProductRepository(context);

            var result = repository.GetAll();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_Products()
        {
            using var context = CreateContext();
            ResetDatabase(context);
            var repository = CreateProductRepository(context);

            var result = await repository.GetAllAsync();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void Add_Should_Throw_Exception_For_Duplicate_ProductId()
        {
            using var context = CreateContext();
            ResetDatabase(context);
            var repository = CreateProductRepository(context);

            var duplicateProduct = new Product { Id = context.Products.First().Id, ProductName = "DuplicateProduct" };

            Assert.ThrowsAny<Exception>(() =>
            {
                repository.Add(duplicateProduct);
                context.SaveChanges();
            });
        }

        [Fact]
        public void GetById_Should_Return_Null_For_Invalid_ProductId()
        {
            using var context = CreateContext();
            ResetDatabase(context);
            var repository = CreateProductRepository(context);

            var result = repository.GetById(Guid.NewGuid()); // Non-existent Product ID

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_For_Invalid_ProductId()
        {
            using var context = CreateContext();
            ResetDatabase(context);
            var repository = CreateProductRepository(context);

            var result = await repository.GetByIdAsync(Guid.NewGuid()); // Non-existent Product ID

            Assert.Null(result);
        }

        [Fact]
        public void Add_Should_Add_Order()
        {
            using var context = CreateContext();
            var orderRepository = CreateOrderRepository(context);
            var productRepository = CreateProductRepository(context);

            var product = productRepository.GetAll().First();
            var newOrder = new Order { Id = 3, Name = "Order3", ProductId = product.Id };
            orderRepository.Add(newOrder);
            context.SaveChanges();

            var result = context.Orders.Find(3);
            Assert.NotNull(result);
            Assert.Equal("Order3", result.Name);
            Assert.Equal(product.Id, result.ProductId);
        }

        [Fact]
        public void GetById_Should_Return_Order_With_Product()
        {
            using var context = CreateContext();
            var repository = CreateOrderRepository(context);

            var order = repository.GetById(1);

            Assert.NotNull(order);
            Assert.Equal("Order1", order.Name);
            Assert.Null(order.Product);

            order = repository.DbSet.Include(x => x.Product).FirstOrDefault(o => o.Id == 1);
            Assert.Equal("Product1", order.Product.ProductName);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Order_With_Product()
        {
            using var context = CreateContext();
            var repository = CreateOrderRepository(context);

            var order = await repository.GetByIdAsync(1);

            Assert.NotNull(order);
            Assert.Equal("Order1", order.Name);
            Assert.Null(order.Product);

            order = await repository.DbSet.Include(x => x.Product).FirstOrDefaultAsync(o => o.Id == 1);
            Assert.Equal("Product1", order.Product.ProductName);
        }

        [Fact]
        public void Delete_Should_Remove_Order()
        {
            using var context = CreateContext();
            var repository = CreateOrderRepository(context);

            repository.Delete(1);
            context.SaveChanges();

            var result = context.Orders.Find(1);
            Assert.Null(result);
        }

        [Fact]
        public void GetAll_Should_Return_All_Orders_With_Products()
        {
            using var context = CreateContext();
            var repository = CreateOrderRepository(context);

            var result = repository.GetAll();

            Assert.Equal(2, result.Count());
            Assert.All(result, order => Assert.Null(order.Product));

            result = repository.DbSet.Include(x => x.Product).ToList();
            Assert.All(result, order => Assert.NotNull(order.Product));
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_Orders_With_Products()
        {
            using var context = CreateContext();
            var repository = CreateOrderRepository(context);

            var result = await repository.GetAllAsync();

            Assert.Equal(2, result.Count());
            Assert.All(result, order => Assert.Null(order.Product));

            result = await repository.DbSet.Include(x=>x.Product).ToListAsync();
            Assert.All(result, order => Assert.NotNull(order.Product));
        }
    }
}
