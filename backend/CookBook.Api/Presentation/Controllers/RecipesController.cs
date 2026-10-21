using CookBook.Api.Domain.Models;
using CookBook.Api.Domain.Services;
using CookBook.Api.Presentation.Contracts;
using CookBook.Api.Presentation.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CookBook.Api.Presentation.Controllers;

/// <summary>
/// Каталог рецептов: список, карточка, похожие и создание.
/// Список/карточка/похожие публичны; создание — только для владельца сессии.
/// </summary>
[Route("api/v1/recipes")]
public sealed class RecipesController(IRecipeService recipeService) : ApiControllerBase
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 8;

    /// <summary>Поиск по названию/автору с постраничной выдачей карточек.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResultDto<RecipeCardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResultDto<RecipeCardDto>>> List(
        [FromQuery] string? query,
        [FromQuery] string? page,
        [FromQuery] string? pageSize)
    {
        var pageNumber = ParseOptionalInt(page, DefaultPage);
        var size = ParseOptionalInt(pageSize, DefaultPageSize);

        var result = await recipeService.ListAsync(
            new RecipeListQuery(query ?? string.Empty, pageNumber, size),
            HttpContext.RequestAborted);

        var items = result.Items.Select(DtoMapper.ToDto).ToList();
        return Ok(new PagedResultDto<RecipeCardDto>(items, result.Total));
    }

    /// <summary>Полная карточка рецепта.</summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RecipeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipeDto>> Get(string id)
    {
        var recipeId = ParseId(id);

        var recipe = await recipeService.GetAsync(recipeId, HttpContext.RequestAborted);

        return Ok(DtoMapper.ToDto(recipe));
    }

    /// <summary>До 4 похожих рецептов, кроме текущего.</summary>
    [HttpGet("{id}/similar")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<RecipeCardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<RecipeCardDto>>> Similar(string id)
    {
        var recipeId = ParseId(id);

        var similar = await recipeService.GetSimilarAsync(
            recipeId,
            HttpContext.RequestAborted);

        var cards = similar.Select(DtoMapper.ToDto).ToList();
        return Ok(cards);
    }

    /// <summary>
    /// Создаёт рецепт от имени текущего пользователя. Автор берётся из claims,
    /// а не из тела (анти-mass-assignment); для гостя — 401.
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(RecipeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RecipeDto>> Create([FromBody] CreateRecipeRequest? request)
    {
        var body = Body(request);
        var nutrition = body.Nutrition;

        var command = new CreateRecipeCommand(
            body.Title,
            body.ImageUrl,
            body.TimeMinutes ?? 0,
            body.Description,
            body.Servings ?? 0,
            body.Difficulty,
            nutrition?.Kcal ?? 0,
            nutrition?.Protein ?? 0,
            nutrition?.Fat ?? 0,
            nutrition?.Carbs ?? 0,
            body.Ingredients?
                .Select(item => new RecipeIngredientInput(item?.Name, item?.Amount))
                .ToList(),
            body.Seasonings,
            body.Steps?
                .Select(item => new RecipeStepInput(item?.Title, item?.Text))
                .ToList(),
            body.Tags);

        var recipe = await recipeService.CreateAsync(command, CurrentUserId, HttpContext.RequestAborted);

        return CreatedAtAction(
            nameof(Get),
            new { id = recipe.Id },
            DtoMapper.ToDto(recipe));
    }
}
