using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Mingxiaoyu.EFCore.NoLock.Tests
{
    public class TestDatabaseFixture
    {
        private const string ConnectionString = @"Server=(localdb)\mssqllocaldb;Database=EFNoLockTest;Trusted_Connection=True;ConnectRetryCount=0";

        private static readonly object _lock = new();
        private static bool _databaseInitialized;

        public TestDatabaseFixture()
        {
            lock (_lock)
            {
                if (!_databaseInitialized)
                {
                    using (var context = CreateContext())
                    {
                        context.Database.EnsureDeleted();
                        context.Database.EnsureCreated();
                        context.AddRange(
                        new Product { Id = 1, ProductName = "Test Product" },
                            new Order { Id = 1, Name = "Test Order", ProductId = 1 });
                        context.SaveChanges();
                    }

                    _databaseInitialized = true;
                }
            }
        }

        public TestDbContext CreateContext(StringWriter logOutput = null)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlServer(ConnectionString)
                .EnableSqlServerWithNoLock();

            if (logOutput != null)
            {
                optionsBuilder.LogTo(logOutput.WriteLine, LogLevel.Information);
            }

            return new TestDbContext(optionsBuilder.Options);
        }
    }
}
