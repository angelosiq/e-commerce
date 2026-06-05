using e_commerce.Domain;
using e_commerce.Infra.Database;
using Microsoft.EntityFrameworkCore;

namespace e_commerce.Infra.Repository;

public interface IProductRepository
{
    Task<List<Product>> GetProducts();
}

public class ProductRepositoryDatabase : IProductRepository
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
}

public class ProductRepositoryMemory : IProductRepository
{
    private readonly List<Product> _products = [];

    public Task<List<Product>> GetProducts() => Task.FromResult(_products);
}
