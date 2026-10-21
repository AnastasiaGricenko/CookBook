namespace CookBook.Api.Presentation.Contracts;

/// <summary>Текущий пользователь (форма MeDto клиента).</summary>
public sealed record UserDto(Guid Id, string Email, string Name, string? AvatarUrl);

/// <summary>Пищевая ценность.</summary>
public sealed record NutritionDto(int Kcal, int Protein, int Fat, int Carbs);

/// <summary>Ингредиент рецепта.</summary>
public sealed record RecipeIngredientDto(string Name, string Amount);

/// <summary>Шаг приготовления.</summary>
public sealed record RecipeStepDto(string Title, string Text);

/// <summary>Карточка каталога.</summary>
public sealed record RecipeCardDto(
    Guid Id,
    string Title,
    string ImageUrl,
    int TimeMinutes,
    int Kcal,
    string Author);

/// <summary>Полный рецепт карточки рецепта.</summary>
public sealed record RecipeDto(
    Guid Id,
    string Title,
    string ImageUrl,
    int TimeMinutes,
    int Kcal,
    string Author,
    string Description,
    Guid AuthorId,
    string AuthorAvatar,
    int Servings,
    string Difficulty,
    NutritionDto Nutrition,
    IReadOnlyList<RecipeIngredientDto> Ingredients,
    IReadOnlyList<string> Seasonings,
    IReadOnlyList<RecipeStepDto> Steps,
    IReadOnlyList<string> Tags);

/// <summary>Постраничная выдача.</summary>
public sealed record PagedResultDto<T>(IReadOnlyList<T> Items, int Total);

/// <summary>Состояние сервиса.</summary>
public sealed record HealthDto(string Status);
