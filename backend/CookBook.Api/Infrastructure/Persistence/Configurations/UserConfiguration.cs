using CookBook.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CookBook.Api.Infrastructure.Persistence.Configurations;

/// <summary>
/// Маппинг доменного <see cref="User"/> на таблицу users. Дублирующую
/// инфраструктурную сущность не создаём: EF работает с доменным классом напрямую.
/// </summary>
internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(u => u.Email).HasColumnName("email").HasMaxLength(254).IsRequired();
        builder.Property(u => u.PasswordHash).HasColumnName("password_hash").HasMaxLength(256).IsRequired();
        builder.Property(u => u.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(u => u.AvatarUrl).HasColumnName("avatar_url").HasMaxLength(500);

        // UNIQUE-индекс по email уже есть в DDL — в модели объявляем только для целостности.
        builder.HasIndex(u => u.Email).IsUnique();
    }
}
