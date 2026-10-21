namespace CookBook.Api.Domain.Entities;

/// <summary>Шаг приготовления: короткий заголовок и текст.</summary>
public sealed class RecipeStep
{
    /// <summary>Заголовок шага.</summary>
    public required string Title { get; set; }

    /// <summary>Описание шага.</summary>
    public required string Text { get; set; }
}
