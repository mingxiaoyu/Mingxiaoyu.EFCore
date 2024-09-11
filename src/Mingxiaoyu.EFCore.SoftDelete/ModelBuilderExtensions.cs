using Microsoft.EntityFrameworkCore;
using Mingxiaoyu.EFCore.SoftDelete;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Mingxiaoyu.EFCore.SoftDelete
{
    public static class ModelBuilderExtensions
    {
        public static void ApplySoftDeleteFilter(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .HasQueryFilter(CreateSoftDeleteFilter(entityType.ClrType));
                }
            }
        }

        private static LambdaExpression CreateSoftDeleteFilter(Type entityType)
        {
            var parameter = Expression.Parameter(entityType, "e");
            var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
            var body = Expression.Equal(property, Expression.Constant(false));
            var filter = Expression.Lambda(body, parameter);

            return filter;
        }
    }
}
