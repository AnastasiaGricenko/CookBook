using CookBook.Api.Presentation.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CookBook.Api.Presentation.Controllers;

/// <summary>Проверка живости сервиса — публичный эндпоинт без побочных эффектов.</summary>
[Route("api/v1/health")]
public sealed class HealthController : ApiControllerBase
{
    /// <summary>Всегда возвращает 200 и {"status":"ok"}.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(HealthDto), StatusCodes.Status200OK)]
    public ActionResult<HealthDto> Get() => Ok(new HealthDto("ok"));
}
