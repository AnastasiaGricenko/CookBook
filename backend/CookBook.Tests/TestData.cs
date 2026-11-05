namespace CookBook.Tests;

/// <summary>
/// Идентификаторы демо-данных из backend/docs/db/kulibook_init.sql.
/// Значения совпадают с прежним кодом сида, поэтому сценарии тестов не меняются.
/// </summary>
internal static class TestData
{
    /// <summary>Guid рецепта по номеру сида: 10000000-0000-0000-0000-0000000000NN.</summary>
    public static Guid RecipeId(int number) =>
        Guid.Parse($"10000000-0000-0000-0000-{number:D12}");
}
