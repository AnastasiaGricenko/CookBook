using System.Text.Json;
using CookBook.Api.Domain;
using CookBook.Api.Presentation.Contracts;
using CookBook.Api.Presentation.Mapping;

namespace CookBook.Api.Presentation.Middleware;

/// <summary>
/// Единая обработка отказов: доменные и инфраструктурные исключения превращаются
/// в ErrorResponse с корректным кодом. Стектрейсы и внутренние детали наружу не уходят.
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    /// <summary>Ловит исключения обработчика и формирует ответ.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Клиент ушёл — отвечать некому, отвечать и не нужно.
            logger.LogDebug("Запрос отменён клиентом: {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }
        catch (DomainValidationException exception)
        {
            var errors = exception.Errors.Count == 0
                ? null
                : exception.Errors.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.FirstOrDefault() ?? string.Empty);

            await WriteErrorAsync(context, StatusCodes.Status400BadRequest, exception.Message, errors);
        }
        catch (UnauthorizedException exception)
        {
            await WriteErrorAsync(context, StatusCodes.Status401Unauthorized, exception.Message);
        }
        catch (NotFoundException exception)
        {
            await WriteErrorAsync(context, StatusCodes.Status404NotFound, exception.Message);
        }
        catch (BadHttpRequestException)
        {
            await WriteErrorAsync(context, StatusCodes.Status400BadRequest, ValidationTexts.InvalidForm);
        }
        catch (JsonException exception)
        {
            logger.LogDebug(exception, "Не удалось разобрать тело запроса");
            await WriteErrorAsync(context, StatusCodes.Status400BadRequest, ValidationTexts.InvalidForm);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Необработанная ошибка при обработке {Method} {Path}",
                context.Request.Method, context.Request.Path);
            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, Messages.InternalError);
        }
    }

    private static async Task WriteErrorAsync(
        HttpContext context,
        int statusCode,
        string message,
        IReadOnlyDictionary<string, string>? errors = null)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        var body = new ErrorResponse(message, errors);
        await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonDefaults.Options));
    }
}
