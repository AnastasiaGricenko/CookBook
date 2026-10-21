using CookBook.Api.Domain.Models;
using CookBook.Api.Domain.Entities;

namespace CookBook.Api.Domain.Services;

/// <summary>Доменный сервис аутентификации и регистрации.</summary>
public interface IAuthService
{
    /// <summary>Проверяет учётные данные и выдаёт токен.</summary>
    Task<AuthResult> LoginAsync(LoginCommand command, CancellationToken ct);

    /// <summary>Регистрирует пользователя и сразу выдаёт токен (автовход).</summary>
    Task<AuthResult> RegisterAsync(RegisterCommand command, CancellationToken ct);

    /// <summary>Возвращает пользователя по id; отсутствует — 401 «Требуется вход».</summary>
    Task<User> GetCurrentUserAsync(Guid userId, CancellationToken ct);
}
