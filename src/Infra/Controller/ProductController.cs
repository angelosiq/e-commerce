using ECommerce.Application.UseCases;
using ECommerce.Infra.Http;

namespace ECommerce.Infra.Controllers;

internal class ProductController
{
    public ProductController(IHttpServer httpServer, GetProducts getProducts)
    {
        httpServer.Route<List<GetProductsOutput>>("get", "/products", async (@params) =>
        {
            return await getProducts.Execute();
        });
    }
}
