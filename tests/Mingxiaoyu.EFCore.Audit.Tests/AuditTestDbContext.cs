using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mingxiaoyu.EFCore.Audit.Tests
{
    public class AuditTestDbContext : DbContext
    {
        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<UnAudit> UnAudits { get; set; }

        public DbSet<MorePrimaryKey> MorePrimaryKeys { get; set; }

        public AuditTestDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MorePrimaryKey>().HasKey(e => new { e.PrimaryKeyOne, e.PrimaryKeyTwo });
        }
    }

    [Auditable]
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public required string ProductName { get; set; }
        public decimal Price { get; set; }
    }

    public class UnAudit
    {
        public Guid Id { get; set; }
        public string SomeProperty { get; set; }
    }

    [Auditable]
    public class MorePrimaryKey
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid PrimaryKeyOne { get; set; }
        [Key,DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid PrimaryKeyTwo { get; set; }
        public string SomeProperty { get; set; }
        public decimal decimalType { get; set; }
        public int IntType { get; set; }
        public bool BoolType { get; set; }
        public DateTime DateTimeType { get; set; }
        public double DoubleType { get; set; }
    }
}
