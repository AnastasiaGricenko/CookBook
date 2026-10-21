using System.Net;
using System.Net.Http.Json;
using CookBook.Api.Presentation.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CookBook.Tests;

/// <summary>Интеграционные тесты аутентификации: регистрация, вход, me, выход.</summary>
public sealed class AuthApiTests(CookBookFactory factory) : IClassFixture<CookBookFactory>
{
    [Fact]
    public async Task Register_Returns201UserAndCookie()
    {
        var client = TestHttp.CreateGuest(factory);
        var email = $"new-{Guid.NewGuid():N}@example.com";

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            name = "Новый Пользователь",
            email,
            password = "secret123",
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(user);
        Assert.Equal(email, user.Email);
        Assert.Equal("Новый Пользователь", user.Name);
        Assert.Null(user.AvatarUrl);

        var setCookie = response.Headers.GetValues("Set-Cookie").Single().ToLowerInvariant();
        Assert.Contains("httponly", setCookie);
        Assert.Contains("samesite=strict", setCookie);
    }

    [Fact]
    public async Task Register_WithTakenEmail_Returns400()
    {
        var client = TestHttp.CreateGuest(factory);

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            name = "Мария Козлова",
            email = "DEMO@kulibook.ru ",
            password = "demo123",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("Этот email уже зарегистрирован", error.Message);
    }

    [Fact]
    public async Task Register_WithInvalidFields_Returns400WithFieldErrors()
    {
        var client = TestHttp.CreateGuest(factory);

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            name = "А",
            email = "битый-email",
            password = "12345",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("Данные заполнены неверно", error.Message);
        Assert.NotNull(error.Errors);
        Assert.Equal("Имя — не менее 2 символов", error.Errors["name"]);
        Assert.Equal("Введите корректный email", error.Errors["email"]);
        Assert.Equal("Пароль должен содержать не менее 6 символов", error.Errors["password"]);
    }

    [Fact]
    public async Task Login_Demo_Returns200AndUser()
    {
        var client = TestHttp.CreateGuest(factory);

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = TestHttp.DemoEmail,
            password = TestHttp.DemoPassword,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(user);
        Assert.Equal("Мария Козлова", user.Name);
        Assert.Equal("demo@kulibook.ru", user.Email);
        Assert.True(response.Headers.Contains("Set-Cookie"));
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var client = TestHttp.CreateGuest(factory);

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = TestHttp.DemoEmail,
            password = "wrong-password",
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("Неверный email или пароль", error.Message);
    }

    [Fact]
    public async Task Me_WithoutCookie_Returns401()
    {
        var client = TestHttp.CreateGuest(factory);

        var response = await client.GetAsync("/api/v1/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("Требуется вход", error.Message);
    }

    [Fact]
    public async Task Me_WithCookie_ReturnsCurrentUser()
    {
        var client = await TestHttp.CreateAuthorizedAsync(factory);

        var response = await client.GetAsync("/api/v1/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(user);
        Assert.Equal(TestHttp.DemoEmail, user.Email);
    }

    [Fact]
    public async Task Logout_Returns204AndClearsCookie()
    {
        // Клиент с CookieContainer: удаление cookie сервером должно реально её убрать.
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });

        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = TestHttp.DemoEmail,
            password = TestHttp.DemoPassword,
        });
        login.EnsureSuccessStatusCode();

        var logout = await client.PostAsync("/api/v1/auth/logout", null);

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        var me = await client.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, me.StatusCode);
    }
}
