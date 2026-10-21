namespace CookBook.Api.Domain.Entities;

/// <summary>
/// Пользователь каталога. Хранит только хеш пароля — открытый пароль не покидает
/// слой аутентификации и никогда не попадает в ответы API.
/// </summary>
public sealed class User
{
    /// <summary>Идентификатор пользователя.</summary>
    public required Guid Id { get; init; }

    /// <summary>Нормализованный email (trim + нижний регистр).</summary>
    public required string Email { get; set; }

    /// <summary>PBKDF2-хеш пароля в формате "iterations.salt.key" (base64).</summary>
    public required string PasswordHash { get; set; }

    /// <summary>Отображаемое имя (trim).</summary>
    public required string Name { get; set; }

    /// <summary>Аватар; null допустим.</summary>
    public string? AvatarUrl { get; set; }
}
