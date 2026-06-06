using ECommerce.Domain;
using ECommerce.Infra.Repository;
using MediatR;

namespace ECommerce.Application.UseCases;

internal record AddProductCommand(string Name, string Description, decimal Price) : IRequest<AddProductOutput>;

internal class AddProductHandler : IRequestHandler<AddProductCommand, AddProductOutput>
{
    private readonly IProductRepository _productRepository;

    public AddProductHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<AddProductOutput> Handle(AddProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            ProductId = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
        };
        await _productRepository.AddProduct(product);
        return new AddProductOutput(product.ProductId, product.Name, product.Description, product.Price);
    }
}

internal record AddProductOutput(Guid ProductId, string Name, string Description, decimal Price);
