using CookBook.Api.Infrastructure;
using CookBook.Api.Presentation;
using CookBook.Api.Presentation.Middleware;

// Composition Root: сначала инфраструктура и домен, затем HTTP-слой.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPresentation(builder.Configuration, builder.Environment);

var app = builder.Build();

// Логирование — самый внешний слой (видит обработку целиком),
// обработка исключений — сразу под ним (успевает поймать до ухода ответа).
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(PresentationDependencyInjection.CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

/// <summary>Точка входа для интеграционных тестов (WebApplicationFactory).</summary>
public partial class Program { }
