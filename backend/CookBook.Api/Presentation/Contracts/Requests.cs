namespace CookBook.Api.Presentation.Contracts;

/// <summary>Тело запроса входа. null-поля контроллер превращает в пустые строки.</summary>
public sealed record LoginRequest(string? Email, string? Password);

/// <summary>Тело запроса регистрации. null-поля контроллер превращает в пустые строки.</summary>
public sealed record RegisterRequest(string? Name, string? Email, string? Password);

/// <summary>Тело запроса создания рецепта; автор берётся из сессии, а не из тела.</summary>
public sealed record CreateRecipeRequest(
    string? Title,
    string? ImageUrl,
    int? TimeMinutes,
    string? Description,
    int? Servings,
    string? Difficulty,
    NutritionRequest? Nutrition,
    IReadOnlyList<CreateIngredientRequest>? Ingredients,
    IReadOnlyList<string>? Seasonings,
    IReadOnlyList<CreateStepRequest>? Steps,
    IReadOnlyList<string>? Tags);

/// <summary>Пищевая ценность в теле создания рецепта.</summary>
public sealed record NutritionRequest(int? Kcal, int? Protein, int? Fat, int? Carbs);

/// <summary>Ингредиент в теле создания рецепта.</summary>
public sealed record CreateIngredientRequest(string? Name, string? Amount);

/// <summary>Шаг приготовления в теле создания рецепта.</summary>
public sealed record CreateStepRequest(string? Title, string? Text);
