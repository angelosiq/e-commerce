using ECommerce.Application.UseCases;
using ECommerce.Infra.Http;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Infra.Controllers;

internal class ProductController
{
    public ProductController(IHttpServer httpServer, IServiceProvider sp)
    {
        httpServer.Route<List<GetProductsOutput>>("get", "/products", async (@params) =>
        {
            await using AsyncServiceScope scope = sp.CreateAsyncScope();
            return await scope.ServiceProvider.GetRequiredService<IMediator>().Send(new GetProductsQuery());
        });

        httpServer.Route<AddProductCommand, AddProductOutput>("post", "/products", async (@params, body) =>
        {
            await using AsyncServiceScope scope = sp.CreateAsyncScope();
            return await scope.ServiceProvider.GetRequiredService<IMediator>().Send(body);
        });
    }
}
