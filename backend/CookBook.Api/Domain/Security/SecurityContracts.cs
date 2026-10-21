namespace CookBook.Api.Domain.Security;

/// <summary>Хеширование и проверка паролей. Домен не знает алгоритма — только контракт.</summary>
public interface IPasswordHasher
{
    /// <summary>Возвращает хеш в формате "iterations.salt.key" (base64-части).</summary>
    string Hash(string password);

    /// <summary>Проверяет пароль против сохранённого хеша с постоянным временем сравнения.</summary>
    bool Verify(string password, string storedHash);
}

/// <summary>Выдача подписанного токена доступа.</summary>
public interface ITokenService
{
    /// <summary>Создаёт токен для пользователя и возвращает его срок действия (UTC).</summary>
    (string Token, DateTime ExpiresAt) CreateAccessToken(Guid userId);
}
