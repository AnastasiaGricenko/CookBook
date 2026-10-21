using System.Reflection;
using System.Text;
using System.Text.Json;
using CookBook.Api.Domain;
using CookBook.Api.Infrastructure.Options;
using CookBook.Api.Presentation.Controllers;
using CookBook.Api.Presentation.Contracts;
using CookBook.Api.Presentation.Mapping;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace CookBook.Api.Presentation;

/// <summary>
/// Composition Root HTTP-слоя: контроллеры и JSON, Swagger, CORS, JWT-аутентификация
/// с токеном из httpOnly-cookie и единым ответом на отказ.
/// </summary>
public static class PresentationDependencyInjection
{
    /// <summary>Имя политики CORS для дев-фронтенда.</summary>
    public const string CorsPolicyName = "frontend";

    private const long MaxRequestBodySize = 1024 * 1024;

    /// <summary>Регистрирует презентационный слой.</summary>
    public static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                // camelCase — контракт клиента. WhenWritingNull глобально не включаем:
                // avatarUrl: null должен присутствовать в ответе.
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });

        // Валидацию выполняют контроллеры и домен, а не авто-ProblemDetails.
        services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        services.Configure<KestrelServerOptions>(options =>
            options.Limits.MaxRequestBodySize = MaxRequestBodySize);

        if (environment.IsDevelopment())
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "CookBook API",
                    Version = "v1",
                    Description = "REST API каталога рецептов. Аутентификация — JWT в httpOnly cookie access_token",
                });

                var xmlPath = Path.Combine(
                    AppContext.BaseDirectory,
                    $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");

                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            });
        }

        var allowedOrigins = configuration
            .GetSection(CorsOptions.SectionName)
            .Get<CorsOptions>()?.AllowedOrigins ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, policy =>
            {
                // Пустой список — политика без разрешений: dev ходит через proxy того же origin.
                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowCredentials()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
            });
        });

        services.TryAddScoped<AuthCookie>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<Microsoft.Extensions.Options.IOptions<AuthOptions>>((jwt, authOptions) =>
            {
                var auth = authOptions.Value;
                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(auth.JwtKey));

                // Токен кладём в cookie, поэтому стандартный claim type mapping выключен:
                // идентификатор читается из claim "sub".
                jwt.MapInboundClaims = false;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = auth.Issuer,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = JwtRegisteredClaimNames.Sub,
                };

                jwt.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Cookies[auth.CookieName];
                        if (!string.IsNullOrEmpty(token))
                        {
                            context.Token = token;
                        }

                        return Task.CompletedTask;
                    },
                    OnChallenge = async context =>
                    {
                        // Подавляем стандартный ответ: клиент ждёт единый ErrorResponse.
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json; charset=utf-8";

                        var body = new ErrorResponse(Messages.Unauthorized);
                        await context.Response.WriteAsync(
                            JsonSerializer.Serialize(body, JsonDefaults.Options));
                    },
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("CookBook.Api.Authentication");

                        logger.LogWarning(
                            "Не удалось проверить токен доступа: {Reason}",
                            context.Exception.Message);

                        return Task.CompletedTask;
                    },
                };
            });

        return services;
    }
}
