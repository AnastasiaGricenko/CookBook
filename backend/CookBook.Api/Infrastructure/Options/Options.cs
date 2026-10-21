namespace CookBook.Api.Infrastructure.Options;

/// <summary>Настройки аутентификации (секция Auth).</summary>
public sealed class AuthOptions
{
    /// <summary>Имя секции конфигурации.</summary>
    public const string SectionName = "Auth";

    /// <summary>Ключ подписи JWT (минимум 32 символа, вне репозитория — секрет).</summary>
    public string JwtKey { get; set; } = string.Empty;

    /// <summary>Издатель токена.</summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>Срок жизни токена в минутах.</summary>
    public int AccessTtlMinutes { get; set; } = 60;

    /// <summary>Имя httpOnly-cookie с токеном.</summary>
    public string CookieName { get; set; } = "access_token";
}

/// <summary>Настройки CORS (секция Cors).</summary>
public sealed class CorsOptions
{
    /// <summary>Имя секции конфигурации.</summary>
    public const string SectionName = "Cors";

    /// <summary>
    /// Разрешённые источники. Пусто — политика не добавляет разрешений:
    /// dev-фронтенд ходит через proxy того же origin.
    /// </summary>
    public string[] AllowedOrigins { get; set; } = [];
}

/// <summary>Настройки хранилища (секция Persistence).</summary>
public sealed class PersistenceOptions
{
    /// <summary>Имя секции конфигурации.</summary>
    public const string SectionName = "Persistence";

    /// <summary>Провайдер хранилища; поддерживается только InMemory.</summary>
    public string Provider { get; set; } = "InMemory";
}

/// <summary>Настройки сида демо-данных (секция Seed).</summary>
public sealed class SeedOptions
{
    /// <summary>Имя секции конфигурации.</summary>
    public const string SectionName = "Seed";

    /// <summary>Выполнять ли сид при старте.</summary>
    public bool Enabled { get; set; } = true;
}
