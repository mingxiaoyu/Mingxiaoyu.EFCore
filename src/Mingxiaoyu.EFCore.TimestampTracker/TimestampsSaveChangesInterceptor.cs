using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Mingxiaoyu.EFCore.TimestampTracker
{
    public class TimestampsSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly IUserContext userContext;

        public TimestampsSaveChangesInterceptor(IUserContext userContext)
        {
            this.userContext = userContext;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            UpdateTimestamps(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
              DbContextEventData eventData,
              InterceptionResult<int> result,
              CancellationToken cancellationToken = default)
        {
            UpdateTimestamps(eventData.Context);
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void UpdateTimestamps(DbContext context)
        {
            var now = DateTime.UtcNow;
            var entities = context.ChangeTracker.Entries<IHasTimestamps>();

            foreach (var entity in entities)
            {
                if (entity.State == EntityState.Added)
                {
                    entity.Entity.CreatedAt = now;
                    entity.Entity.CreatedBy = userContext.CurrentUser;
                }
                else if (entity.State == EntityState.Modified)
                {
                    entity.Entity.UpdatedAt = now;
                    entity.Entity.UpdatedBy = userContext.CurrentUser;
                }
            }
        }
    }
}
