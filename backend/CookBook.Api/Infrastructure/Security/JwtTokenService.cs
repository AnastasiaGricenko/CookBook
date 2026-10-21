using System.Text;
using CookBook.Api.Domain.Security;
using CookBook.Api.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CookBook.Api.Infrastructure.Security;

/// <summary>
/// Выдача JWT (HS256). Claims: sub = идентификатор пользователя, iss = Auth:Issuer.
/// Срок жизни берётся из Auth:AccessTtlMinutes.
/// </summary>
public sealed class JwtTokenService(IOptions<AuthOptions> options) : ITokenService
{
    /// <inheritdoc />
    public (string Token, DateTime ExpiresAt) CreateAccessToken(Guid userId)
    {
        var auth = options.Value;
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(auth.AccessTtlMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = auth.Issuer,
            Subject = new System.Security.Claims.ClaimsIdentity(
            [
                new System.Security.Claims.Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            ]),
            NotBefore = now,
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(auth.JwtKey)),
                SecurityAlgorithms.HmacSha256),
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return (token, expiresAt);
    }
}
