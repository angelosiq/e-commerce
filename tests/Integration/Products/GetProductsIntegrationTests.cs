using System.Net;
using System.Net.Http.Json;
using ECommerce.Application.UseCases;
using ECommerce.Domain;
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
public class GetProductsIntegrationTests
{
    private AppDbContext _dbContext = null!;
    private HttpClient _client = null!;
    private AspNetCoreAdapter _adapter = null!;

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
        builder.Services.AddScoped<GetProducts>();

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
    public async Task GetProducts_WhenDatabaseIsEmpty_Returns200WithEmptyList()
    {
        var response = await _client.GetAsync("/products");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<ProductOutput>>();
        Assert.IsNotNull(body);
        Assert.IsEmpty(body);
    }

    [TestMethod]
    public async Task GetProducts_WhenOneProductExists_Returns200WithThatProduct()
    {
        var product = new Product
        {
            ProductId = Guid.NewGuid(),
            Name = "Laptop",
            Description = "Gaming laptop",
            Price = 2999.99m,
        };
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        var response = await _client.GetAsync("/products");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<ProductOutput>>();
        Assert.HasCount(1, body!);
        Assert.AreEqual(product.ProductId, body![0].ProductId);
        Assert.AreEqual(product.Name, body[0].Name);
        Assert.AreEqual(product.Description, body[0].Description);
        Assert.AreEqual(product.Price, body[0].Price);
    }

    [TestMethod]
    public async Task GetProducts_WhenMultipleProductsExist_ReturnsAllProducts()
    {
        _dbContext.Products.AddRange(
            new Product { ProductId = Guid.NewGuid(), Name = "Laptop", Description = "Gaming laptop", Price = 2999.99m },
            new Product { ProductId = Guid.NewGuid(), Name = "Mouse", Description = "Wireless mouse", Price = 49.99m },
            new Product { ProductId = Guid.NewGuid(), Name = "Keyboard", Description = "Mechanical keyboard", Price = 149.99m }
        );
        await _dbContext.SaveChangesAsync();

        var response = await _client.GetAsync("/products");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<ProductOutput>>();
        Assert.HasCount(3, body!);
    }
}
