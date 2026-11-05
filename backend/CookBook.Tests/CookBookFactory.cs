using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Npgsql;

namespace CookBook.Tests;

/// <summary>
/// Общая фабрика приложения для интеграционных тестов. Ключ подписи задаётся
/// в памяти (последним слоем конфигурации), поэтому тесты не зависят от local secrets.
/// Тесты работают против реального PostgreSQL: строка подключения берётся из
/// переменной окружения KULIBOOK_PG с фолбэком на локальную БД kulibook.
/// Перед каждым тестовым классом схема пересоздаётся скриптом из docs/db,
/// чтобы прогоны не зависели от данных, оставленных предыдущими тестами.
/// </summary>
public sealed class CookBookFactory : WebApplicationFactory<Program>
{
    /// <summary>Тестовый ключ подписи JWT (не секрет, только для тестов).</summary>
    public const string TestJwtKey = "cookbook-tests-only-jwt-key-32+chars";

    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=kulibook;Username=postgres;Password=postgres";

    private static readonly object ResetGate = new();
    private bool _databaseReset;

    /// <summary>Строка подключения к PostgreSQL, используемая тестами.</summary>
    public static string ConnectionString { get; } =
        Environment.GetEnvironmentVariable("KULIBOOK_PG") ?? DefaultConnectionString;

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ResetDatabase();

        // UseSetting попадает в конфигурацию до выполнения Program.cs, поэтому
        // строка подключения видна уже при регистрации инфраструктуры.
        builder.UseSetting("Persistence:Provider", "Postgres");
        builder.UseSetting("ConnectionStrings:Default", ConnectionString);
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.Add(new MemoryConfigurationSource
            {
                InitialData = new Dictionary<string, string?>
                {
                    ["Auth:JwtKey"] = TestJwtKey,
                    ["Auth:Issuer"] = "CookBook.Api",
                    ["Auth:AccessTtlMinutes"] = "60",
                    ["Auth:CookieName"] = "access_token",
                    ["Persistence:Provider"] = "Postgres",
                    ["ConnectionStrings:Default"] = ConnectionString,
                },
            });
        });
    }

    /// <summary>
    /// Пересоздаёт схему и заливает демо-данные тем же скриптом, который
    /// выполняется в pgAdmin. Тесты не создают схему кодом приложения.
    /// </summary>
    private void ResetDatabase()
    {
        if (_databaseReset)
        {
            return;
        }

        lock (ResetGate)
        {
            if (_databaseReset)
            {
                return;
            }

            var scriptPath = Path.Combine(AppContext.BaseDirectory, "kulibook_init.sql");
            var script = File.ReadAllText(scriptPath);

            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            using var command = new NpgsqlCommand(script, connection);
            command.ExecuteNonQuery();

            _databaseReset = true;
        }
    }
}
