using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mingxiaoyu.EFCore.Audit
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly IUserContext userContext;

        public AuditInterceptor(IUserContext userContext)
        {
            this.userContext = userContext;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            var context = (DbContext)eventData.Context!;

            var auditLogs = GenerateAuditLogs(context);

            context.Set<AuditLog>().AddRange(auditLogs);

            return base.SavingChanges(eventData, result);
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
          DbContextEventData eventData,
          InterceptionResult<int> result,
          CancellationToken cancellationToken = default)
        {
            var context = (DbContext)eventData.Context!;

            var auditLogs = GenerateAuditLogs(context);

            context.Set<AuditLog>().AddRange(auditLogs);

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private IEnumerable<AuditLog> GenerateAuditLogs(DbContext context)
        {
            var entries = context.ChangeTracker.Entries()
                .Where(e => e.Entity.GetType().GetCustomAttributes(typeof(AuditableAttribute), true).Any())
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                .ToList();

            var auditLogs = new List<AuditLog>();

            foreach (var entry in entries)
            {
                var key = GetCompositeKey(entry);

                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    TableName = entry.Entity.GetType().Name,
                    RecordID = key,
                    Operation = entry.State.ToString(),
                    OldValue = entry.State == EntityState.Deleted || entry.State == EntityState.Modified ? GetOriginalValues(entry) : null,
                    NewValue = entry.State == EntityState.Added || entry.State == EntityState.Modified ? GetCurrentValues(entry) : null,
                    ChangedBy = userContext.CurrentUser,
                    ChangedAt = DateTime.UtcNow
                };

                auditLogs.Add(auditLog);
            }

            return auditLogs;
        }

        private string GetCompositeKey(EntityEntry entry)
        {
            var keyProperties = entry.OriginalValues.Properties
                .Where(p => p.IsKey())
                .ToList();

            var keyValues = keyProperties
                .Select(p => entry.OriginalValues[p]?.ToString())
                .ToArray();

            return string.Join(":", keyValues);
        }

        private string GetOriginalValues(EntityEntry entry)
        {
            var originalValues = entry.OriginalValues.Properties
                .ToDictionary(p => p.Name, p => entry.OriginalValues[p]?.ToString());

            return string.Join(", ", originalValues.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
        }

        private string GetCurrentValues(EntityEntry entry)
        {
            var currentValues = entry.CurrentValues.Properties
                .ToDictionary(p => p.Name, p => entry.CurrentValues[p]?.ToString());

            return string.Join(", ", currentValues.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
        }
    }
}