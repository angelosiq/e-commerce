using System.Text.Json;
using System.Text.RegularExpressions;

namespace ECommerce.Infra.Http;

internal interface IHttpServer
{
    public void Route<TResponse>(string method, string url,
        Func<IReadOnlyDictionary<string, string?>, Task<TResponse>> handler);

    public void Route<TRequest, TResponse>(string method, string url,
        Func<IReadOnlyDictionary<string, string?>, TRequest, Task<TResponse>> handler)
        where TRequest : notnull;

    public Task Run();
}

internal class AspNetCoreAdapter : IHttpServer
{
    internal WebApplication App { get; }
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public AspNetCoreAdapter() : this(WebApplication.CreateBuilder()) { }

    internal AspNetCoreAdapter(WebApplicationBuilder builder)
    {
        builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
            p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApiDocument();
        App = builder.Build();
        App.UseOpenApi();
        App.UseSwaggerUi();
        App.UseHttpsRedirection();
        App.UseCors();
    }

    public void Route<TRequest, TResponse>(
        string method, string url,
        Func<IReadOnlyDictionary<string, string?>, TRequest, Task<TResponse>> handler)
        where TRequest : notnull
    {
        string pattern = Regex.Replace(url, @":(\w+)", "{$1}");
        Delegate routeHandler = async (HttpContext ctx) =>
        {
            try
            {
                Dictionary<string, string?> @params = ctx.Request.RouteValues
                    .ToDictionary(kv => kv.Key, kv => kv.Value?.ToString());
                TRequest input = (await ctx.Request.ReadFromJsonAsync<TRequest>(Json))!;
                TResponse output = await handler(@params, input);
                await ctx.Response.WriteAsJsonAsync(output, Json);
            }
            catch (Exception e)
            {
                ctx.Response.StatusCode = 422;
                await ctx.Response.WriteAsJsonAsync(new { message = e.Message }, Json);
            }
        };
        App.MapMethods(pattern, new[] { method.ToUpperInvariant() }, routeHandler)
        .Accepts<TRequest>("application/json")
        .Produces<TResponse>()
        .Produces(422);
    }

    public void Route<TResponse>(
        string method, string url,
        Func<IReadOnlyDictionary<string, string?>, Task<TResponse>> handler)
    {
        string pattern = Regex.Replace(url, @":(\w+)", "{$1}");
        Delegate routeHandler = async (HttpContext ctx) =>
        {
            try
            {
                Dictionary<string, string?> @params = ctx.Request.RouteValues
                    .ToDictionary(kv => kv.Key, kv => kv.Value?.ToString());
                TResponse output = await handler(@params);
                await ctx.Response.WriteAsJsonAsync(output, Json);
            }
            catch (Exception e)
            {
                ctx.Response.StatusCode = 422;
                await ctx.Response.WriteAsJsonAsync(new { message = e.Message }, Json);
            }
        };
        App.MapMethods(pattern, new[] { method.ToUpperInvariant() }, routeHandler)
        .Produces<TResponse>()
        .Produces(422);
    }

    public async Task Run() => await App.RunAsync();
}
