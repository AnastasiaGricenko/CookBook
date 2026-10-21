using CookBook.Api.Domain.Entities;
using CookBook.Api.Domain.Repositories;

namespace CookBook.Api.Infrastructure.InMemory;

/// <summary>Репозиторий пользователей поверх in-memory заглушки.</summary>
public sealed class InMemoryUserRepository(InMemoryDatabase db) : IUserRepository
{
    /// <inheritdoc />
    public Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        lock (db.SyncRoot)
        {
            var user = db.Users.Values.FirstOrDefault(
                candidate => string.Equals(candidate.Email, normalizedEmail, StringComparison.Ordinal));
            return Task.FromResult(user);
        }
    }

    /// <inheritdoc />
    public Task<User?> FindByIdAsync(Guid id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        lock (db.SyncRoot)
        {
            db.Users.TryGetValue(id, out var user);
            return Task.FromResult(user);
        }
    }

    /// <inheritdoc />
    public Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        lock (db.SyncRoot)
        {
            var exists = db.Users.Values.Any(
                candidate => string.Equals(candidate.Email, normalizedEmail, StringComparison.Ordinal));
            return Task.FromResult(exists);
        }
    }

    /// <inheritdoc />
    public Task<User> AddAsync(User user, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        lock (db.SyncRoot)
        {
            db.Users[user.Id] = user;
            return Task.FromResult(user);
        }
    }
}
