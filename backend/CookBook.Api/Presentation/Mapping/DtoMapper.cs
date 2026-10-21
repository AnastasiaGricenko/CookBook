using System.Text.Json;
using CookBook.Api.Domain;
using CookBook.Api.Domain.Entities;
using CookBook.Api.Domain.Models;
using CookBook.Api.Presentation.Contracts;

namespace CookBook.Api.Presentation.Mapping;

/// <summary>
/// Маппинг доменных сущностей в DTO контракта. Единственное место, где домен
/// встречается с формой JSON (включая русские строки сложности).
/// </summary>
public static class DtoMapper
{
    /// <summary>Пользователь → UserDto (MeDto клиента).</summary>
    public static UserDto ToDto(User user) =>
        new(user.Id, user.Email, user.Name, user.AvatarUrl);

    /// <summary>Доменная карточка → DTO каталога.</summary>
    public static RecipeCardDto ToDto(RecipeCard card) =>
        new(card.Id, card.Title, card.ImageUrl, card.TimeMinutes, card.Kcal, card.Author);

    /// <summary>Полный рецепт → DTO карточки рецепта.</summary>
    public static RecipeDto ToDto(Recipe recipe) => new(
        recipe.Id,
        recipe.Title.Value,
        recipe.ImageUrl,
        recipe.TimeMinutes,
        recipe.Nutrition.Kcal,
        recipe.AuthorName,
        recipe.Description,
        recipe.AuthorId,
        recipe.AuthorAvatar,
        recipe.Servings,
        DifficultyMapper.ToApiString(recipe.Difficulty),
        new NutritionDto(
            recipe.Nutrition.Kcal,
            recipe.Nutrition.Protein,
            recipe.Nutrition.Fat,
            recipe.Nutrition.Carbs),
        recipe.Ingredients
            .Select(ingredient => new RecipeIngredientDto(ingredient.Name, ingredient.Amount))
            .ToList(),
        recipe.Seasonings.ToList(),
        recipe.Steps
            .Select(step => new RecipeStepDto(step.Title, step.Text))
            .ToList(),
        recipe.Tags.ToList());
}

/// <summary>Общие настройки JSON для middleware и ответов вне MVC.</summary>
public static class JsonDefaults
{
    /// <summary>camelCase + нечувствительность к регистру — как у контроллеров.</summary>
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}
