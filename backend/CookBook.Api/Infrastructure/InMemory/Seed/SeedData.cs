using CookBook.Api.Domain;
using CookBook.Api.Domain.Entities;
using CookBook.Api.Domain.ValueObjects;

namespace CookBook.Api.Infrastructure.InMemory.Seed;

/// <summary>
/// Статические демо-данные сида: пользователь Мария Козлова и 8 рецептов.
/// Данные перенесены дословно из клиентского сида — ни тексты, ни порядок не меняются.
/// </summary>
public static class SeedData
{
    /// <summary>Идентификатор демо-пользователя.</summary>
    public static readonly Guid DemoUserId = Guid.Parse("20000000-0000-0000-0000-000000000001");

    /// <summary>Email демо-пользователя.</summary>
    public const string DemoEmail = "demo@kulibook.ru";

    /// <summary>Пароль демо-пользователя (хранится только хеш).</summary>
    public const string DemoPassword = "demo123";

    /// <summary>Имя демо-пользователя.</summary>
    public const string DemoName = "Мария Козлова";

    /// <summary>Аватар демо-пользователя.</summary>
    public const string DemoAvatar = "https://i.pravatar.cc/150?u=maria";

    /// <summary>Создаёт демо-пользователя с готовым хешем пароля.</summary>
    public static User CreateDemoUser(string passwordHash) => new()
    {
        Id = DemoUserId,
        Email = DemoEmail,
        PasswordHash = passwordHash,
        Name = DemoName,
        AvatarUrl = DemoAvatar,
    };

    /// <summary>Возвращает 8 рецептов в порядке сида (1 → 8).</summary>
    public static IReadOnlyList<Recipe> CreateRecipes() =>
    [
        CreateRecipe(
            number: 1,
            title: "Паста Карбонара",
            imageUrl: "/assets/pasta-karbonara.png",
            timeMinutes: 30,
            description: "Классическая итальянская паста с гуанчиале, яйцами и пармезаном. Простое, но невероятно вкусное блюдо.",
            servings: 2,
            difficulty: Difficulty.Medium,
            nutrition: NutritionFacts.Create(520, 28, 26, 48),
            ingredients:
            [
                Ingredient("Спагетти", "200 г"),
                Ingredient("Гуанчиале", "150 г"),
                Ingredient("Яйца", "3 шт"),
                Ingredient("Пармезан", "50 г"),
                Ingredient("Трюфельное масло", "2 ст. л."),
            ],
            seasonings: ["Чёрный перец", "Соль"],
            steps:
            [
                Step("Подготовка ингредиентов", "Нарежьте гуанчиале кубиками, натрите пармезан, взбейте яйца с сыром."),
                Step("Обжарка гуанчиале", "Обжарьте гуанчиале на среднем огне до хрустящей корочки."),
                Step("Варка пасты", "Отварите спагетти в подсоленной воде до состояния аль денте."),
                Step("Соединение", "Смешайте горячую пасту с гуанчиале, затем добавьте яичную смесь."),
                Step("Подача", "Подавайте сразу, посыпав пармезаном и чёрным перцем."),
            ],
            tags: ["паста", "итальянская", "быстро"]),
        CreateRecipe(
            number: 2,
            title: "Борщ классический",
            imageUrl: "/assets/borsch.png",
            timeMinutes: 90,
            description: "Традиционный украинский борщ со свёклой, капустой и мясом. Насыщенный и ароматный.",
            servings: 6,
            difficulty: Difficulty.Medium,
            nutrition: NutritionFacts.Create(180, 12, 6, 22),
            ingredients:
            [
                Ingredient("Говядина", "500 г"),
                Ingredient("Свёкла", "2 шт"),
                Ingredient("Капуста", "300 г"),
                Ingredient("Картофель", "3 шт"),
                Ingredient("Морковь", "1 шт"),
            ],
            seasonings: ["Лавровый лист", "Соль", "Перец"],
            steps:
            [
                Step("Бульон", "Сварите мясной бульон из говядины."),
                Step("Подготовка овощей", "Нарежьте все овощи."),
                Step("Обжарка", "Обжарьте свёклу с уксусом."),
                Step("Варка", "Добавьте овощи в бульон и варите до готовности."),
            ],
            tags: ["суп", "украинская", "классика"]),
        CreateRecipe(
            number: 3,
            title: "Плов узбекский",
            imageUrl: "/assets/plov.png",
            timeMinutes: 120,
            description: "Настоящий узбекский плов с бараниной, зирой и барбарисом. Рассыпчатый и ароматный.",
            servings: 8,
            difficulty: Difficulty.Hard,
            nutrition: NutritionFacts.Create(450, 25, 18, 52),
            ingredients:
            [
                Ingredient("Баранина", "1 кг"),
                Ingredient("Рис", "1 кг"),
                Ingredient("Морковь", "1 кг"),
                Ingredient("Лук", "500 г"),
                Ingredient("Чеснок", "2 головки"),
            ],
            seasonings: ["Зира", "Барбарис", "Куркума"],
            steps:
            [
                Step("Подготовка", "Нарежьте мясо и овощи."),
                Step("Зирвак", "Обжарьте мясо и овощи в казане."),
                Step("Рис", "Добавьте рис и залейте водой."),
                Step("Томление", "Готовьте на медленном огне 40 минут."),
            ],
            tags: ["плов", "узбекская", "мясо"]),
        CreateRecipe(
            number: 4,
            title: "Оливье",
            imageUrl: "/assets/olivie.png",
            timeMinutes: 45,
            description: "Традиционный новогодний салат с колбасой, овощами и майонезом.",
            servings: 6,
            difficulty: Difficulty.Easy,
            nutrition: NutritionFacts.Create(220, 8, 14, 18),
            ingredients:
            [
                Ingredient("Колбаса", "300 г"),
                Ingredient("Картофель", "3 шт"),
                Ingredient("Яйца", "4 шт"),
                Ingredient("Огурцы", "2 шт"),
                Ingredient("Горошек", "1 банка"),
            ],
            seasonings: ["Соль", "Перец", "Майонез"],
            steps:
            [
                Step("Варка", "Отварите картофель и яйца."),
                Step("Нарезка", "Нарежьте все ингредиенты кубиками."),
                Step("Смешивание", "Смешайте с горошком и заправьте майонезом."),
            ],
            tags: ["салат", "новый год", "классика"]),
        CreateRecipe(
            number: 5,
            title: "Блины",
            imageUrl: "/assets/bliny.png",
            timeMinutes: 30,
            description: "Тонкие ажурные блины на молоке. Идеальны для Масленицы.",
            servings: 4,
            difficulty: Difficulty.Easy,
            nutrition: NutritionFacts.Create(150, 5, 4, 24),
            ingredients:
            [
                Ingredient("Молоко", "500 мл"),
                Ingredient("Яйца", "2 шт"),
                Ingredient("Мука", "200 г"),
                Ingredient("Сахар", "2 ст. л."),
                Ingredient("Масло", "2 ст. л."),
            ],
            seasonings: ["Соль", "Сахар"],
            steps:
            [
                Step("Тесто", "Смешайте все ингредиенты в тесто."),
                Step("Жарка", "Жарьте на раскалённой сковороде с двух сторон."),
            ],
            tags: ["блины", "выпечка", "масленица"]),
        CreateRecipe(
            number: 6,
            title: "Шашлык из свинины",
            imageUrl: "/assets/shashlik.png",
            timeMinutes: 60,
            description: "Сочный шашлык из свиной шеи с луком и специями.",
            servings: 6,
            difficulty: Difficulty.Medium,
            nutrition: NutritionFacts.Create(380, 32, 24, 2),
            ingredients:
            [
                Ingredient("Свиная шея", "1.5 кг"),
                Ingredient("Лук", "3 шт"),
                Ingredient("Лимон", "1 шт"),
            ],
            seasonings: ["Паприка", "Кориандр", "Соль"],
            steps:
            [
                Step("Маринад", "Замаринуйте мясо с луком и специями."),
                Step("Жарка", "Жарьте на мангале до готовности."),
            ],
            tags: ["мясо", "шашлык", "лето"]),
        CreateRecipe(
            number: 7,
            title: "Торт Наполеон",
            imageUrl: "/assets/napoleon.png",
            timeMinutes: 180,
            description: "Классический торт Наполеон с заварным кремом и слоёным тестом.",
            servings: 12,
            difficulty: Difficulty.Hard,
            nutrition: NutritionFacts.Create(320, 6, 18, 36),
            ingredients:
            [
                Ingredient("Мука", "500 г"),
                Ingredient("Масло", "300 г"),
                Ingredient("Молоко", "1 л"),
                Ingredient("Яйца", "4 шт"),
                Ingredient("Сахар", "200 г"),
            ],
            seasonings: ["Ванилин"],
            steps:
            [
                Step("Тесто", "Замесите слоёное тесто."),
                Step("Выпечка", "Выпеките коржи."),
                Step("Крем", "Приготовьте заварной крем."),
                Step("Сборка", "Соберите торт и оставьте пропитаться."),
            ],
            tags: ["торт", "десерт", "выпечка"]),
        CreateRecipe(
            number: 8,
            title: "Солянка мясная",
            imageUrl: "/assets/solyanka.png",
            timeMinutes: 75,
            description: "Наваристая солянка с копчёностями, оливками и лимоном.",
            servings: 6,
            difficulty: Difficulty.Medium,
            nutrition: NutritionFacts.Create(280, 18, 12, 20),
            ingredients:
            [
                Ingredient("Копчёности", "500 г"),
                Ingredient("Лук", "2 шт"),
                Ingredient("Огурцы", "3 шт"),
                Ingredient("Оливки", "100 г"),
                Ingredient("Лимон", "1 шт"),
            ],
            seasonings: ["Лавровый лист", "Перец", "Соль"],
            steps:
            [
                Step("Бульон", "Сварите бульон из копчёностей."),
                Step("Овощи", "Обжарьте овощи."),
                Step("Варка", "Соедините все ингредиенты и варите 30 минут."),
            ],
            tags: ["суп", "мясо", "классика"]),
    ];

    /// <summary>Guid рецепта по номеру сида: 10000000-0000-0000-0000-0000000000NN.</summary>
    public static Guid RecipeId(int number) =>
        Guid.Parse($"10000000-0000-0000-0000-{number:D12}");

    private static Recipe CreateRecipe(
        int number,
        string title,
        string imageUrl,
        int timeMinutes,
        string description,
        int servings,
        Difficulty difficulty,
        NutritionFacts nutrition,
        List<RecipeIngredient> ingredients,
        List<string> seasonings,
        List<RecipeStep> steps,
        List<string> tags) => new()
        {
            Id = RecipeId(number),
            Title = RecipeTitle.Create(title),
            ImageUrl = imageUrl,
            TimeMinutes = timeMinutes,
            Description = description,
            AuthorId = DemoUserId,
            AuthorName = DemoName,
            AuthorAvatar = DemoAvatar,
            Servings = servings,
            Difficulty = difficulty,
            Nutrition = nutrition,
            Ingredients = ingredients,
            Seasonings = seasonings,
            Steps = steps,
            Tags = tags,
        };

    private static RecipeIngredient Ingredient(string name, string amount) =>
        new() { Name = name, Amount = amount };

    private static RecipeStep Step(string title, string text) =>
        new() { Title = title, Text = text };
}
