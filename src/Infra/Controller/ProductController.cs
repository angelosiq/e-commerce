using e_commerce.Application.UseCase;
using e_commerce.Infra.Http;

namespace e_commerce.Infra.Controllers;

public class ProductController
{
    public ProductController(IHttpServer httpServer, GetProducts getProducts)
    {
        httpServer.Route<List<GetProducts.Output>>("get", "/products", async (@params) =>
        {
            return await getProducts.Execute();
        });
    }
}