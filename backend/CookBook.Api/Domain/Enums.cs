namespace CookBook.Api.Domain;

/// <summary>Сложность приготовления — доменное перечисление.</summary>
public enum Difficulty
{
    /// <summary>Легкая.</summary>
    Easy,

    /// <summary>Средняя.</summary>
    Medium,

    /// <summary>Сложная.</summary>
    Hard,
}

/// <summary>
/// Перевод сложности между доменом и строковым контрактом REST API.
/// В JSON уходят русские строки — форма зафиксирована мок-контрактом клиента.
/// </summary>
public static class DifficultyMapper
{
    /// <summary>Строка контракта для лёгкой сложности.</summary>
    public const string Easy = "Легкая";

    /// <summary>Строка контракта для средней сложности.</summary>
    public const string Medium = "Средняя";

    /// <summary>Строка контракта для сложной сложности.</summary>
    public const string Hard = "Сложная";

    /// <summary>Доменное значение → строка контракта.</summary>
    public static string ToApiString(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Easy => Easy,
        Difficulty.Medium => Medium,
        Difficulty.Hard => Hard,
        _ => throw new DomainValidationException(
            ValidationTexts.InvalidForm,
            ValidationErrors.Field("difficulty", ValidationTexts.DifficultyInvalid)),
    };

    /// <summary>Строка контракта → доменное значение; неизвестная строка → ошибка валидации.</summary>
    public static Difficulty Parse(string? value) => value?.Trim() switch
    {
        Easy => Difficulty.Easy,
        Medium => Difficulty.Medium,
        Hard => Difficulty.Hard,
        _ => throw new DomainValidationException(
            ValidationTexts.InvalidForm,
            ValidationErrors.Field("difficulty", ValidationTexts.DifficultyInvalid)),
    };
}
