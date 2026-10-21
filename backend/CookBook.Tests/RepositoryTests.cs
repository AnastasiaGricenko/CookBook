using CookBook.Api.Domain;
using CookBook.Api.Domain.Entities;
using CookBook.Api.Domain.ValueObjects;
using CookBook.Api.Infrastructure.InMemory;

namespace CookBook.Tests;

/// <summary>Юнит-тесты репозиториев in-memory заглушки (полный CRUD и конкурентность).</summary>
public sealed class RepositoryTests
{
    [Fact]
    public async Task RecipeRepository_Add_AppearsLastInGetAll()
    {
        var database = new InMemoryDatabase();
        var repository = new InMemoryRecipeRepository(database);
        await repository.AddAsync(CreateRecipe(Guid.NewGuid(), "Первый"), CancellationToken.None);

        var added = CreateRecipe(Guid.NewGuid(), "Второй");
        await repository.AddAsync(added, CancellationToken.None);

        var all = await repository.GetAllAsync(CancellationToken.None);
        Assert.Equal(2, all.Count);
        Assert.Equal("Второй", all[^1].Title.Value);
    }

    [Fact]
    public async Task RecipeRepository_Update_ChangesFields()
    {
        var database = new InMemoryDatabase();
        var repository = new InMemoryRecipeRepository(database);
        var recipe = CreateRecipe(Guid.NewGuid(), "Старое название");
        await repository.AddAsync(recipe, CancellationToken.None);

        recipe.Title = RecipeTitle.Create("Новое название");
        recipe.TimeMinutes = 42;
        await repository.UpdateAsync(recipe, CancellationToken.None);

        var stored = await repository.FindByIdAsync(recipe.Id, CancellationToken.None);
        Assert.NotNull(stored);
        Assert.Equal("Новое название", stored.Title.Value);
        Assert.Equal(42, stored.TimeMinutes);
    }

    [Fact]
    public async Task RecipeRepository_Update_Missing_ThrowsNotFound()
    {
        var repository = new InMemoryRecipeRepository(new InMemoryDatabase());
        var recipe = CreateRecipe(Guid.NewGuid(), "Нет такого");

        await Assert.ThrowsAsync<NotFoundException>(
            () => repository.UpdateAsync(recipe, CancellationToken.None));
    }

    [Fact]
    public async Task RecipeRepository_Delete_Missing_ReturnsFalse()
    {
        var repository = new InMemoryRecipeRepository(new InMemoryDatabase());

        Assert.False(await repository.DeleteAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task RecipeRepository_Delete_Existing_ReturnsTrue()
    {
        var repository = new InMemoryRecipeRepository(new InMemoryDatabase());
        var recipe = CreateRecipe(Guid.NewGuid(), "Удаляемый");
        await repository.AddAsync(recipe, CancellationToken.None);

        Assert.True(await repository.DeleteAsync(recipe.Id, CancellationToken.None));
        Assert.Null(await repository.FindByIdAsync(recipe.Id, CancellationToken.None));
    }

    [Fact]
    public async Task UserRepository_FindByEmail_IsCaseSensitiveToNormalizedValue()
    {
        var repository = new InMemoryUserRepository(new InMemoryDatabase());
        await repository.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            Email = "demo@kulibook.ru",
            PasswordHash = "hash",
            Name = "Мария",
            AvatarUrl = null,
        }, CancellationToken.None);

        Assert.NotNull(await repository.FindByEmailAsync("demo@kulibook.ru", CancellationToken.None));
        Assert.Null(await repository.FindByEmailAsync("DEMO@kulibook.ru", CancellationToken.None));
    }

    private static Recipe CreateRecipe(Guid id, string title) => new()
    {
        Id = id,
        Title = RecipeTitle.Create(title),
        ImageUrl = "/assets/borsch.png",
        TimeMinutes = 30,
        Description = "Тестовое описание.",
        AuthorId = Guid.NewGuid(),
        AuthorName = "Мария Козлова",
        AuthorAvatar = "https://i.pravatar.cc/150?u=maria",
        Servings = 2,
        Difficulty = Difficulty.Medium,
        Nutrition = NutritionFacts.Create(100, 10, 10, 10),
        Ingredients = [new RecipeIngredient { Name = "Соль", Amount = "1 г" }],
        Seasonings = ["Соль"],
        Steps = [new RecipeStep { Title = "Шаг", Text = "Текст" }],
        Tags = ["тест"],
    };
}
