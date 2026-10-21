using CookBook.Api.Domain.ValueObjects;

namespace CookBook.Api.Domain.Entities;

/// <summary>
/// Агрегат рецепта. Данные автора денормализованы: контракт карточки отдаёт
/// имя и аватар без отдельного запроса к пользователю.
/// </summary>
public sealed class Recipe
{
    /// <summary>Идентификатор рецепта.</summary>
    public required Guid Id { get; init; }

    /// <summary>Название (объект-значение с собственной валидацией).</summary>
    public required RecipeTitle Title { get; set; }

    /// <summary>Путь клиентского ассета, например "/assets/borsch.png".</summary>
    public required string ImageUrl { get; set; }

    /// <summary>Время приготовления в минутах (1..1440).</summary>
    public required int TimeMinutes { get; set; }

    /// <summary>Описание блюда.</summary>
    public required string Description { get; set; }

    /// <summary>Идентификатор автора.</summary>
    public required Guid AuthorId { get; set; }

    /// <summary>
    /// Момент создания рецепта. Сид оставляет значение по умолчанию, поэтому
    /// новые рецепты сортируются выше предзаполненных (порядок сида сохраняется).
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Денормализованное имя автора (поле author контракта).</summary>
    public required string AuthorName { get; set; }

    /// <summary>Денормализованный аватар автора.</summary>
    public required string AuthorAvatar { get; set; }

    /// <summary>Количество порций (1..100).</summary>
    public required int Servings { get; set; }

    /// <summary>Сложность приготовления.</summary>
    public required Difficulty Difficulty { get; set; }

    /// <summary>Пищевая ценность (объект-значение).</summary>
    public required NutritionFacts Nutrition { get; set; }

    /// <summary>Ингредиенты.</summary>
    public required List<RecipeIngredient> Ingredients { get; set; }

    /// <summary>Приправы (может быть пустым списком).</summary>
    public required List<string> Seasonings { get; set; }

    /// <summary>Шаги приготовления.</summary>
    public required List<RecipeStep> Steps { get; set; }

    /// <summary>Теги.</summary>
    public required List<string> Tags { get; set; }
}
