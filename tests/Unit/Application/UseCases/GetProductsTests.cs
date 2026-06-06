using ECommerce.Application.UseCase;
using ECommerce.Domain;
using ECommerce.Infra.Repository;

namespace Unit.Application.UseCases;

[TestClass]
public class GetProductsTests
{
    [TestMethod]
    public async Task Execute_WhenRepositoryIsEmpty_ReturnsEmptyList()
    {
        var repository = new ProductRepositoryMemory();
        var useCase = new GetProducts(repository);
        var result = await useCase.Execute();
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public async Task Execute_WhenRepositoryHasProducts_ReturnsAllProducts()
    {
        var repository = new ProductRepositoryMemory();
        repository.Add(new Product
        {
            ProductId = Guid.NewGuid(),
            Name = "Laptop",
            Description = "Gaming laptop",
            Price = 2999.99m
        });
        repository.Add(new Product
        {
            ProductId = Guid.NewGuid(),
            Name = "Mouse",
            Description = "Wireless mouse",
            Price = 49.99m
        });
        var useCase = new GetProducts(repository);
        var result = await useCase.Execute();
        Assert.HasCount(2, result);
    }

    [TestMethod]
    public async Task Execute_MapsProductFieldsCorrectly()
    {
        var repository = new ProductRepositoryMemory();
        var expectedId = Guid.NewGuid();
        repository.Add(new Product
        {
            ProductId = expectedId,
            Name = "Keyboard",
            Description = "Mechanical keyboard",
            Price = 149.99m
        });
        var useCase = new GetProducts(repository);
        var result = await useCase.Execute();
        Assert.HasCount(1, result);
        var output = result[0];
        Assert.AreEqual(expectedId, output.ProductId);
        Assert.AreEqual("Keyboard", output.Name);
        Assert.AreEqual("Mechanical keyboard", output.Description);
        Assert.AreEqual(149.99m, output.Price);
    }
}
