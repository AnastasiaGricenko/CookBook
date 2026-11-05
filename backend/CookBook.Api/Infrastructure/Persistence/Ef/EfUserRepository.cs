using CookBook.Api.Domain.Entities;
using CookBook.Api.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CookBook.Api.Infrastructure.Persistence.Ef;

/// <summary>
/// Репозиторий пользователей на EF Core (LINQ). Это половина доступа к данным,
/// реализованная через ORM; рецепты намеренно живут в SQL-репозитории.
/// Контекст создаётся на каждый метод через фабрику — репозиторий singleton.
/// </summary>
public sealed class EfUserRepository(IDbContextFactory<CookBookDbContext> factory) : IUserRepository
{
    /// <inheritdoc />
    public async Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Email == normalizedEmail, ct);
    }

    /// <inheritdoc />
    public async Task<User?> FindByIdAsync(Guid id, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == id, ct);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.Users.AnyAsync(u => u.Email == normalizedEmail, ct);
    }

    /// <inheritdoc />
    public async Task<User> AddAsync(User user, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return user;
    }
}
