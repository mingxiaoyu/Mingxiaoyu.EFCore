namespace Mingxiaoyu.EFCore.UoW.Tests
{
    public class ProductRepository : Repository<Product, int>
    {

        public ProductRepository(UowTestDbContext context) : base(context)
        {
        }

        public Product GetProductByName(string name)
        {
            return DbSet.First(x => x.ProductName == name);
        }
    }
}
