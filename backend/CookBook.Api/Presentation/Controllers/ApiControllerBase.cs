using System.Globalization;
using System.Security.Claims;
using CookBook.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace CookBook.Api.Presentation.Controllers;

/// <summary>
/// База контроллеров: единая «лёгкая» проверка входа/тела/идентификатора.
/// userId берётся ТОЛЬКО из claims токена (анти-IDOR, анти-mass-assignment).
/// </summary>
[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Идентификатор текущего пользователя из claims токена.</summary>
    protected Guid CurrentUserId
    {
        get
        {
            var raw = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(raw, out var userId))
            {
                throw new UnauthorizedException(Messages.Unauthorized);
            }

            return userId;
        }
    }

    /// <summary>Пустое тело запроса → 400 «Данные заполнены неверно».</summary>
    protected static T Body<T>(T? body) where T : class =>
        body ?? throw DomainValidationException.InvalidForm(new Dictionary<string, string[]>());

    /// <summary>Разбор Guid из маршрута; некорректный → 400 «Некорректный идентификатор».</summary>
    protected static Guid ParseId(string? value)
    {
        if (!Guid.TryParse(value, out var id))
        {
            throw new DomainValidationException(ValidationTexts.InvalidId);
        }

        return id;
    }

    /// <summary>Разбор необязательного целого query-параметра; мусор → 400 пагинации.</summary>
    protected static int ParseOptionalInt(string? value, int defaultValue)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            throw new DomainValidationException(ValidationTexts.InvalidPagination);
        }

        return parsed;
    }
}
