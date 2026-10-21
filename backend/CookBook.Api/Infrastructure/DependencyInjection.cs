using CookBook.Api.Domain.Repositories;
using CookBook.Api.Domain.Security;
using CookBook.Api.Domain.Services;
using CookBook.Api.Infrastructure.InMemory;
using CookBook.Api.Infrastructure.InMemory.Seed;
using CookBook.Api.Infrastructure.Options;
using CookBook.Api.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CookBook.Api.Infrastructure;

/// <summary>
/// Composition Root инфраструктуры: настройки, хранилище, безопасность и доменные
/// сервисы. Времена жизни фиксируются здесь явно — все регистрации singleton,
/// потому что хранилище общее, а сервисы не хранят состояния.
/// </summary>
public static class InfrastructureDependencyInjection
{
    /// <summary>Регистрирует инфраструктуру и доменные сервисы.</summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<AuthOptions>()
            .Bind(configuration.GetSection(AuthOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.JwtKey),
                "Auth:JwtKey не задан. Укажите ключ подписи JWT в appsettings.Development.json, " +
                "user-secrets или переменной окружения Auth__JwtKey.")
            .Validate(
                options => options.JwtKey.Trim().Length >= 32,
                "Auth:JwtKey слишком короткий: требуется не менее 32 символов.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Issuer),
                "Auth:Issuer не задан. Укажите издателя токена в конфигурации (например, CookBook.Api).")
            .Validate(
                options => options.AccessTtlMinutes > 0,
                "Auth:AccessTtlMinutes должен быть больше нуля.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.CookieName),
                "Auth:CookieName не задан. Укажите имя httpOnly-cookie с токеном.")
            .ValidateOnStart();

        services.AddOptions<CorsOptions>()
            .Bind(configuration.GetSection(CorsOptions.SectionName));

        services.AddOptions<SeedOptions>()
            .Bind(configuration.GetSection(SeedOptions.SectionName));

        services.AddOptions<PersistenceOptions>()
            .Bind(configuration.GetSection(PersistenceOptions.SectionName));

        var persistence = configuration
            .GetSection(PersistenceOptions.SectionName)
            .Get<PersistenceOptions>() ?? new PersistenceOptions();

        if (!string.Equals(persistence.Provider, "InMemory", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Persistence:Provider = «{persistence.Provider}» не поддерживается. " +
                "Доступна только заглушка «InMemory»: реализуйте интерфейсы репозиториев домена " +
                "поверх реальной БД и зарегистрируйте их в AddInfrastructure.");
        }

        services.AddSingleton<InMemoryDatabase>();

        services.TryAddSingleton<IUserRepository, InMemoryUserRepository>();
        services.TryAddSingleton<IRecipeRepository, InMemoryRecipeRepository>();

        services.TryAddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.TryAddSingleton<ITokenService, JwtTokenService>();

        services.TryAddSingleton<IAuthService, AuthService>();
        services.TryAddSingleton<IRecipeService, RecipeService>();

        services.TryAddSingleton<DatabaseSeeder>();

        return services;
    }
}
