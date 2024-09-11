using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mingxiaoyu.EFCore.UoW.Tests
{
    public class UowTestDbContext : DbContext, IDbContext
    {
        public UowTestDbContext(DbContextOptions<UowTestDbContext> options) :
            base(options)
        {
        }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }

    }
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
        public string ProductName { get; set; }
        public ICollection<Order> Orders { get; set; }
    }

    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Name { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; }
    }
}
