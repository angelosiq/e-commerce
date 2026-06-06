using ECommerce.Domain;
using ECommerce.Infra.Database;
using Microsoft.EntityFrameworkCore;

namespace Unit.Infra.Database;

[TestClass]
public class AppDbContextTests
{
    private static AppDbContext CreateContext(string dbName) =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options);

    [TestMethod]
    public void Products_DbSet_IsNotNull()
    {
        using var context = CreateContext(nameof(Products_DbSet_IsNotNull));
        Assert.IsNotNull(context.Products);
    }

    [TestMethod]
    public void OnModelCreating_ConfiguresProductId_AsPrimaryKey()
    {
        using var context = CreateContext(nameof(OnModelCreating_ConfiguresProductId_AsPrimaryKey));
        var entityType = context.Model.FindEntityType(typeof(Product));
        Assert.IsNotNull(entityType);
        var primaryKey = entityType.FindPrimaryKey();
        Assert.IsNotNull(primaryKey);
        Assert.HasCount(1, primaryKey.Properties);
        Assert.AreEqual(nameof(Product.ProductId), primaryKey.Properties[0].Name);
    }

    [TestMethod]
    public async Task SaveChangesAsync_AddsProduct_CanBeRetrieved()
    {
        using var context = CreateContext(nameof(SaveChangesAsync_AddsProduct_CanBeRetrieved));
        var product = new Product { ProductId = Guid.NewGuid(), Name = "Laptop", Description = "Gaming laptop", Price = 2999.99m };
        context.Products.Add(product);
        await context.SaveChangesAsync();
        var saved = await context.Products.ToListAsync();
        Assert.HasCount(1, saved);
    }

    [TestMethod]
    public async Task SaveChangesAsync_AddsMultipleProducts_AllPersisted()
    {
        using var context = CreateContext(nameof(SaveChangesAsync_AddsMultipleProducts_AllPersisted));
        context.Products.AddRange(
            new Product { ProductId = Guid.NewGuid(), Name = "Mouse", Description = "Wireless mouse", Price = 49.99m },
            new Product { ProductId = Guid.NewGuid(), Name = "Keyboard", Description = "Mechanical keyboard", Price = 149.99m }
        );
        await context.SaveChangesAsync();
        var saved = await context.Products.ToListAsync();
        Assert.HasCount(2, saved);
    }

    [TestMethod]
    public async Task SaveChangesAsync_Product_FieldsAreCorrect()
    {
        using var context = CreateContext(nameof(SaveChangesAsync_Product_FieldsAreCorrect));
        var expectedId = Guid.NewGuid();
        context.Products.Add(new Product { ProductId = expectedId, Name = "Monitor", Description = "4K monitor", Price = 799.99m });
        await context.SaveChangesAsync();
        var product = await context.Products.FirstAsync();
        Assert.AreEqual(expectedId, product.ProductId);
        Assert.AreEqual("Monitor", product.Name);
        Assert.AreEqual("4K monitor", product.Description);
        Assert.AreEqual(799.99m, product.Price);
    }

    [TestMethod]
    public async Task Products_InitialState_IsEmpty()
    {
        using var context = CreateContext(nameof(Products_InitialState_IsEmpty));
        var result = await context.Products.ToListAsync();
        Assert.IsEmpty(result);
    }
}
