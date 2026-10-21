using CookBook.Api.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace CookBook.Api.Presentation.Controllers;

/// <summary>
/// Хелпер работы с httpOnly-cookie токена. Cookie недоступна из JavaScript,
/// SameSite=Strict, Secure — вне Development.
/// </summary>
public sealed class AuthCookie(IOptions<AuthOptions> options, IWebHostEnvironment environment)
{
    /// <summary>Прописывает токен в cookie ответа.</summary>
    public void Write(HttpResponse response, string token)
    {
        var auth = options.Value;

        response.Cookies.Append(auth.CookieName, token, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Secure = !environment.IsDevelopment(),
            Path = "/",
            MaxAge = TimeSpan.FromMinutes(auth.AccessTtlMinutes),
        });
    }

    /// <summary>Удаляет cookie токена (идемпотентно).</summary>
    public void Delete(HttpResponse response)
    {
        response.Cookies.Delete(options.Value.CookieName, new CookieOptions { Path = "/" });
    }
}
