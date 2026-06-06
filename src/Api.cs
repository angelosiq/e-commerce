using ECommerce.Application.UseCases;
using ECommerce.Infra.Controllers;
using ECommerce.Infra.Database;
using ECommerce.Infra.Http;
using ECommerce.Infra.Repository;
using Microsoft.EntityFrameworkCore;

var dbContext = new AppDbContext(
    new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase("ecommerce")
        .Options
);
var productRepository = new ProductRepositoryDatabase(dbContext);
var getProducts = new GetProducts(productRepository);
var httpServer = new AspNetCoreAdapter();
_ = new ProductController(httpServer, getProducts);
await httpServer.Run();
