using CookBook.Api.Domain.Entities;

namespace CookBook.Api.Domain.Repositories;

/// <summary>Хранилище пользователей. Реализация живёт в инфраструктуре.</summary>
public interface IUserRepository
{
    /// <summary>Находит пользователя по нормализованному email.</summary>
    Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken ct);

    /// <summary>Находит пользователя по идентификатору.</summary>
    Task<User?> FindByIdAsync(Guid id, CancellationToken ct);

    /// <summary>Проверяет занятость email.</summary>
    Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken ct);

    /// <summary>Добавляет пользователя и возвращает сохранённую сущность.</summary>
    Task<User> AddAsync(User user, CancellationToken ct);
}
