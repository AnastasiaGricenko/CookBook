using CookBook.Api.Domain;
using CookBook.Api.Domain.Entities;
using CookBook.Api.Domain.ValueObjects;
using CookBook.Api.Infrastructure.Persistence;
using CookBook.Api.Infrastructure.Persistence.Ef;
using CookBook.Api.Infrastructure.Persistence.Sql;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CookBook.Tests;

/// <summary>
/// Тесты репозиториев поверх реального PostgreSQL: EF Core для пользователей
/// и сырой SQL для рецептов. Все создаваемые данные имеют случайные id/email,
/// чтобы повторные прогоны не упирались в UNIQUE-ограничения.
/// </summary>
public sealed class RepositoryTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    [Fact]
    public async Task RecipeRepository_Add_AppearsLastInGetAll()
    {
        using var dataSource = CreateDataSource();
        var users = CreateUserRepository();
        var author = await users.AddAsync(NewUser(), Ct);
        var repository = new SqlRecipeRepository(dataSource);

        // Даты в прошлом: демо-рецепты (2024) и созданные приложением (сейчас)
        // остаются выше, а наши два рецепта занимают конец каталога.
        var first = CreateRecipe(author.Id, "Первый", new DateTimeOffset(2000, 1, 2, 0, 0, 0, TimeSpan.Zero));
        var second = CreateRecipe(author.Id, "Второй", new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero));

        try
        {
            await repository.AddAsync(first, Ct);
            await repository.AddAsync(second, Ct);

            var all = await repository.GetAllAsync(Ct);
            Assert.True(all.Count >= 2);
            Assert.Equal(second.Id, all[^1].Id);
            Assert.Equal("Второй", all[^1].Title.Value);
        }
        finally
        {
            await repository.DeleteAsync(first.Id, Ct);
            await repository.DeleteAsync(second.Id, Ct);
        }
    }

    [Fact]
    public async Task RecipeRepository_Update_ChangesFields()
    {
        using var dataSource = CreateDataSource();
        var users = CreateUserRepository();
        var author = await users.AddAsync(NewUser(), Ct);
        var repository = new SqlRecipeRepository(dataSource);
        var recipe = CreateRecipe(author.Id, "Старое название", DateTimeOffset.UtcNow);

        try
        {
            await repository.AddAsync(recipe, Ct);

            recipe.Title = RecipeTitle.Create("Новое название");
            recipe.TimeMinutes = 42;
            await repository.UpdateAsync(recipe, Ct);

            var stored = await repository.FindByIdAsync(recipe.Id, Ct);
            Assert.NotNull(stored);
            Assert.Equal("Новое название", stored.Title.Value);
            Assert.Equal(42, stored.TimeMinutes);
        }
        finally
        {
            await repository.DeleteAsync(recipe.Id, Ct);
        }
    }

    [Fact]
    public async Task RecipeRepository_Update_Missing_ThrowsNotFound()
    {
        using var dataSource = CreateDataSource();
        var repository = new SqlRecipeRepository(dataSource);
        var recipe = CreateRecipe(Guid.NewGuid(), "Нет такого", DateTimeOffset.UtcNow);

        await Assert.ThrowsAsync<NotFoundException>(
            () => repository.UpdateAsync(recipe, Ct));
    }

    [Fact]
    public async Task RecipeRepository_Delete_Missing_ReturnsFalse()
    {
        using var dataSource = CreateDataSource();
        var repository = new SqlRecipeRepository(dataSource);

        Assert.False(await repository.DeleteAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task RecipeRepository_Delete_Existing_ReturnsTrue()
    {
        using var dataSource = CreateDataSource();
        var users = CreateUserRepository();
        var author = await users.AddAsync(NewUser(), Ct);
        var repository = new SqlRecipeRepository(dataSource);
        var recipe = CreateRecipe(author.Id, "Удаляемый", DateTimeOffset.UtcNow);

        await repository.AddAsync(recipe, Ct);

        Assert.True(await repository.DeleteAsync(recipe.Id, Ct));
        Assert.Null(await repository.FindByIdAsync(recipe.Id, Ct));
    }

    [Fact]
    public async Task UserRepository_FindByEmail_IsCaseSensitiveToNormalizedValue()
    {
        var repository = CreateUserRepository();
        var email = $"test-{Guid.NewGuid():N}@example.com";

        await repository.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = "hash",
            Name = "Мария",
            AvatarUrl = null,
        }, Ct);

        Assert.NotNull(await repository.FindByEmailAsync(email, Ct));
        Assert.Null(await repository.FindByEmailAsync(email.ToUpperInvariant(), Ct));
    }

    private static NpgsqlDataSource CreateDataSource() =>
        new NpgsqlDataSourceBuilder(CookBookFactory.ConnectionString).Build();

    private static EfUserRepository CreateUserRepository() =>
        new(new TestDbContextFactory(CreateDbContextOptions()));

    private static DbContextOptions<CookBookDbContext> CreateDbContextOptions() =>
        new DbContextOptionsBuilder<CookBookDbContext>()
            .UseNpgsql(CookBookFactory.ConnectionString)
            .Options;

    private static User NewUser() => new()
    {
        Id = Guid.NewGuid(),
        Email = $"test-{Guid.NewGuid():N}@example.com",
        PasswordHash = "hash",
        Name = "Тестовый Пользователь",
        AvatarUrl = null,
    };

    private static Recipe CreateRecipe(Guid authorId, string title, DateTimeOffset createdAt) => new()
    {
        Id = Guid.NewGuid(),
        Title = RecipeTitle.Create(title),
        ImageUrl = "/assets/borsch.png",
        TimeMinutes = 30,
        Description = "Тестовое описание.",
        AuthorId = authorId,
        AuthorName = "Мария Козлова",
        AuthorAvatar = "https://i.pravatar.cc/150?u=maria",
        Servings = 2,
        Difficulty = Difficulty.Medium,
        Nutrition = NutritionFacts.Create(100, 10, 10, 10),
        Ingredients = [new RecipeIngredient { Name = "Соль", Amount = "1 г" }],
        Seasonings = ["Соль"],
        Steps = [new RecipeStep { Title = "Шаг", Text = "Текст" }],
        Tags = ["тест"],
        CreatedAt = createdAt,
    };

    private sealed class TestDbContextFactory(DbContextOptions<CookBookDbContext> options)
        : IDbContextFactory<CookBookDbContext>
    {
        public CookBookDbContext CreateDbContext() => new(options);

        public Task<CookBookDbContext> CreateDbContextAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(CreateDbContext());
    }
}
