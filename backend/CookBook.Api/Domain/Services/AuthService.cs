using CookBook.Api.Domain.Entities;
using CookBook.Api.Domain.Models;
using CookBook.Api.Domain.Repositories;
using CookBook.Api.Domain.Security;
using CookBook.Api.Domain.ValueObjects;

namespace CookBook.Api.Domain.Services;

/// <summary>
/// Аутентификация и регистрация. Содержательная логика (нормализация, проверка
/// пароля, занятость email, выдача токена) сосредоточена здесь, а не в контроллере.
/// </summary>
public sealed class AuthService(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    ITokenService tokenService) : IAuthService
{
    private const int PasswordMinLength = 6;
    private const int PasswordMaxLength = 128;

    /// <inheritdoc />
    public async Task<AuthResult> LoginAsync(LoginCommand command, CancellationToken ct)
    {
        var email = Email.Create(command.Email);
        var user = await users.FindByEmailAsync(email.Value, ct);

        if (user is null || !passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            throw new UnauthorizedException(Messages.WrongCredentials);
        }

        var (token, _) = tokenService.CreateAccessToken(user.Id);
        return new AuthResult(user, token);
    }

    /// <inheritdoc />
    public async Task<AuthResult> RegisterAsync(RegisterCommand command, CancellationToken ct)
    {
        var errors = new Dictionary<string, string[]>();

        PersonName? name = null;
        Email? email = null;

        try
        {
            name = PersonName.Create(command.Name);
        }
        catch (DomainValidationException exception)
        {
            Merge(errors, exception.Errors);
        }

        try
        {
            email = Email.Create(command.Email);
        }
        catch (DomainValidationException exception)
        {
            Merge(errors, exception.Errors);
        }

        var password = command.Password ?? string.Empty;
        if (password.Length == 0)
        {
            errors["password"] = [ValidationTexts.Required];
        }
        else if (password.Length < PasswordMinLength)
        {
            errors["password"] = [ValidationTexts.PasswordMin];
        }
        else if (password.Length > PasswordMaxLength)
        {
            errors["password"] = [ValidationTexts.PasswordMax];
        }

        if (errors.Count > 0)
        {
            throw DomainValidationException.InvalidForm(errors);
        }

        if (await users.ExistsByEmailAsync(email!.Value, ct))
        {
            // Контракт мока: занятый email — 400 с этим текстом (не 409).
            throw new DomainValidationException(Messages.EmailTaken);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email.Value,
            PasswordHash = passwordHasher.Hash(password),
            Name = name!.Value,
            AvatarUrl = null,
        };

        await users.AddAsync(user, ct);

        var (token, _) = tokenService.CreateAccessToken(user.Id);
        return new AuthResult(user, token);
    }

    /// <inheritdoc />
    public async Task<User> GetCurrentUserAsync(Guid userId, CancellationToken ct) =>
        await users.FindByIdAsync(userId, ct)
        ?? throw new UnauthorizedException(Messages.Unauthorized);

    /// <summary>Переносит ошибки поля; последний текст перезаписывает предыдущий.</summary>
    private static void Merge(
        Dictionary<string, string[]> target,
        IReadOnlyDictionary<string, string[]> source)
    {
        foreach (var (field, messages) in source)
        {
            target[field] = messages;
        }
    }
}
