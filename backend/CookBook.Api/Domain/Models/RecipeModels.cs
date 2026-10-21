namespace CookBook.Api.Domain.Models;

/// <summary>Параметры запроса каталога: поиск и пагинация (1-based).</summary>
public sealed record RecipeListQuery(string Query, int Page, int PageSize);

/// <summary>Доменная проекция карточки каталога.</summary>
public sealed record RecipeCard(
    Guid Id,
    string Title,
    string ImageUrl,
    int TimeMinutes,
    int Kcal,
    string Author);

/// <summary>Постраничная выдача: элементы текущей страницы и общее количество.</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Total);
