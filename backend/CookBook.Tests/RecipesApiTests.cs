using System.Net;
using System.Net.Http.Json;
using CookBook.Api.Infrastructure.InMemory.Seed;
using CookBook.Api.Presentation.Contracts;

namespace CookBook.Tests;

/// <summary>Интеграционные тесты каталога рецептов от имени гостя.</summary>
public sealed class RecipesApiTests(CookBookFactory factory) : IClassFixture<CookBookFactory>
{
    private static readonly Guid CarbonaraId = SeedData.RecipeId(1);
    private static readonly Guid BorschId = SeedData.RecipeId(2);

    [Fact]
    public async Task List_ReturnsEightCardsInSeedOrder()
    {
        var client = TestHttp.CreateGuest(factory);

        var page = await GetPageAsync(client, string.Empty, 1, 8);

        Assert.Equal(8, page.Total);
        Assert.Equal(8, page.Items.Count);
        Assert.Equal(
            ["Паста Карбонара", "Борщ классический", "Плов узбекский", "Оливье", "Блины", "Шашлык из свинины", "Торт Наполеон", "Солянка мясная"],
            page.Items.Select(card => card.Title));

        var card = page.Items[1];
        Assert.Equal(BorschId, card.Id);
        Assert.Equal("/assets/borsch.png", card.ImageUrl);
        Assert.Equal(90, card.TimeMinutes);
        Assert.Equal(180, card.Kcal);
        Assert.Equal("Мария Козлова", card.Author);
    }

    [Fact]
    public async Task List_QueryFiltersByTitle()
    {
        var client = TestHttp.CreateGuest(factory);

        var page = await GetPageAsync(client, "борщ", 1, 8);

        Assert.Equal(1, page.Total);
        Assert.Equal("Борщ классический", page.Items.Single().Title);
    }

    [Fact]
    public async Task List_QueryFiltersByAuthorIgnoringCase()
    {
        var client = TestHttp.CreateGuest(factory);

        var page = await GetPageAsync(client, "КОЗЛОВА", 1, 8);

        Assert.Equal(8, page.Total);
    }

    [Fact]
    public async Task List_UnknownQuery_ReturnsEmptyPage()
    {
        var client = TestHttp.CreateGuest(factory);

        var page = await GetPageAsync(client, "qqq", 1, 8);

        Assert.Equal(0, page.Total);
        Assert.Empty(page.Items);
    }

    [Fact]
    public async Task List_Pagination_ReturnsRequestedSlice()
    {
        var client = TestHttp.CreateGuest(factory);

        var page = await GetPageAsync(client, string.Empty, 2, 3);

        Assert.Equal(8, page.Total);
        Assert.Equal(3, page.Items.Count);
        Assert.Equal(
            ["Оливье", "Блины", "Шашлык из свинины"],
            page.Items.Select(card => card.Title));
    }

    [Fact]
    public async Task List_PageBeyondRange_ReturnsEmptyItemsWithTotal()
    {
        var client = TestHttp.CreateGuest(factory);

        var page = await GetPageAsync(client, string.Empty, 10, 3);

        Assert.Equal(8, page.Total);
        Assert.Empty(page.Items);
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("pageSize=abc")]
    [InlineData("pageSize=0")]
    [InlineData("pageSize=51")]
    public async Task List_InvalidPagination_Returns400(string queryString)
    {
        var client = TestHttp.CreateGuest(factory);

        var response = await client.GetAsync($"/api/v1/recipes?{queryString}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("Некорректные параметры пагинации", error.Message);
    }

    [Fact]
    public async Task Get_ReturnsFullRecipe()
    {
        var client = TestHttp.CreateGuest(factory);

        var response = await client.GetAsync($"/api/v1/recipes/{BorschId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var recipe = await response.Content.ReadFromJsonAsync<RecipeDto>();
        Assert.NotNull(recipe);
        Assert.Equal("Борщ классический", recipe.Title);
        Assert.Equal("Средняя", recipe.Difficulty);
        Assert.Equal(180, recipe.Nutrition.Kcal);
        Assert.Equal(180, recipe.Kcal);
        Assert.Equal(6, recipe.Servings);
        Assert.Equal(5, recipe.Ingredients.Count);
        Assert.Equal("Говядина", recipe.Ingredients[0].Name);
        Assert.Equal(4, recipe.Steps.Count);
        Assert.Equal(["суп", "украинская", "классика"], recipe.Tags);
        Assert.Equal("/assets/borsch.png", recipe.ImageUrl);
    }

    [Fact]
    public async Task Get_NotGuid_Returns400()
    {
        var client = TestHttp.CreateGuest(factory);

        var response = await client.GetAsync("/api/v1/recipes/not-a-guid");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("Некорректный идентификатор", error.Message);
    }

    [Fact]
    public async Task Get_Missing_Returns404()
    {
        var client = TestHttp.CreateGuest(factory);

        var response = await client.GetAsync($"/api/v1/recipes/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("Рецепт не найден", error.Message);
    }

    [Fact]
    public async Task Similar_ReturnsFourCardsWithoutCurrent()
    {
        var client = TestHttp.CreateGuest(factory);

        var response = await client.GetAsync($"/api/v1/recipes/{CarbonaraId}/similar");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var cards = await response.Content.ReadFromJsonAsync<List<RecipeCardDto>>();
        Assert.NotNull(cards);
        Assert.Equal(4, cards.Count);
        Assert.Equal(
            ["Борщ классический", "Плов узбекский", "Оливье", "Блины"],
            cards.Select(card => card.Title));
        Assert.DoesNotContain(cards, card => card.Id == CarbonaraId);
    }

    private static async Task<PagedResultDto<RecipeCardDto>> GetPageAsync(
        HttpClient client,
        string query,
        int page,
        int pageSize)
    {
        var response = await client.GetAsync(
            $"/api/v1/recipes?query={Uri.EscapeDataString(query)}&page={page}&pageSize={pageSize}");

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<RecipeCardDto>>();
        Assert.NotNull(result);
        return result;
    }
}
