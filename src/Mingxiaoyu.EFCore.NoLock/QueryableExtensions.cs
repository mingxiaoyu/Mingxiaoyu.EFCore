using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mingxiaoyu.EFCore.NoLock
{
    public static class QueryableExtensions
    {
        public static IQueryable<TEntity> WithNoLock<TEntity>(this IQueryable<TEntity> query)
            where TEntity : class
        {
            return query.TagWith("NOLOCK");
        }
    }
}
