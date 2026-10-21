using System.Text.Json.Serialization;

namespace CookBook.Api.Presentation.Contracts;

/// <summary>
/// Единый формат отказа API. errors присутствует только у 400 валидации формы
/// и имеет форму "поле → одна строка" (требование типа ApiError клиента).
/// У остальных отказов поле отсутствует вовсе.
/// </summary>
public sealed record ErrorResponse(
    string Message,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyDictionary<string, string>? Errors = null);
