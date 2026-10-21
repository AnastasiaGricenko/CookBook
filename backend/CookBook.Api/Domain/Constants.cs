namespace CookBook.Api.Domain;

/// <summary>
/// Тексты отказов валидации. Дословно совпадают с сообщениями Angular-валидаторов
/// и контрактом клиента — менять формулировки запрещено.
/// </summary>
public static class ValidationTexts
{
    /// <summary>Поле не заполнено.</summary>
    public const string Required = "Заполните поле";

    /// <summary>Имя слишком короткое.</summary>
    public const string NameMin = "Имя — не менее 2 символов";

    /// <summary>Имя слишком длинное.</summary>
    public const string NameMax = "Имя — не более 100 символов";

    /// <summary>Некорректный формат email.</summary>
    public const string EmailFormat = "Введите корректный email";

    /// <summary>Email слишком длинный.</summary>
    public const string EmailMax = "Email — не более 254 символов";

    /// <summary>Пароль слишком короткий.</summary>
    public const string PasswordMin = "Пароль должен содержать не менее 6 символов";

    /// <summary>Пароль слишком длинный.</summary>
    public const string PasswordMax = "Пароль — не более 128 символов";

    /// <summary>Общий текст ошибки формы.</summary>
    public const string InvalidForm = "Данные заполнены неверно";

    /// <summary>Некорректный идентификатор в маршруте.</summary>
    public const string InvalidId = "Некорректный идентификатор";

    /// <summary>Некорректные параметры пагинации.</summary>
    public const string InvalidPagination = "Некорректные параметры пагинации";

    /// <summary>Название рецепта слишком длинное.</summary>
    public const string TitleMax = "Название — не более 100 символов";

    /// <summary>Описание рецепта слишком длинное.</summary>
    public const string DescriptionMax = "Описание — не более 1000 символов";

    /// <summary>Ссылка на изображение слишком длинная.</summary>
    public const string ImageUrlMax = "Ссылка на изображение — не более 500 символов";

    /// <summary>Время приготовления вне диапазона.</summary>
    public const string TimeOutOfRange = "Время — от 1 до 1440 минут";

    /// <summary>Количество порций вне диапазона.</summary>
    public const string ServingsOutOfRange = "Порции — от 1 до 100";

    /// <summary>У рецепта нет ни одного ингредиента.</summary>
    public const string IngredientsRequired = "Добавьте хотя бы один ингредиент";

    /// <summary>Ингредиент без названия или количества.</summary>
    public const string IngredientsInvalid = "Заполните название и количество ингредиентов";

    /// <summary>Слишком много ингредиентов.</summary>
    public const string IngredientsMax = "Не более 50 ингредиентов";

    /// <summary>У рецепта нет ни одного шага.</summary>
    public const string StepsRequired = "Добавьте хотя бы один шаг";

    /// <summary>Шаг без заголовка или описания.</summary>
    public const string StepsInvalid = "Заполните заголовок и описание шага";

    /// <summary>Слишком много шагов.</summary>
    public const string StepsMax = "Не более 50 шагов";

    /// <summary>Слишком много приправ.</summary>
    public const string SeasoningsMax = "Не более 20 приправ";

    /// <summary>Слишком много тегов.</summary>
    public const string TagsMax = "Не более 20 тегов";

    /// <summary>Слишком длинный элемент списка (приправа/тег).</summary>
    public const string ListItemMax = "Каждый пункт — не более 50 символов";

    /// <summary>Недопустимое значение сложности.</summary>
    public const string DifficultyInvalid = "Недопустимое значение сложности";

    /// <summary>Значение пищевой ценности вне допустимого диапазона.</summary>
    public const string NutritionOutOfRange = "Значение должно быть от 0 до 10000";
}

/// <summary>
/// Пользовательские тексты отказов REST API. Единственный источник для страниц:
/// клиент сравнивает и показывает их дословно.
/// </summary>
public static class Messages
{
    /// <summary>Требуется аутентификация (мок: AUTH_ERRORS.unauthorized).</summary>
    public const string Unauthorized = "Требуется вход";

    /// <summary>Неверная пара email/пароль (мок: AUTH_ERRORS.invalidCredentials).</summary>
    public const string WrongCredentials = "Неверный email или пароль";

    /// <summary>Email уже занят (мок: AUTH_ERRORS.emailTaken).</summary>
    public const string EmailTaken = "Этот email уже зарегистрирован";

    /// <summary>Рецепт не найден (мок: RECIPE_ERRORS.notFound).</summary>
    public const string RecipeNotFound = "Рецепт не найден";

    /// <summary>Неперехваченная ошибка сервера.</summary>
    public const string InternalError = "Внутренняя ошибка сервера";
}
