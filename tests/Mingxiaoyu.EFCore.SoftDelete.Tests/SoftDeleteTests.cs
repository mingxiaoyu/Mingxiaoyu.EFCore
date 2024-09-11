using Microsoft.EntityFrameworkCore;

namespace Mingxiaoyu.EFCore.SoftDelete.Tests
{
    public class SoftDeleteTests
    {       

        [Fact]
        public async Task SoftDeleteInterceptor_MarksEntityAsDeleted()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite("Filename=:memory:")
                .AddInterceptors(new SoftDeleteSaveChangesInterceptor())
                .Options;

            using var context = new TestDbContext(options);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();


            // Add a test entity
            context.TestEntities.Add(new TestEntity { Id = 1 });
            await context.SaveChangesAsync();

            // Mark entity as deleted
            var entity = context.TestEntities.Find(1);
            context.TestEntities.Remove(entity);
            await context.SaveChangesAsync();

            // Ensure the entity was soft-deleted
            var deletedEntity = context.TestEntities.Find(1);
            Assert.NotNull(deletedEntity);
            Assert.True(deletedEntity.IsDeleted);
        }

        [Fact]
        public void ApplySoftDeleteFilter_FiltersOutDeletedEntities()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
               .UseSqlite("Filename=:memory:")
               .AddInterceptors(new SoftDeleteSaveChangesInterceptor())
               .Options;

            using var context = new TestDbContext(options);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();

            // Add entities
            context.TestEntities.Add(new TestEntity { Id = 1, IsDeleted = false });
            context.TestEntities.Add(new TestEntity { Id = 2, IsDeleted = true });
            context.SaveChanges();

            // Query the context with the soft delete filter applied
            var activeEntities = context.TestEntities.ToList();

            Assert.Single(activeEntities);
            Assert.Equal(1, activeEntities[0].Id);
        }
    }
}
