using CookBook.Api.Domain.Entities;

namespace CookBook.Api.Domain.Repositories;

/// <summary>
/// Хранилище рецептов. Полный CRUD сосредоточен здесь: сервисы и контроллеры
/// не работают с хранилищем напрямую (лекция: CRUD — в одном классе).
/// </summary>
public interface IRecipeRepository
{
    /// <summary>Все рецепты в порядке вставки (порядок сида).</summary>
    Task<IReadOnlyList<Recipe>> GetAllAsync(CancellationToken ct);

    /// <summary>Рецепт по идентификатору или null.</summary>
    Task<Recipe?> FindByIdAsync(Guid id, CancellationToken ct);

    /// <summary>Создаёт рецепт.</summary>
    Task<Recipe> AddAsync(Recipe recipe, CancellationToken ct);

    /// <summary>Обновляет рецепт; отсутствующий id → <see cref="NotFoundException"/>.</summary>
    Task<Recipe> UpdateAsync(Recipe recipe, CancellationToken ct);

    /// <summary>Удаляет рецепт; false, если такого не было.</summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
