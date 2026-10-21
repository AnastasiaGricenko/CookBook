using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace CookBook.Tests;

/// <summary>
/// Общая фабрика приложения для интеграционных тестов. Ключ подписи задаётся
/// в памяти (последним слоем конфигурации), поэтому тесты не зависят от local secrets.
/// Каждый тестовый класс получает собственную фабрику и отдельное in-memory хранилище.
/// </summary>
public sealed class CookBookFactory : WebApplicationFactory<Program>
{
    /// <summary>Тестовый ключ подписи JWT (не секрет, только для тестов).</summary>
    public const string TestJwtKey = "cookbook-tests-only-jwt-key-32+chars";

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Auth:JwtKey"] = TestJwtKey,
                ["Auth:Issuer"] = "CookBook.Api",
                ["Auth:AccessTtlMinutes"] = "60",
                ["Auth:CookieName"] = "access_token",
                ["Seed:Enabled"] = "true",
            });
        });
    }
}
