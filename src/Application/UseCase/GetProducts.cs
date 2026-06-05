using e_commerce.Infra.Repository;

namespace e_commerce.Application.UseCase;

public class GetProducts
{
    private readonly IProductRepository _productRepository;

    public GetProducts(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<List<Output>> Execute()
    {
        var products = await _productRepository.GetProducts();
        return products.Select(p => new Output(p.ProductId, p.Name, p.Description, p.Price)).ToList();
    }

    public record Output(Guid ProductId, string Name, string Description, decimal Price);
}
