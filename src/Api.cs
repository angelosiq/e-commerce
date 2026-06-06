using System.Reflection;
using ECommerce.Infra.Controllers;
using ECommerce.Infra.Database;
using ECommerce.Infra.Http;
using ECommerce.Infra.Repository;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder();
builder.Services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("ecommerce"));
builder.Services.AddScoped<IProductRepository, ProductRepositoryDatabase>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
AspNetCoreAdapter httpServer = new(builder);
_ = new ProductController(httpServer, httpServer.App.Services);
await httpServer.Run();
