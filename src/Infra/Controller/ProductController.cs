using e_commerce.Infra.Http;

namespace e_commerce.Infra.Controllers;

public class ProductController
{
    public ProductController(IHttpServer httpServer)
    {
        httpServer.Route<string>("get", "/products", async (@params) => await Task.FromResult("funcionou"));
    }
}