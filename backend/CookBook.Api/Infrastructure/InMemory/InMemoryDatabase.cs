using CookBook.Api.Domain.Entities;

namespace CookBook.Api.Infrastructure.InMemory;

/// <summary>
/// In-memory заглушка БД: данные живут в памяти процесса до остановки приложения.
/// Словари сохраняют порядок вставки — это порядок выдачи каталога и похожих.
/// Любой доступ выполняется под общим <see cref="SyncRoot"/> и только через репозитории.
/// </summary>
public sealed class InMemoryDatabase
{
    /// <summary>Общий замок для всех коллекций хранилища.</summary>
    public object SyncRoot { get; } = new();

    /// <summary>Пользователи по идентификатору.</summary>
    public Dictionary<Guid, User> Users { get; } = [];

    /// <summary>Рецепты по идентификатору (порядок вставки = порядок выдачи).</summary>
    public Dictionary<Guid, Recipe> Recipes { get; } = [];
}
