using System.Net;
using System.Net.Http.Json;

namespace CookBook.Tests;

/// <summary>Интеграционные тесты служебного эндпоинта.</summary>
public sealed class HealthApiTests(CookBookFactory factory) : IClassFixture<CookBookFactory>
{
    [Fact]
    public async Task Health_ReturnsOk()
    {
        var client = TestHttp.CreateGuest(factory);

        var response = await client.GetAsync("/api/v1/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(body);
        Assert.Equal("ok", body["status"]);
    }
}
