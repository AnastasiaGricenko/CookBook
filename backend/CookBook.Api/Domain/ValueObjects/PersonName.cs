namespace CookBook.Api.Domain.ValueObjects;

/// <summary>Имя человека: обязательное, 2..100 символов после trim.</summary>
public sealed record PersonName
{
    private const int MinLength = 2;
    private const int MaxLength = 100;

    private PersonName(string value) => Value = value;

    /// <summary>Нормализованное (обрезанное) значение имени.</summary>
    public string Value { get; }

    /// <summary>Создаёт объект-значение, проверяя длину имени.</summary>
    public static PersonName Create(string? raw)
    {
        var value = (raw ?? string.Empty).Trim();

        if (value.Length == 0)
        {
            throw DomainValidationException.InvalidForm(
                ValidationErrors.Field("name", ValidationTexts.Required));
        }

        if (value.Length < MinLength)
        {
            throw DomainValidationException.InvalidForm(
                ValidationErrors.Field("name", ValidationTexts.NameMin));
        }

        if (value.Length > MaxLength)
        {
            throw DomainValidationException.InvalidForm(
                ValidationErrors.Field("name", ValidationTexts.NameMax));
        }

        return new PersonName(value);
    }
}
