using System.Text.Json;
using System.Text.RegularExpressions;

namespace e_commerce.Infra.Http;

public interface IHttpServer
{
    void Route<TResponse>(string method, string url,
        Func<IReadOnlyDictionary<string, string?>, Task<TResponse>> handler);

    void Route<TRequest, TResponse>(string method, string url,
        Func<IReadOnlyDictionary<string, string?>, TRequest, Task<TResponse>> handler)
        where TRequest : notnull;

    Task Run();
}

public class AspNetCoreAdapter : IHttpServer
{
    private readonly WebApplication _app;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public AspNetCoreAdapter()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
            p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApiDocument();
        _app = builder.Build();
        _app.UseOpenApi();
        _app.UseSwaggerUi();
        _app.UseHttpsRedirection();
        _app.UseCors();
    }

    public void Route<TRequest, TResponse>(
        string method, string url,
        Func<IReadOnlyDictionary<string, string?>, TRequest, Task<TResponse>> handler)
        where TRequest : notnull
    {
        var pattern = Regex.Replace(url, @":(\w+)", "{$1}");
        Delegate routeHandler = async (HttpContext ctx) =>
        {
            try
            {
                var @params = ctx.Request.RouteValues
                    .ToDictionary(kv => kv.Key, kv => kv.Value?.ToString());
                var input = (await ctx.Request.ReadFromJsonAsync<TRequest>(Json))!;
                var output = await handler(@params, input);
                await ctx.Response.WriteAsJsonAsync(output, Json);
            }
            catch (Exception e)
            {
                ctx.Response.StatusCode = 422;
                await ctx.Response.WriteAsJsonAsync(new { message = e.Message }, Json);
            }
        };
        _app.MapMethods(pattern, new[] { method.ToUpperInvariant() }, routeHandler)
        .Accepts<TRequest>("application/json")
        .Produces<TResponse>()
        .Produces(422);
    }

    public void Route<TResponse>(
        string method, string url,
        Func<IReadOnlyDictionary<string, string?>, Task<TResponse>> handler)
    {
        var pattern = Regex.Replace(url, @":(\w+)", "{$1}");
        Delegate routeHandler = async (HttpContext ctx) =>
        {
            try
            {
                var @params = ctx.Request.RouteValues
                    .ToDictionary(kv => kv.Key, kv => kv.Value?.ToString());
                var output = await handler(@params);
                await ctx.Response.WriteAsJsonAsync(output, Json);
            }
            catch (Exception e)
            {
                ctx.Response.StatusCode = 422;
                await ctx.Response.WriteAsJsonAsync(new { message = e.Message }, Json);
            }
        };
        _app.MapMethods(pattern, new[] { method.ToUpperInvariant() }, routeHandler)
        .Produces<TResponse>()
        .Produces(422);
    }

    public async Task Run() => await _app.RunAsync();
}
