using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CookBook.Tests;

/// <summary>
/// Хелперы HTTP-тестов: гостевой клиент и клиент с cookie демо-пользователя.
/// Cookie ставится заголовком вручную — так тест не зависит от CookieContainer.
/// </summary>
internal static class TestHttp
{
    public const string DemoEmail = "demo@kulibook.ru";
    public const string DemoPassword = "demo123";

    /// <summary>Клиент без cookie (гость).</summary>
    public static HttpClient CreateGuest(CookBookFactory factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false });

    /// <summary>Клиент с cookie демо-пользователя.</summary>
    public static async Task<HttpClient> CreateAuthorizedAsync(CookBookFactory factory)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false });
        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email = DemoEmail, password = DemoPassword });

        response.EnsureSuccessStatusCode();

        client.DefaultRequestHeaders.Add("Cookie", ExtractCookie(response));
        return client;
    }

    /// <summary>Достаёт пару "имя=значение" из первого Set-Cookie.</summary>
    public static string ExtractCookie(HttpResponseMessage response)
    {
        var setCookie = response.Headers.TryGetValues("Set-Cookie", out var values)
            ? values.First()
            : throw new InvalidOperationException("Ответ не содержит Set-Cookie.");

        return setCookie.Split(';')[0];
    }
}
