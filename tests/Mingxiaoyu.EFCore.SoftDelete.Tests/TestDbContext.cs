using Microsoft.EntityFrameworkCore;

namespace Mingxiaoyu.EFCore.SoftDelete.Tests
{
    internal class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options)
            : base(options)
        {

        }

        public DbSet<TestEntity> TestEntities { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplySoftDeleteFilter();
        }
    }

    internal class TestEntity : ISoftDeletable
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
    }
}
