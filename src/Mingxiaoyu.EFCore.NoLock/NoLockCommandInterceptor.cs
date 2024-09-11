using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public class NoLockCommandInterceptor : DbCommandInterceptor
{
    private const string NoLockTag = "NOLOCK"; // Tag to detect in the SQL

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        if (IsTaggedWithNoLock(command))
        {
            ApplyNoLock(command);
        }
        return base.ReaderExecuting(command, eventData, result);
    }

    public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        if (IsTaggedWithNoLock(command))
        {
            ApplyNoLock(command);
        }
        return await base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    private bool IsTaggedWithNoLock(DbCommand command)
    {
        // Check if the command contains the NOLOCK tag
        return command.CommandText.Contains($"-- {NoLockTag}", System.StringComparison.OrdinalIgnoreCase);
    }

    
    public void ApplyNoLock(DbCommand command)
    {
        var cleanedCommandText = RemoveSqlComments(command.CommandText);
        if (cleanedCommandText.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
        {
            var regex = new Regex(@"(FROM\s+\[\w+\]|\s+INNER\s+JOIN\s+\[\w+\]|\s+LEFT\s+JOIN\s+\[\w+\])\s+AS\s+\[\w+\]", RegexOptions.IgnoreCase);
            var result= regex.Replace(cleanedCommandText, match => match.Value + " WITH (NOLOCK)");
            command.CommandText = result;
        }
    }
    private string RemoveSqlComments(string sql)
    {
        // Remove single-line comments
        var lines = sql.Split('\n');
        var filteredLines = lines.Where(line => !line.TrimStart().StartsWith("--", StringComparison.OrdinalIgnoreCase)).ToArray();
        return string.Join("\n", filteredLines).Trim();
    }
}
