using CookBook.Api.Domain.Entities;
using CookBook.Api.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CookBook.Api.Infrastructure.Persistence;

/// <summary>
/// EF Core-контекст для той части доступа к данным, которая реализована через ORM.
/// Схему создаёт SQL-скрипт (docs/db/kulibook_init.sql), поэтому миграции и
/// EnsureCreated не используются: контекст только читает и пишет поверх неё.
/// </summary>
public sealed class CookBookDbContext(DbContextOptions<CookBookDbContext> options)
    : DbContext(options)
{
    /// <summary>Пользователи каталога.</summary>
    public DbSet<User> Users => Set<User>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }
}
