using ECommerce.Domain;
using ECommerce.Infra.Database;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infra.Repository;

internal interface IProductRepository
{
    public Task<List<Product>> GetProducts();
    public Task AddProduct(Product product);
}

internal class ProductRepositoryDatabase : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepositoryDatabase(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetProducts()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task AddProduct(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
    }
}

internal class ProductRepositoryMemory : IProductRepository
{
    private readonly List<Product> _products = [];

    public void Add(Product product) => _products.Add(product);

    public Task<List<Product>> GetProducts() => Task.FromResult(_products);

    public Task AddProduct(Product product)
    {
        _products.Add(product);
        return Task.CompletedTask;
    }
}
