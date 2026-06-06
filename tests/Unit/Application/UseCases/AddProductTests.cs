using ECommerce.Application.UseCases;
using ECommerce.Infra.Repository;

namespace Unit.Application.UseCases;

[TestClass]
public class AddProductTests
{
    [TestMethod]
    public async Task Execute_WithValidInput_ReturnsProductWithGeneratedId()
    {
        var repository = new ProductRepositoryMemory();
        var handler = new AddProductHandler(repository);
        var result = await handler.Handle(new AddProductCommand("Laptop", "Gaming laptop", 2999.99m), default);
        Assert.AreNotEqual(Guid.Empty, result.ProductId);
    }

    [TestMethod]
    public async Task Execute_WithValidInput_StoresProductInRepository()
    {
        var repository = new ProductRepositoryMemory();
        var handler = new AddProductHandler(repository);
        await handler.Handle(new AddProductCommand("Laptop", "Gaming laptop", 2999.99m), default);
        var products = await repository.GetProducts();
        Assert.HasCount(1, products);
    }

    [TestMethod]
    public async Task Execute_MapsInputFieldsCorrectly()
    {
        var repository = new ProductRepositoryMemory();
        var handler = new AddProductHandler(repository);
        var result = await handler.Handle(new AddProductCommand("Keyboard", "Mechanical keyboard", 149.99m), default);
        Assert.AreEqual("Keyboard", result.Name);
        Assert.AreEqual("Mechanical keyboard", result.Description);
        Assert.AreEqual(149.99m, result.Price);
    }

    [TestMethod]
    public async Task Execute_CalledTwice_GeneratesDistinctIds()
    {
        var repository = new ProductRepositoryMemory();
        var handler = new AddProductHandler(repository);
        var first = await handler.Handle(new AddProductCommand("Laptop", "Gaming laptop", 2999.99m), default);
        var second = await handler.Handle(new AddProductCommand("Mouse", "Wireless mouse", 49.99m), default);
        Assert.AreNotEqual(first.ProductId, second.ProductId);
    }
}
