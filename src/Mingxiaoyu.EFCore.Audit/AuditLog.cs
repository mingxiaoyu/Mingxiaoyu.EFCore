using System;

namespace Mingxiaoyu.EFCore.Audit
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        public required string TableName { get; set; }
        public required string RecordID { get; set; }
        public required string Operation { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public required string ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }
    }

}
