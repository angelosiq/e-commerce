using ECommerce.Domain;
using ECommerce.Infra.Database;
using ECommerce.Infra.Repository;
using Microsoft.EntityFrameworkCore;

namespace Unit.Infra.Repository;

[TestClass]
public class ProductRepositoryMemoryTests
{
    [TestMethod]
    public async Task GetProducts_WhenEmpty_ReturnsEmptyList()
    {
        var repository = new ProductRepositoryMemory();
        var result = await repository.GetProducts();
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public async Task GetProducts_AfterAddingOneProduct_ReturnsOneProduct()
    {
        var repository = new ProductRepositoryMemory();
        repository.Add(new Product { ProductId = Guid.NewGuid(), Name = "Laptop", Description = "Gaming laptop", Price = 2999.99m });
        var result = await repository.GetProducts();
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetProducts_AfterAddingMultipleProducts_ReturnsAll()
    {
        var repository = new ProductRepositoryMemory();
        repository.Add(new Product { ProductId = Guid.NewGuid(), Name = "Laptop", Description = "Gaming laptop", Price = 2999.99m });
        repository.Add(new Product { ProductId = Guid.NewGuid(), Name = "Mouse", Description = "Wireless mouse", Price = 49.99m });
        repository.Add(new Product { ProductId = Guid.NewGuid(), Name = "Keyboard", Description = "Mechanical keyboard", Price = 149.99m });
        var result = await repository.GetProducts();
        Assert.HasCount(3, result);
    }

    [TestMethod]
    public async Task GetProducts_ReturnsProductWithCorrectFields()
    {
        var repository = new ProductRepositoryMemory();
        var expectedId = Guid.NewGuid();
        repository.Add(new Product { ProductId = expectedId, Name = "Monitor", Description = "4K monitor", Price = 799.99m });
        var result = await repository.GetProducts();
        var product = result[0];
        Assert.AreEqual(expectedId, product.ProductId);
        Assert.AreEqual("Monitor", product.Name);
        Assert.AreEqual("4K monitor", product.Description);
        Assert.AreEqual(799.99m, product.Price);
    }

    [TestMethod]
    public async Task GetProducts_CalledTwice_ReturnsSameList()
    {
        var repository = new ProductRepositoryMemory();
        repository.Add(new Product { ProductId = Guid.NewGuid(), Name = "Laptop", Description = "Gaming laptop", Price = 2999.99m });
        var first = await repository.GetProducts();
        var second = await repository.GetProducts();
        Assert.HasCount(first.Count, second);
    }
}

[TestClass]
public class ProductRepositoryDatabaseTests
{
    private static AppDbContext CreateContext(string dbName) =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options);

    [TestMethod]
    public async Task GetProducts_WhenDatabaseIsEmpty_ReturnsEmptyList()
    {
        using var context = CreateContext(nameof(GetProducts_WhenDatabaseIsEmpty_ReturnsEmptyList));
        var repository = new ProductRepositoryDatabase(context);
        var result = await repository.GetProducts();
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public async Task GetProducts_WhenDatabaseHasOneProduct_ReturnsOneProduct()
    {
        using var context = CreateContext(nameof(GetProducts_WhenDatabaseHasOneProduct_ReturnsOneProduct));
        context.Products.Add(new Product { ProductId = Guid.NewGuid(), Name = "Laptop", Description = "Gaming laptop", Price = 2999.99m });
        await context.SaveChangesAsync();
        var repository = new ProductRepositoryDatabase(context);
        var result = await repository.GetProducts();
        Assert.HasCount(1, result);
    }

    [TestMethod]
    public async Task GetProducts_WhenDatabaseHasMultipleProducts_ReturnsAll()
    {
        using var context = CreateContext(nameof(GetProducts_WhenDatabaseHasMultipleProducts_ReturnsAll));
        context.Products.AddRange(
            new Product { ProductId = Guid.NewGuid(), Name = "Laptop", Description = "Gaming laptop", Price = 2999.99m },
            new Product { ProductId = Guid.NewGuid(), Name = "Mouse", Description = "Wireless mouse", Price = 49.99m }
        );
        await context.SaveChangesAsync();
        var repository = new ProductRepositoryDatabase(context);
        var result = await repository.GetProducts();
        Assert.HasCount(2, result);
    }

    [TestMethod]
    public async Task GetProducts_ReturnsProductWithCorrectFields()
    {
        using var context = CreateContext(nameof(GetProducts_ReturnsProductWithCorrectFields));
        var expectedId = Guid.NewGuid();
        context.Products.Add(new Product { ProductId = expectedId, Name = "Monitor", Description = "4K monitor", Price = 799.99m });
        await context.SaveChangesAsync();
        var repository = new ProductRepositoryDatabase(context);
        var result = await repository.GetProducts();
        var product = result[0];
        Assert.AreEqual(expectedId, product.ProductId);
        Assert.AreEqual("Monitor", product.Name);
        Assert.AreEqual("4K monitor", product.Description);
        Assert.AreEqual(799.99m, product.Price);
    }
}
