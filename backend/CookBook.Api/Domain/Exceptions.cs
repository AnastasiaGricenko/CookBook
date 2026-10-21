namespace CookBook.Api.Domain;

/// <summary>
/// Доменные исключения и их единая семантика для HTTP-слоя.
/// </summary>
public sealed class DomainValidationException : Exception
{
    /// <summary>Ошибки по полям: имя поля → тексты (наружу уходит первый текст).</summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    /// <summary>Создаёт ошибку валидации с необязательным набором ошибок по полям.</summary>
    public DomainValidationException(
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null)
        : base(message)
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }

    /// <summary>Ошибка формы: общий текст «Данные заполнены неверно» и ошибки по полям.</summary>
    public static DomainValidationException InvalidForm(
        IReadOnlyDictionary<string, string[]> errors) =>
        new(ValidationTexts.InvalidForm, errors);
}

/// <summary>Отказ аутентификации/авторизации — HTTP 401.</summary>
public sealed class UnauthorizedException(string message) : Exception(message);

/// <summary>Запрошенный ресурс не найден — HTTP 404.</summary>
public sealed class NotFoundException(string message) : Exception(message);

/// <summary>Внутренний хелпер домена: собрать ошибку одного поля.</summary>
internal static class ValidationErrors
{
    /// <summary>Возвращает словарь с единственной ошибкой указанного поля.</summary>
    public static IReadOnlyDictionary<string, string[]> Field(string field, string message) =>
        new Dictionary<string, string[]> { [field] = [message] };
}
