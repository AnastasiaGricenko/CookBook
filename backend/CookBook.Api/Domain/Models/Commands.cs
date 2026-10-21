using CookBook.Api.Domain.Entities;

namespace CookBook.Api.Domain.Models;

/// <summary>Команда входа.</summary>
public sealed record LoginCommand(string Email, string Password);

/// <summary>Команда регистрации.</summary>
public sealed record RegisterCommand(string Name, string Email, string Password);

/// <summary>Результат успешной аутентификации: пользователь и подписанный токен.</summary>
public sealed record AuthResult(User User, string Token);

/// <summary>
/// Команда создания рецепта. Автор в команду не входит: он берётся из сессии
/// (анти-mass-assignment). Пустые коллекции трактуются как отсутствующие.
/// </summary>
public sealed record CreateRecipeCommand(
    string? Title,
    string? ImageUrl,
    int TimeMinutes,
    string? Description,
    int Servings,
    string? Difficulty,
    int Kcal,
    int Protein,
    int Fat,
    int Carbs,
    IReadOnlyList<RecipeIngredientInput>? Ingredients,
    IReadOnlyList<string>? Seasonings,
    IReadOnlyList<RecipeStepInput>? Steps,
    IReadOnlyList<string>? Tags)
{
    /// <summary>Картинка по умолчанию, если клиент не передал ссылку.</summary>
    public const string DefaultImageUrl = "/assets/borsch.png";
}

/// <summary>Ингредиент в команде создания: свободные строки до валидации.</summary>
public sealed record RecipeIngredientInput(string? Name, string? Amount);

/// <summary>Шаг приготовления в команде создания: свободные строки до валидации.</summary>
public sealed record RecipeStepInput(string? Title, string? Text);
