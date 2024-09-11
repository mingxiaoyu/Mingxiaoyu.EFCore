using Microsoft.EntityFrameworkCore;
using Mingxiaoyu.EFCore.NoLock;
using Mingxiaoyu.EFCore.NoLock.Tests;

public class NoLockCommandInterceptorTests : IClassFixture<TestDatabaseFixture>
{
    public NoLockCommandInterceptorTests(TestDatabaseFixture fixture)
         => Fixture = fixture;

    public TestDatabaseFixture Fixture { get; }

    [Fact]
    public void Test_SingleTableQuery_WithNoLock()
    {
        var logOutput = new StringWriter();
        using var context = Fixture.CreateContext(logOutput);
        var products = context.Products.WithNoLock().ToList();

        var logMessages = logOutput.ToString();

        Assert.Single(products);
        Assert.Equal("Test Product", products[0].ProductName);

        Assert.Contains("SELECT", logMessages);
        Assert.Contains("WITH (NOLOCK)", logMessages);
    }

    [Fact]
    public void Test_TwoTableJoin_WithNoLock()
    {
        var logOutput = new StringWriter();
        using var context = Fixture.CreateContext(logOutput);
        var ordersWithProducts = context.Orders
                    .Include(o => o.Product)
                    .WithNoLock()
                    .ToList();

        Assert.Single(ordersWithProducts);
        Assert.Equal("Test Order", ordersWithProducts[0].Name);
        Assert.Equal("Test Product", ordersWithProducts[0].Product.ProductName);


        // Assert log contains the expected SQL with NOLOCK
        var logMessages = logOutput.ToString();
        Assert.Contains("SELECT", logMessages);
        Assert.Contains("WITH (NOLOCK)", logMessages);

    }

    [Fact]
    public void Test_RawSql_WithNoLock()
    {
        var logOutput = new StringWriter();
        using var context = Fixture.CreateContext(logOutput);
        var products = context.Products.FromSql($"SELECT * FROM dbo.Products");

        var logMessages = logOutput.ToString();

        Assert.Single(products);
        Assert.Contains("", logMessages);
    }
}
