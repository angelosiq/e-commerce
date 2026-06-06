using System.Net;
using System.Net.Http.Json;
using ECommerce.Infra.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;

namespace Unit.Infra.Http;

[TestClass]
public class HttpServerTests
{
    private sealed record ErrorBody(string Message);
    private sealed record InputDto(string Name);
    private sealed record OutputDto(string Name);

    private static (AspNetCoreAdapter Adapter, HttpClient Client) CreateTestServer(Action<AspNetCoreAdapter> configure)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        AspNetCoreAdapter adapter = new(builder);
        configure(adapter);
        adapter.App.StartAsync().GetAwaiter().GetResult();
        return (adapter, adapter.App.GetTestServer().CreateClient());
    }

    [TestMethod]
    public async Task Route_NoBody_WhenHandlerSucceeds_Returns200WithJsonResponse()
    {
        var (adapter, client) = CreateTestServer(s =>
            s.Route<string>("get", "/test", _ => Task.FromResult("hello")));
        try
        {
            var response = await client.GetAsync("/test");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<string>();
            Assert.AreEqual("hello", body);
        }
        finally { await adapter.App.StopAsync(); }
    }

    [TestMethod]
    public async Task Route_NoBody_WhenHandlerThrows_Returns422WithErrorMessage()
    {
        var (adapter, client) = CreateTestServer(s =>
            s.Route<string>("get", "/test", _ => throw new InvalidOperationException("oops")));
        try
        {
            var response = await client.GetAsync("/test");
            Assert.AreEqual((HttpStatusCode)422, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<ErrorBody>();
            Assert.AreEqual("oops", body!.Message);
        }
        finally { await adapter.App.StopAsync(); }
    }

    [TestMethod]
    public async Task Route_NoBody_WithColonUrlParam_PassesParamToHandler()
    {
        string? captured = null;
        var (adapter, client) = CreateTestServer(s =>
            s.Route<string>("get", "/items/:id", @params =>
            {
                captured = @params["id"];
                return Task.FromResult("ok");
            }));
        try
        {
            await client.GetAsync("/items/abc123");
            Assert.AreEqual("abc123", captured);
        }
        finally { await adapter.App.StopAsync(); }
    }

    [TestMethod]
    public async Task Route_NoBody_WithMultipleColonParams_PassesAllParamsToHandler()
    {
        string? capturedCategory = null;
        string? capturedId = null;
        var (adapter, client) = CreateTestServer(s =>
            s.Route<string>("get", "/categories/:category/items/:id", @params =>
            {
                capturedCategory = @params["category"];
                capturedId = @params["id"];
                return Task.FromResult("ok");
            }));
        try
        {
            await client.GetAsync("/categories/electronics/items/42");
            Assert.AreEqual("electronics", capturedCategory);
            Assert.AreEqual("42", capturedId);
        }
        finally { await adapter.App.StopAsync(); }
    }

    [TestMethod]
    public async Task Route_WithBody_WhenHandlerSucceeds_DeserializesBodyAndReturns200()
    {
        var (adapter, client) = CreateTestServer(s =>
            s.Route<InputDto, OutputDto>("post", "/test",
                (_, input) => Task.FromResult(new OutputDto(input.Name.ToUpperInvariant()))));
        try
        {
            var response = await client.PostAsJsonAsync("/test", new InputDto("hello"));
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<OutputDto>();
            Assert.AreEqual("HELLO", body!.Name);
        }
        finally { await adapter.App.StopAsync(); }
    }

    [TestMethod]
    public async Task Route_WithBody_WhenHandlerThrows_Returns422WithErrorMessage()
    {
        var (adapter, client) = CreateTestServer(s =>
            s.Route<InputDto, OutputDto>("post", "/test",
                (_, _) => throw new InvalidOperationException("handler error")));
        try
        {
            var response = await client.PostAsJsonAsync("/test", new InputDto("data"));
            Assert.AreEqual((HttpStatusCode)422, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<ErrorBody>();
            Assert.AreEqual("handler error", body!.Message);
        }
        finally { await adapter.App.StopAsync(); }
    }

    [TestMethod]
    public async Task Route_WithBody_WithColonUrlParam_PassesParamToHandler()
    {
        string? captured = null;
        var (adapter, client) = CreateTestServer(s =>
            s.Route<InputDto, OutputDto>("put", "/items/:id",
                (@params, input) =>
                {
                    captured = @params["id"];
                    return Task.FromResult(new OutputDto(input.Name));
                }));
        try
        {
            await client.PutAsJsonAsync("/items/xyz", new InputDto("data"));
            Assert.AreEqual("xyz", captured);
        }
        finally { await adapter.App.StopAsync(); }
    }

    [TestMethod]
    public async Task Route_HttpMethodIsRegistered_OnlyMatchesSpecifiedMethod()
    {
        var (adapter, client) = CreateTestServer(s =>
            s.Route<string>("get", "/test", _ => Task.FromResult("ok")));
        try
        {
            var getResponse = await client.GetAsync("/test");
            var postResponse = await client.PostAsync("/test", null);
            Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);
            Assert.AreNotEqual(HttpStatusCode.OK, postResponse.StatusCode);
        }
        finally { await adapter.App.StopAsync(); }
    }
}
