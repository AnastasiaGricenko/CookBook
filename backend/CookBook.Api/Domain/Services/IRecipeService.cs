using CookBook.Api.Domain.Entities;
using CookBook.Api.Domain.Models;

namespace CookBook.Api.Domain.Services;

/// <summary>Доменный сервис каталога рецептов и создания новых рецептов.</summary>
public interface IRecipeService
{
    /// <summary>Поиск по названию/автору и постраничная выдача карточек.</summary>
    Task<PagedResult<RecipeCard>> ListAsync(RecipeListQuery query, CancellationToken ct);

    /// <summary>Полный рецепт; отсутствует — <see cref="NotFoundException"/>.</summary>
    Task<Recipe> GetAsync(Guid id, CancellationToken ct);

    /// <summary>До 4 похожих рецептов, кроме текущего.</summary>
    Task<IReadOnlyList<RecipeCard>> GetSimilarAsync(Guid id, CancellationToken ct);

    /// <summary>Создаёт рецепт от имени авторизованного пользователя.</summary>
    Task<Recipe> CreateAsync(CreateRecipeCommand command, Guid authorId, CancellationToken ct);
}
