using Microsoft.EntityFrameworkCore;

namespace Mingxiaoyu.EFCore.NoLock
{
    public static class SqlServerDbContextOptionsExtensions
    {
        public static DbContextOptionsBuilder<TContext> EnableSqlServerWithNoLock<TContext>(
            this DbContextOptionsBuilder<TContext> optionsBuilder) where TContext : DbContext
        {
            return optionsBuilder
                .AddInterceptors(new NoLockCommandInterceptor());
        }
    }
}
