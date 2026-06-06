using ECommerce.Domain;
using ECommerce.Infra.Repository;
using MediatR;

namespace ECommerce.Application.UseCases;

internal record GetProductsQuery : IRequest<List<GetProductsOutput>>;

internal class GetProductsHandler : IRequestHandler<GetProductsQuery, List<GetProductsOutput>>
{
    private readonly IProductRepository _productRepository;

    public GetProductsHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<List<GetProductsOutput>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        List<Product> products = await _productRepository.GetProducts();
        return products.Select(p => new GetProductsOutput(p.ProductId, p.Name, p.Description, p.Price)).ToList();
    }
}

internal record GetProductsOutput(Guid ProductId, string Name, string Description, decimal Price);
