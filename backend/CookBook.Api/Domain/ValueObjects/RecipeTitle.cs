namespace CookBook.Api.Domain.ValueObjects;

/// <summary>Название рецепта: обязательное, 1..100 символов после trim.</summary>
public sealed record RecipeTitle
{
    private const int MaxLength = 100;

    private RecipeTitle(string value) => Value = value;

    /// <summary>Нормализованное (обрезанное) название.</summary>
    public string Value { get; }

    /// <summary>Создаёт объект-значение, проверяя длину названия.</summary>
    public static RecipeTitle Create(string? raw)
    {
        var value = (raw ?? string.Empty).Trim();

        if (value.Length == 0)
        {
            throw DomainValidationException.InvalidForm(
                ValidationErrors.Field("title", ValidationTexts.Required));
        }

        if (value.Length > MaxLength)
        {
            throw DomainValidationException.InvalidForm(
                ValidationErrors.Field("title", ValidationTexts.TitleMax));
        }

        return new RecipeTitle(value);
    }
}
