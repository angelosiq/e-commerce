using System.Net;
using System.Net.Http.Json;
using ECommerce.Application.UseCases;
using ECommerce.Infra.Controllers;
using ECommerce.Infra.Database;
using ECommerce.Infra.Http;
using ECommerce.Infra.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Integration.Products;

[TestClass]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001", Justification = "Disposed in [TestCleanup]")]
public class AddProductIntegrationTests
{
    private AppDbContext _dbContext = null!;
    private HttpClient _client = null!;
    private AspNetCoreAdapter _adapter = null!;

    private sealed record ProductInput(string Name, string Description, decimal Price);
    private sealed record ProductOutput(Guid ProductId, string Name, string Description, decimal Price);

    [TestInitialize]
    public void Initialize()
    {
        _dbContext = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options
        );

        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton(_dbContext);
        builder.Services.AddScoped<IProductRepository, ProductRepositoryDatabase>();
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<GetProductsHandler>());

        _adapter = new AspNetCoreAdapter(builder);
        _ = new ProductController(_adapter, _adapter.App.Services);

        _adapter.App.StartAsync().GetAwaiter().GetResult();
        _client = _adapter.App.GetTestServer().CreateClient();
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        await _adapter.App.StopAsync();
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.DisposeAsync();
        _client.Dispose();
    }

    [TestMethod]
    public async Task AddProduct_WithValidBody_Returns200WithCreatedProduct()
    {
        var body = new ProductInput("Laptop", "Gaming laptop", 2999.99m);

        var response = await _client.PostAsJsonAsync("/products", body);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ProductOutput>();
        Assert.IsNotNull(result);
        Assert.AreNotEqual(Guid.Empty, result.ProductId);
        Assert.AreEqual(body.Name, result.Name);
        Assert.AreEqual(body.Description, result.Description);
        Assert.AreEqual(body.Price, result.Price);
    }

    [TestMethod]
    public async Task AddProduct_WithValidBody_ProductAppearsInGetProducts()
    {
        var body = new ProductInput("Mouse", "Wireless mouse", 49.99m);
        await _client.PostAsJsonAsync("/products", body);

        var response = await _client.GetAsync("/products");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var products = await response.Content.ReadFromJsonAsync<List<ProductOutput>>();
        Assert.HasCount(1, products!);
        Assert.AreEqual(body.Name, products![0].Name);
    }

    [TestMethod]
    public async Task AddProduct_CalledTwice_ReturnsDifferentIds()
    {
        var first = await _client.PostAsJsonAsync("/products", new ProductInput("Laptop", "Gaming laptop", 2999.99m));
        var second = await _client.PostAsJsonAsync("/products", new ProductInput("Mouse", "Wireless mouse", 49.99m));

        var firstResult = await first.Content.ReadFromJsonAsync<ProductOutput>();
        var secondResult = await second.Content.ReadFromJsonAsync<ProductOutput>();

        Assert.AreNotEqual(firstResult!.ProductId, secondResult!.ProductId);
    }

    [TestMethod]
    public async Task AddProduct_MultipleProducts_AllAppearInGetProducts()
    {
        await _client.PostAsJsonAsync("/products", new ProductInput("Laptop", "Gaming laptop", 2999.99m));
        await _client.PostAsJsonAsync("/products", new ProductInput("Mouse", "Wireless mouse", 49.99m));
        await _client.PostAsJsonAsync("/products", new ProductInput("Keyboard", "Mechanical keyboard", 149.99m));

        var response = await _client.GetAsync("/products");
        var products = await response.Content.ReadFromJsonAsync<List<ProductOutput>>();

        Assert.HasCount(3, products!);
    }
}
