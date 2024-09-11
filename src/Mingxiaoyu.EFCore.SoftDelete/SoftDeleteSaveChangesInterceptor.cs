using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Mingxiaoyu.EFCore.SoftDelete;
using System.Linq;

namespace Mingxiaoyu.EFCore.SoftDelete
{
    public class SoftDeleteSaveChangesInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            // Perform the soft delete logic
            ApplySoftDeletes(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {

            // Perform the soft delete logic
            ApplySoftDeletes(eventData.Context);
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        }

        private void ApplySoftDeletes(DbContext context)
        {
            var entries = context.ChangeTracker.Entries<ISoftDeletable>()
                .Where(e => e.State == EntityState.Deleted)
                .ToList();

            foreach (var entry in entries)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
            }
        }
    }

}
