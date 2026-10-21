namespace CookBook.Api.Domain.ValueObjects;

/// <summary>
/// Email-адрес. Хранится нормализованным (trim + нижний регистр), чтобы поиск
/// пользователя не зависел от регистра ввода.
/// </summary>
public sealed record Email
{
    private const int MaxLength = 254;

    private Email(string value) => Value = value;

    /// <summary>Нормализованное значение адреса.</summary>
    public string Value { get; }

    /// <summary>Создаёт объект-значение, проверяя обязательность, длину и формат.</summary>
    public static Email Create(string? raw)
    {
        var value = (raw ?? string.Empty).Trim();

        if (value.Length == 0)
        {
            throw DomainValidationException.InvalidForm(
                ValidationErrors.Field("email", ValidationTexts.Required));
        }

        if (value.Length > MaxLength)
        {
            throw DomainValidationException.InvalidForm(
                ValidationErrors.Field("email", ValidationTexts.EmailMax));
        }

        if (!HasValidFormat(value))
        {
            throw DomainValidationException.InvalidForm(
                ValidationErrors.Field("email", ValidationTexts.EmailFormat));
        }

        return new Email(value.ToLowerInvariant());
    }

    /// <summary>
    /// Упрощённая проверка формата: одна «собака», непустые части, точка в домене.
    /// Отклоняет очевидные ошибки вида «мыло@мыло».
    /// </summary>
    private static bool HasValidFormat(string value)
    {
        var at = value.IndexOf('@');
        if (at <= 0 || at != value.LastIndexOf('@') || at == value.Length - 1)
        {
            return false;
        }

        var local = value[..at];
        var domain = value[(at + 1)..];

        if (local.Contains(' ') || domain.Contains(' ') || local.Contains('\t') || domain.Contains('\t'))
        {
            return false;
        }

        return domain.Contains('.')
            && !domain.StartsWith('.')
            && !domain.EndsWith('.')
            && !domain.Contains("..");
    }
}
