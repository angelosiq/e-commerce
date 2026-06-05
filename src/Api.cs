using e_commerce.Infra.Http;
using e_commerce.Infra.Controllers;

var httpServer = new AspNetCoreAdapter();
_ = new ProductController(httpServer);
await httpServer.Run();