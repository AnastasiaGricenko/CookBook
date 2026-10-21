namespace CookBook.Api.Domain.Entities;

/// <summary>Ингредиент рецепта: название и количество в свободной форме.</summary>
public sealed class RecipeIngredient
{
    /// <summary>Название ингредиента.</summary>
    public required string Name { get; set; }

    /// <summary>Количество, например "500 г".</summary>
    public required string Amount { get; set; }
}
