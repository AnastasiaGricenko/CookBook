using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CookBook.Api.Infrastructure.InMemory.Seed;

namespace CookBook.Tests;

/// <summary>
/// Контракт ошибок и защита от утечки хеша пароля: единая форма ответа,
/// errors только у 400 валидации, отсутствие passwordHash в любом ответе.
/// </summary>
public sealed class ErrorContractTests(CookBookFactory factory) : IClassFixture<CookBookFactory>
{
    [Fact]
    public async Task NotFound_HasMessageAndNoErrorsField()
    {
        var client = TestHttp.CreateGuest(factory);

        var response = await client.GetAsync($"/api/v1/recipes/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal("Рецепт не найден", json.RootElement.GetProperty("message").GetString());
        Assert.False(json.RootElement.TryGetProperty("errors", out _));
    }

    [Fact]
    public async Task Unauthorized_HasMessageAndNoErrorsField()
    {
        var client = TestHttp.CreateGuest(factory);

        var response = await client.GetAsync("/api/v1/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal("Требуется вход", json.RootElement.GetProperty("message").GetString());
        Assert.False(json.RootElement.TryGetProperty("errors", out _));
    }

    [Fact]
    public async Task ValidationError_ErrorsIsObjectOfStrings()
    {
        var client = TestHttp.CreateGuest(factory);

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            name = "А",
            email = "bad",
            password = "123",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        var errors = json.RootElement.GetProperty("errors");
        Assert.Equal(JsonValueKind.Object, errors.ValueKind);

        foreach (var field in errors.EnumerateObject())
        {
            // Клиентский тип ApiError.body.errors — Record<string, string>, не массивы.
            Assert.Equal(JsonValueKind.String, field.Value.ValueKind);
        }

        Assert.Equal("Имя — не менее 2 символов", errors.GetProperty("name").GetString());
    }

    [Fact]
    public async Task NoResponseContainsPasswordHash()
    {
        var guest = TestHttp.CreateGuest(factory);
        var authorized = await TestHttp.CreateAuthorizedAsync(factory);
        var borschId = SeedData.RecipeId(2);

        var responses = new[]
        {
            await guest.GetAsync("/api/v1/health"),
            await guest.GetAsync("/api/v1/recipes"),
            await guest.GetAsync($"/api/v1/recipes/{borschId}"),
            await guest.GetAsync($"/api/v1/recipes/{Guid.NewGuid()}"),
            await guest.GetAsync("/api/v1/auth/me"),
            await authorized.GetAsync("/api/v1/auth/me"),
            await guest.PostAsJsonAsync("/api/v1/auth/login", new
            {
                email = TestHttp.DemoEmail,
                password = TestHttp.DemoPassword,
            }),
        };

        foreach (var response in responses)
        {
            var body = await response.Content.ReadAsStringAsync();
            Assert.DoesNotContain("passwordHash", body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("password_hash", body, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task UnknownRoute_Returns404()
    {
        var client = TestHttp.CreateGuest(factory);

        var response = await client.GetAsync("/api/v1/nope");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
