using CookBook.Api.Domain.Models;
using CookBook.Api.Domain.Services;
using CookBook.Api.Presentation.Contracts;
using CookBook.Api.Presentation.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CookBook.Api.Presentation.Controllers;

/// <summary>
/// Аутентификация: регистрация (201), вход (200), выход (204), текущий пользователь (200).
/// Автовход обеспечивается httpOnly-cookie с JWT.
/// </summary>
[Route("api/v1/auth")]
public sealed class AuthController(IAuthService authService, AuthCookie authCookie) : ApiControllerBase
{
    /// <summary>Регистрирует пользователя и сразу выдаёт cookie автовхода.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterRequest? request)
    {
        var body = Body(request);
        var command = new RegisterCommand(
            body.Name ?? string.Empty,
            body.Email ?? string.Empty,
            body.Password ?? string.Empty);

        var result = await authService.RegisterAsync(command, HttpContext.RequestAborted);
        authCookie.Write(Response, result.Token);

        return StatusCode(StatusCodes.Status201Created, DtoMapper.ToDto(result.User));
    }

    /// <summary>Проверяет учётные данные и выдаёт cookie автовхода.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> Login([FromBody] LoginRequest? request)
    {
        var body = Body(request);
        var command = new LoginCommand(body.Email ?? string.Empty, body.Password ?? string.Empty);

        var result = await authService.LoginAsync(command, HttpContext.RequestAborted);
        authCookie.Write(Response, result.Token);

        return Ok(DtoMapper.ToDto(result.User));
    }

    /// <summary>Удаляет cookie токена. Идемпотентен, тела не возвращает.</summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public ActionResult<UserDto> Logout()
    {
        authCookie.Delete(Response);
        return NoContent();
    }

    /// <summary>Возвращает текущего пользователя по cookie.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> Me()
    {
        var user = await authService.GetCurrentUserAsync(CurrentUserId, HttpContext.RequestAborted);
        return Ok(DtoMapper.ToDto(user));
    }
}
