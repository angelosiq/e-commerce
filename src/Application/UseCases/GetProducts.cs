using ECommerce.Domain;
using ECommerce.Infra.Repository;

namespace ECommerce.Application.UseCases;

internal class GetProducts
{
    private readonly IProductRepository _productRepository;

    public GetProducts(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<List<GetProductsOutput>> Execute()
    {
        List<Product> products = await _productRepository.GetProducts();
        return products.Select(p => new GetProductsOutput(p.ProductId, p.Name, p.Description, p.Price)).ToList();
    }
}

internal record GetProductsOutput(Guid ProductId, string Name, string Description, decimal Price);
