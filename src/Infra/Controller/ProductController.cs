using ECommerce.Application.UseCases;
using ECommerce.Infra.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Infra.Controllers;

internal class ProductController
{
    public ProductController(IHttpServer httpServer, IServiceProvider sp)
    {
        httpServer.Route<List<GetProductsOutput>>("get", "/products", async (@params) =>
        {
            await using AsyncServiceScope scope = sp.CreateAsyncScope();
            return await scope.ServiceProvider.GetRequiredService<GetProducts>().Execute();
        });
    }
}
