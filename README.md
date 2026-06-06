# e-commerce

A .NET 10 web API built with a clean architecture and a custom HTTP abstraction layer over ASP.NET Core.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Running

```bash
dotnet run
```

Swagger UI is available at `http://localhost:<port>/swagger`.

## Testing

```bash
dotnet test
```

## Project structure

```
src/
  Api.cs                        # Composition root — DI registration and startup
  Domain/
    Product.cs                  # Core entity, no framework dependencies
  Application/
    UseCases/
      GetProducts.cs            # Use case, depends only on domain interfaces
      AddProduct.cs             # Use case, depends only on domain interfaces
  Infra/
    Controller/
      ProductController.cs      # Registers routes via IHttpServer
    Http/
      HttpServer.cs             # IHttpServer interface + AspNetCoreAdapter
    Repository/
      ProductRepository.cs      # IProductRepository + EF Core implementation
    Database/
      AppDbContext.cs           # EF Core DbContext
tests/
  Unit/                         # Isolated unit tests (no I/O)
  Integration/                  # Endpoint-to-database tests via TestServer
```

## Architecture

The codebase follows a layered architecture where dependencies only point inward:

```
Domain  ←  Application  ←  Infra  ←  Api.cs
```

**`IHttpServer`** abstracts ASP.NET Core's routing from the controllers. `ProductController` registers routes in its constructor via `IHttpServer.Route(...)`, remaining unaware of the underlying framework.

**Dependency injection** is configured in `Api.cs`. Lifetimes:

| Service | Lifetime |
|---|---|
| `AppDbContext` | Scoped (one per request, via `AddDbContext`) |
| `IProductRepository` / `ProductRepositoryDatabase` | Scoped |
| MediatR handlers (`GetProductsHandler`, `AddProductHandler`, …) | Transient (via MediatR assembly scanning) |

Each route handler creates an `AsyncServiceScope` so every request gets fresh scoped instances:

```csharp
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
```

## Adding a new endpoint

1. Add the domain entity to `src/Domain/` if needed.
2. Add a use case class to `src/Application/UseCases/` — implement `IRequestHandler<TCommand, TResponse>`. MediatR picks it up automatically via assembly scanning; no registration in `Api.cs` is required.
3. Add the route in `src/Infra/Controller/ProductController.cs` (or a new controller class) using `httpServer.Route(...)`.
4. If you created a new controller class, instantiate it in `Api.cs`: `_ = new MyController(httpServer, httpServer.App.Services);`

## Git hooks

Hooks are managed by [Husky.Net](https://alirezanet.github.io/Husky.Net/) and install automatically on `dotnet restore`. Set `HUSKY=0` to skip installation (e.g. in CI).

| Hook | What it runs |
|---|---|
| `pre-commit` | Full build (warnings as errors) + all tests |
| `commit-msg` | Conventional Commits format check |
| `pre-push` | Full build (warnings as errors) + all tests |

Commit messages must follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <description>

feat: add product search
fix(cart): prevent duplicate items
chore!: drop support for .NET 8
```

Valid types: `build`, `chore`, `ci`, `docs`, `feat`, `fix`, `perf`, `refactor`, `revert`, `style`, `test`.

## Code quality

- .NET SDK analyzers (`AnalysisMode=All`) and [Roslynator](https://github.com/dotnet/roslynator) run on every build.
- Style rules from `.editorconfig` are enforced at build time (`EnforceCodeStyleInBuild=true`).
- Warnings are treated as errors in CI hooks.
