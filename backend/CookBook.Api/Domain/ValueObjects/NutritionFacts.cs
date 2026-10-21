namespace CookBook.Api.Domain.ValueObjects;

/// <summary>
/// Пищевая ценность блюда. Все четыре значения — целые в диапазоне 0..10 000;
/// создавать объект следует через <see cref="Create"/>, который проверяет инвариант.
/// </summary>
public sealed record NutritionFacts(int Kcal, int Protein, int Fat, int Carbs)
{
    private const int MinValue = 0;
    private const int MaxValue = 10_000;

    /// <summary>Создаёт объект-значение, проверяя диапазон каждого показателя.</summary>
    public static NutritionFacts Create(int kcal, int protein, int fat, int carbs)
    {
        var errors = new Dictionary<string, string[]>();

        AddIfOutOfRange(errors, "kcal", kcal);
        AddIfOutOfRange(errors, "protein", protein);
        AddIfOutOfRange(errors, "fat", fat);
        AddIfOutOfRange(errors, "carbs", carbs);

        if (errors.Count > 0)
        {
            throw DomainValidationException.InvalidForm(errors);
        }

        return new NutritionFacts(kcal, protein, fat, carbs);
    }

    private static void AddIfOutOfRange(Dictionary<string, string[]> errors, string field, int value)
    {
        if (value is < MinValue or > MaxValue)
        {
            errors[field] = [ValidationTexts.NutritionOutOfRange];
        }
    }
}
