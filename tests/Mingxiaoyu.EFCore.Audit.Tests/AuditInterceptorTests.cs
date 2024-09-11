using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Moq;
using System.Data.Common;

namespace Mingxiaoyu.EFCore.Audit.Tests
{
    public class AuditInterceptorTests : IDisposable
    {
        private readonly Mock<IUserContext> _mockUserContext;
        private readonly AuditInterceptor _interceptor;
        private readonly DbContextOptions<AuditTestDbContext> _contextOptions;
        private readonly DbConnection _connection;

        public AuditInterceptorTests()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            _mockUserContext = new Mock<IUserContext>();
            _mockUserContext.Setup(x => x.CurrentUser).Returns("TestUser");

            _interceptor = new AuditInterceptor(_mockUserContext.Object);

            _contextOptions = new DbContextOptionsBuilder<AuditTestDbContext>()
                .UseSqlite(_connection)
                .AddInterceptors(_interceptor)
                .Options;

            SeedData();
        }

        private void SeedData()
        {
            using var context = new AuditTestDbContext(_contextOptions);
            context.Database.EnsureCreated();

            context.AddRange(
                new Product { Id = 1, ProductName = "Product1", Price = 1 }
            );
            context.SaveChanges();
        }

        private AuditTestDbContext CreateContext() => new AuditTestDbContext(_contextOptions);

        public void Dispose() => _connection.Dispose();

        [Fact]
        public void AddsAuditLogOnAdd()
        {
            // Arrange
            using var context = CreateContext();
            var product = new Product { Id = 2, ProductName = "Product2", Price = 2 };
            context.Add(product);
            context.SaveChanges();

            // Act
            var auditLogs = context.Set<AuditLog>().ToList();

            // Assert
            Assert.Equal(2, auditLogs.Count);
            var auditLog = auditLogs.Last();
            Assert.Equal("Product", auditLog.TableName);
            Assert.Equal(product.Id.ToString(), auditLog.RecordID);
            Assert.Equal("Added", auditLog.Operation);
            Assert.Equal("TestUser", auditLog.ChangedBy);
            Assert.Null(auditLog.OldValue);
            Assert.Contains("ProductName: Product2", auditLog.NewValue);
        }

        [Fact]
        public void AddsAuditLogOnModify()
        {
            // Arrange
            using var context = CreateContext();
            var product = context.Set<Product>().First();
            product.Price = 20;
            context.SaveChanges();

            // Act
            var auditLogs = context.Set<AuditLog>().ToList();

            // Assert
            Assert.Equal(2, auditLogs.Count);
            var auditLog = auditLogs.Last();
            Assert.Equal("Product", auditLog.TableName);
            Assert.Equal(product.Id.ToString(), auditLog.RecordID);
            Assert.Equal("Modified", auditLog.Operation);
            Assert.Contains("Price: 1", auditLog.OldValue);
            Assert.Contains("Price: 20", auditLog.NewValue);
            Assert.Equal("TestUser", auditLog.ChangedBy);
        }

        [Fact]
        public void AddsAuditLogOnDelete()
        {
            // Arrange
            using var context = CreateContext();
            var product = context.Set<Product>().First();
            context.Remove(product);
            context.SaveChanges();

            // Act
            var auditLogs = context.Set<AuditLog>().ToList();

            // Assert
            Assert.Equal(2, auditLogs.Count);
            var auditLog = auditLogs.Last();
            Assert.Equal("Product", auditLog.TableName);
            Assert.Equal(product.Id.ToString(), auditLog.RecordID);
            Assert.Equal("Deleted", auditLog.Operation);
            Assert.Contains("ProductName: Product1", auditLog.OldValue);
            Assert.Null(auditLog.NewValue);
            Assert.Equal("TestUser", auditLog.ChangedBy);
        }

        [Fact]
        public void DoesNotCreateAuditLogForNonAuditedEntity()
        {
            // Arrange
            var unAudit = new UnAudit { SomeProperty = "SomeValue" };

            using var context = CreateContext();
            context.Add(unAudit);
            context.SaveChanges();

            // Act
            var auditLogs = context.Set<AuditLog>().Where(x => x.TableName == "UnAudit").ToList();

            // Assert
            Assert.Empty(auditLogs); // No audit logs should be created for non-audited entities
        }

        [Fact]
        public async Task CreatesAuditLogForEntityWithCompositeKey()
        {
            // Arrange
            var entity = new MorePrimaryKey
            {
                PrimaryKeyOne = Guid.NewGuid(),
                PrimaryKeyTwo = Guid.NewGuid(),
                SomeProperty = "InitialValue"
            };

            using var context = CreateContext();
            context.Add(entity);
            await context.SaveChangesAsync();

            // Act
            var auditLogs = context.Set<AuditLog>().ToList();

            // Assert
            Assert.Equal(2, auditLogs.Count); // Expecting 2 logs for composite key entities
            var auditLog = auditLogs.Last();

            Assert.Equal("MorePrimaryKey", auditLog.TableName);
            Assert.Equal($"{entity.PrimaryKeyOne}:{entity.PrimaryKeyTwo}", auditLog.RecordID);
            Assert.Equal("Added", auditLog.Operation);
            Assert.Contains("SomeProperty: InitialValue", auditLog.NewValue);
            Assert.Equal("TestUser", auditLog.ChangedBy);
        }
    }
}
