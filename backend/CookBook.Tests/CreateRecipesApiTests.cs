using System.Net;
using System.Net.Http.Json;
using System.Text;
using CookBook.Api.Presentation.Contracts;

namespace CookBook.Tests;

/// <summary>
/// Интеграционные тесты создания рецепта: авторизация, валидация полей,
/// сохранение в каталоге и связь автора с текущей сессией.
/// </summary>
public sealed class CreateRecipesApiTests(CookBookFactory factory) : IClassFixture<CookBookFactory>
{
    private const string DemoUserIdValue = "20000000-0000-0000-0000-000000000001";

    [Fact]
    public async Task Create_AsGuest_Returns401()
    {
        var client = TestHttp.CreateGuest(factory);

        var response = await client.PostAsJsonAsync("/api/v1/recipes", Payload());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("Требуется вход", error.Message);
    }

    [Fact]
    public async Task Create_ValidPayload_Returns201AndRecipe()
    {
        var client = await TestHttp.CreateAuthorizedAsync(factory);
        var payload = Payload();

        var response = await client.PostAsJsonAsync("/api/v1/recipes", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var recipe = await response.Content.ReadFromJsonAsync<RecipeDto>();
        Assert.NotNull(recipe);
        Assert.Equal("Гуляш по-домашнему", recipe.Title);
        Assert.Equal("/assets/plov.png", recipe.ImageUrl);
        Assert.Equal(40, recipe.TimeMinutes);
        Assert.Equal("Сытное второе блюдо.", recipe.Description);
        Assert.Equal("Мария Козлова", recipe.Author);
        Assert.Equal(Guid.Parse(DemoUserIdValue), recipe.AuthorId);
        Assert.Equal(4, recipe.Servings);
        Assert.Equal("Средняя", recipe.Difficulty);
        Assert.Equal(120, recipe.Nutrition.Kcal);
        Assert.Equal(10, recipe.Nutrition.Protein);
        Assert.Equal(5, recipe.Nutrition.Fat);
        Assert.Equal(8, recipe.Nutrition.Carbs);
        Assert.Equal(2, recipe.Ingredients.Count);
        Assert.Equal(2, recipe.Steps.Count);
        Assert.Contains($"/api/v1/recipes/{recipe.Id}", response.Headers.Location!.ToString());

        var stored = await client.GetAsync($"/api/v1/recipes/{recipe.Id}");
        Assert.Equal(HttpStatusCode.OK, stored.StatusCode);

        var storedRecipe = await stored.Content.ReadFromJsonAsync<RecipeDto>();
        Assert.NotNull(storedRecipe);
        Assert.Equal(recipe.Id, storedRecipe.Id);
        Assert.Equal("Гуляш по-домашнему", storedRecipe.Title);
    }

    [Fact]
    public async Task Create_AppearsInSearch()
    {
        var client = await TestHttp.CreateAuthorizedAsync(factory);

        var created = await client.PostAsJsonAsync(
            "/api/v1/recipes",
            Payload(title: "Гуляш с грибами"));
        created.EnsureSuccessStatusCode();

        var page = await client.GetFromJsonAsync<PagedResultDto<RecipeCardDto>>(
            "/api/v1/recipes?query=%D0%93%D1%80%D0%B8%D0%B1%D0%B0%D0%BC%D0%B8&page=1&pageSize=8");

        Assert.NotNull(page);
        Assert.Equal(1, page.Total);
        Assert.Equal("Гуляш с грибами", page.Items.Single().Title);
        Assert.Equal("Мария Козлова", page.Items.Single().Author);
    }

    [Fact]
    public async Task Create_EmptyTitleAndDescription_Returns400()
    {
        var client = await TestHttp.CreateAuthorizedAsync(factory);

        var response = await client.PostAsJsonAsync(
            "/api/v1/recipes",
            Payload(title: "  ", description: "  "));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("Данные заполнены неверно", error.Message);
        Assert.NotNull(error.Errors);
        Assert.Equal("Заполните поле", error.Errors["title"]);
        Assert.Equal("Заполните поле", error.Errors["description"]);
    }

    [Fact]
    public async Task Create_TitleTooLong_Returns400()
    {
        var client = await TestHttp.CreateAuthorizedAsync(factory);

        var response = await client.PostAsJsonAsync(
            "/api/v1/recipes",
            Payload(title: new string('a', 101)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.NotNull(error.Errors);
        Assert.Equal("Название — не более 100 символов", error.Errors["title"]);
    }

    [Fact]
    public async Task Create_InvalidTimeAndServings_Returns400()
    {
        var client = await TestHttp.CreateAuthorizedAsync(factory);

        var response = await client.PostAsJsonAsync(
            "/api/v1/recipes",
            Payload(timeMinutes: 0, servings: 101));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.NotNull(error.Errors);
        Assert.Equal("Время — от 1 до 1440 минут", error.Errors["timeMinutes"]);
        Assert.Equal("Порции — от 1 до 100", error.Errors["servings"]);
    }

    [Fact]
    public async Task Create_InvalidDifficulty_Returns400()
    {
        var client = await TestHttp.CreateAuthorizedAsync(factory);

        var response = await client.PostAsJsonAsync(
            "/api/v1/recipes",
            Payload(difficulty: "Очень сложно"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.NotNull(error.Errors);
        Assert.Equal("Недопустимое значение сложности", error.Errors["difficulty"]);
    }

    [Fact]
    public async Task Create_NoIngredientsOrSteps_Returns400()
    {
        var client = await TestHttp.CreateAuthorizedAsync(factory);

        var response = await client.PostAsJsonAsync(
            "/api/v1/recipes",
            Payload(ingredients: [], steps: []));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.NotNull(error.Errors);
        Assert.Equal("Добавьте хотя бы один ингредиент", error.Errors["ingredients"]);
        Assert.Equal("Добавьте хотя бы один шаг", error.Errors["steps"]);
    }

    [Fact]
    public async Task Create_IngredientWithoutAmount_Returns400()
    {
        var client = await TestHttp.CreateAuthorizedAsync(factory);

        var response = await client.PostAsJsonAsync(
            "/api/v1/recipes",
            Payload(ingredients: [new() { ["name"] = "Мясо", ["amount"] = " " }]));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.NotNull(error.Errors);
        Assert.Equal("Заполните название и количество ингредиентов", error.Errors["ingredients"]);
    }

    [Fact]
    public async Task Create_InvalidNutrition_Returns400()
    {
        var client = await TestHttp.CreateAuthorizedAsync(factory);

        var response = await client.PostAsJsonAsync(
            "/api/v1/recipes",
            Payload(nutrition: new() { ["kcal"] = -1, ["protein"] = 10_001, ["fat"] = 5, ["carbs"] = 8 }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.NotNull(error.Errors);
        Assert.Equal("Значение должно быть от 0 до 10000", error.Errors["kcal"]);
        Assert.Equal("Значение должно быть от 0 до 10000", error.Errors["protein"]);
    }

    [Fact]
    public async Task Create_BlankImageUrl_UsesDefault()
    {
        var client = await TestHttp.CreateAuthorizedAsync(factory);

        var response = await client.PostAsJsonAsync(
            "/api/v1/recipes",
            Payload(imageUrl: "   "));

        response.EnsureSuccessStatusCode();

        var recipe = await response.Content.ReadFromJsonAsync<RecipeDto>();
        Assert.NotNull(recipe);
        Assert.Equal("/assets/borsch.png", recipe.ImageUrl);
    }

    [Fact]
    public async Task Create_NormalizesSeasoningsAndTags()
    {
        var client = await TestHttp.CreateAuthorizedAsync(factory);

        var response = await client.PostAsJsonAsync(
            "/api/v1/recipes",
            Payload(
                seasonings: ["  соль ", "", "Соль", "перец"],
                tags: [" второе ", "мясо", "МЯСО", "  "]));

        response.EnsureSuccessStatusCode();

        var recipe = await response.Content.ReadFromJsonAsync<RecipeDto>();
        Assert.NotNull(recipe);
        Assert.Equal(["соль", "перец"], recipe.Seasonings);
        Assert.Equal(["второе", "мясо"], recipe.Tags);
    }

    [Fact]
    public async Task Create_NullBody_Returns400()
    {
        var client = await TestHttp.CreateAuthorizedAsync(factory);

        var content = new StringContent("null", Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/recipes", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("Данные заполнены неверно", error.Message);
    }

    [Fact]
    public async Task Create_AppearsFirstOnMainPage()
    {
        var client = await TestHttp.CreateAuthorizedAsync(factory);

        var created = await client.PostAsJsonAsync(
            "/api/v1/recipes",
            Payload(title: "Свежий рецепт"));
        created.EnsureSuccessStatusCode();

        var recipe = await created.Content.ReadFromJsonAsync<RecipeDto>();
        Assert.NotNull(recipe);

        var page = await client.GetFromJsonAsync<PagedResultDto<RecipeCardDto>>(
            "/api/v1/recipes?page=1&pageSize=8");

        Assert.NotNull(page);
        Assert.Equal(recipe.Id, page.Items[0].Id);
        Assert.Equal("Свежий рецепт", page.Items[0].Title);
    }

    /// <summary>Валидное тело запроса с возможностью точечно испортить поля.</summary>
    private static Dictionary<string, object?> Payload(
        string title = "Гуляш по-домашнему",
        string? imageUrl = "/assets/plov.png",
        int timeMinutes = 40,
        string description = "Сытное второе блюдо.",
        int servings = 4,
        string difficulty = "Средняя",
        Dictionary<string, int>? nutrition = null,
        List<Dictionary<string, string>>? ingredients = null,
        List<string>? seasonings = null,
        List<Dictionary<string, string>>? steps = null,
        List<string>? tags = null) =>
        new()
        {
            ["title"] = title,
            ["imageUrl"] = imageUrl,
            ["timeMinutes"] = timeMinutes,
            ["description"] = description,
            ["servings"] = servings,
            ["difficulty"] = difficulty,
            ["nutrition"] = nutrition ?? new Dictionary<string, int>
            {
                ["kcal"] = 120,
                ["protein"] = 10,
                ["fat"] = 5,
                ["carbs"] = 8,
            },
            ["ingredients"] = ingredients ?? new List<Dictionary<string, string>>
            {
                new() { ["name"] = "Говядина", ["amount"] = "500 г" },
                new() { ["name"] = "Лук", ["amount"] = "2 шт" },
            },
            ["seasonings"] = seasonings ?? new List<string> { "соль", "перец" },
            ["steps"] = steps ?? new List<Dictionary<string, string>>
            {
                new() { ["title"] = "Обжарка", ["text"] = "Обжарьте мясо до корочки." },
                new() { ["title"] = "Тушение", ["text"] = "Тушите с луком 40 минут." },
            },
            ["tags"] = tags ?? new List<string> { "второе", "мясо" },
        };
}
