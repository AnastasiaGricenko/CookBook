using CookBook.Api.Domain;
using CookBook.Api.Domain.Entities;
using CookBook.Api.Domain.Repositories;

namespace CookBook.Api.Infrastructure.InMemory;

/// <summary>
/// Репозиторий рецептов поверх in-memory заглушки. Здесь сосредоточен весь CRUD
/// агрегата рецепта — сервисы и презентация хранилища не видят.
/// </summary>
public sealed class InMemoryRecipeRepository(InMemoryDatabase db) : IRecipeRepository
{
    /// <inheritdoc />
    public Task<IReadOnlyList<Recipe>> GetAllAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        lock (db.SyncRoot)
        {
            // Снимок списка: вызывающий код не держит замок во время обработки.
            IReadOnlyList<Recipe> snapshot = db.Recipes.Values.ToList();
            return Task.FromResult(snapshot);
        }
    }

    /// <inheritdoc />
    public Task<Recipe?> FindByIdAsync(Guid id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        lock (db.SyncRoot)
        {
            db.Recipes.TryGetValue(id, out var recipe);
            return Task.FromResult(recipe);
        }
    }

    /// <inheritdoc />
    public Task<Recipe> AddAsync(Recipe recipe, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        lock (db.SyncRoot)
        {
            db.Recipes[recipe.Id] = recipe;
            return Task.FromResult(recipe);
        }
    }

    /// <inheritdoc />
    public Task<Recipe> UpdateAsync(Recipe recipe, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        lock (db.SyncRoot)
        {
            if (!db.Recipes.ContainsKey(recipe.Id))
            {
                throw new NotFoundException(Messages.RecipeNotFound);
            }

            db.Recipes[recipe.Id] = recipe;
            return Task.FromResult(recipe);
        }
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        lock (db.SyncRoot)
        {
            var removed = db.Recipes.Remove(id);
            return Task.FromResult(removed);
        }
    }
}
