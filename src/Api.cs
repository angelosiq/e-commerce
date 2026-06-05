using e_commerce.Application.UseCase;
using e_commerce.Infra.Controllers;
using e_commerce.Infra.Database;
using e_commerce.Infra.Http;
using e_commerce.Infra.Repository;
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
