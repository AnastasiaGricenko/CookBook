using System.Diagnostics;

namespace CookBook.Api.Presentation.Middleware;

/// <summary>
/// Логирование запросов самым внешним middleware: работа ДО обработчика
/// (что пришло) и ПОСЛЕ него (что вернулось и сколько заняло).
/// Тела, пароли, cookie и query-строки не логируются — только метод и путь.
/// </summary>
public sealed class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger)
{
    /// <summary>Оборачивает обработку запроса логами «до» и «после».</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        // ── ДО обработчика: что пришло (без PII).
        logger.LogInformation("→ {Method} {Path}", context.Request.Method, context.Request.Path);

        await next(context);

        // ── ПОСЛЕ обработчика: что вернулось и сколько это заняло.
        stopwatch.Stop();
        logger.LogInformation(
            "← {StatusCode} за {Elapsed} мс",
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }
}
