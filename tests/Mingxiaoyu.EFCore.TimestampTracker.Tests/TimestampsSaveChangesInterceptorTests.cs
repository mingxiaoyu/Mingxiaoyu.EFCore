using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Mingxiaoyu.EFCore.TimestampTracker.Tests
{
    public class TimestampsSaveChangesInterceptorTests : IDisposable
    {
        private readonly Mock<IUserContext> _userContextMock;
        private readonly TimestampsSaveChangesInterceptor _interceptor;
        private readonly DbContextOptions<TestDbContext> _contextOptions;
        private readonly DbConnection _connection;

        public TimestampsSaveChangesInterceptorTests()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            _userContextMock = new Mock<IUserContext>();
            _userContextMock.Setup(u => u.CurrentUser).Returns("TestUser");

            _interceptor = new TimestampsSaveChangesInterceptor(_userContextMock.Object);

            _contextOptions = new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite(_connection)
                .AddInterceptors(_interceptor)
                .Options;

            SeedData();
        }

        private void SeedData()
        {
            using var context = new TestDbContext(_contextOptions);
            context.Database.EnsureCreated();

            context.Products.Add(new Product { Id = 1, ProductName = "Product1" });
            context.SaveChanges();
        }

        private TestDbContext CreateContext() => new TestDbContext(_contextOptions);

        public void Dispose() => _connection.Dispose();

        [Fact]
        public void AddsTimestampsOnAdd_Order()
        {
            // Arrange
            using var context = CreateContext();
            var order = new Order { Id = 1, Name = "Order1", ProductId = 1 };

            // Act
            context.Orders.Add(order);
            context.SaveChanges();

            // Assert
            var savedOrder = context.Orders.FirstOrDefault(o => o.Id == 1);
            Assert.NotNull(savedOrder);
            Assert.Equal("TestUser", savedOrder.CreatedBy);
            Assert.NotNull(savedOrder.CreatedAt);
            Assert.Null(savedOrder.UpdatedBy);
            Assert.Null(savedOrder.UpdatedAt);
        }

        [Fact]
        public async Task AddsTimestampsOnModify_Order()
        {
            // Arrange
            using var context = CreateContext();
            var order = new Order { Id = 1, Name = "Order1", ProductId = 1, CreatedBy = "InitialUser" };

            context.Orders.Add(order);
            context.SaveChanges();

            // Act
            order.Name = "UpdatedOrder";
            context.Orders.Update(order);
            await context.SaveChangesAsync();

            // Assert
            var updatedOrder = context.Orders.FirstOrDefault(o => o.Id == 1);
            Assert.Equal("TestUser", updatedOrder.UpdatedBy);
            Assert.NotNull(updatedOrder.UpdatedAt);
            Assert.Equal("TestUser", updatedOrder.CreatedBy);
        }

        [Fact]
        public async Task AddsTimestampsOnAdd_Product()
        {
            // Arrange
            using var context = CreateContext();
            var product = new Product { Id = 2, ProductName = "Product2" };

            // Act
            context.Products.Add(product);
            await context.SaveChangesAsync();

            // Assert
            var savedProduct = context.Products.FirstOrDefault(p => p.Id == 2);
            Assert.NotNull(savedProduct);
            Assert.Equal("TestUser", savedProduct.CreatedBy);
            Assert.NotNull(savedProduct.CreatedAt);
            Assert.Null(savedProduct.UpdatedBy);
            Assert.Null(savedProduct.UpdatedAt);
        }

        [Fact]
        public async Task AddsTimestampsOnModify_Product()
        {
            // Arrange
            using var context = CreateContext();
            var product = context.Products.FirstOrDefault(p => p.Id == 1);

            // Act
            product.ProductName = "UpdatedProduct";
            context.Products.Update(product);
            await context.SaveChangesAsync();

            // Assert
            var updatedProduct = context.Products.FirstOrDefault(p => p.Id == 1);
            Assert.Equal("TestUser", updatedProduct.UpdatedBy);
            Assert.NotNull(updatedProduct.UpdatedAt);
            Assert.Equal("TestUser", updatedProduct.CreatedBy);
        }
        
    }
}
