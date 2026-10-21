using CookBook.Api.Domain.Entities;
using CookBook.Api.Domain.Models;
using CookBook.Api.Domain.Repositories;
using CookBook.Api.Domain.ValueObjects;

namespace CookBook.Api.Domain.Services;

/// <summary>
/// Каталог рецептов: поиск, пагинация и создание новых рецептов.
/// Сервис не знает о HTTP и о конкретном хранилище — только интерфейсы домена.
/// </summary>
public sealed class RecipeService(
    IRecipeRepository recipes,
    IUserRepository users) : IRecipeService
{
    private const int MaxPageSize = 50;
    private const int SimilarLimit = 4;

    private const int MinTimeMinutes = 1;
    private const int MaxTimeMinutes = 1440;
    private const int MinServings = 1;
    private const int MaxServings = 100;
    private const int MaxDescriptionLength = 1000;
    private const int MaxImageUrlLength = 500;
    private const int MaxIngredients = 50;
    private const int MaxSteps = 50;
    private const int MaxSeasonings = 20;
    private const int MaxTags = 20;
    private const int MaxListItemLength = 50;

    /// <inheritdoc />
    public async Task<PagedResult<RecipeCard>> ListAsync(
        RecipeListQuery query,
        CancellationToken ct)
    {
        ValidatePagination(query.Page, query.PageSize);

        var all = (await recipes.GetAllAsync(ct))
            .OrderByDescending(recipe => recipe.CreatedAt)
            .ToList();
        var search = (query.Query ?? string.Empty).Trim();

        var filtered = string.IsNullOrEmpty(search)
            ? all
            : all.Where(recipe =>
                    recipe.Title.Value.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                    || recipe.AuthorName.Contains(search, StringComparison.InvariantCultureIgnoreCase))
                .ToList();

        var total = filtered.Count;
        var items = filtered
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var cards = items.Select(ToCard).ToList();

        return new PagedResult<RecipeCard>(cards, total);
    }

    /// <inheritdoc />
    public async Task<Recipe> GetAsync(Guid id, CancellationToken ct) =>
        await recipes.FindByIdAsync(id, ct)
        ?? throw new NotFoundException(Messages.RecipeNotFound);

    /// <inheritdoc />
    public async Task<IReadOnlyList<RecipeCard>> GetSimilarAsync(Guid id, CancellationToken ct)
    {
        _ = await recipes.FindByIdAsync(id, ct)
            ?? throw new NotFoundException(Messages.RecipeNotFound);

        var all = (await recipes.GetAllAsync(ct))
            .OrderByDescending(recipe => recipe.CreatedAt)
            .ToList();

        return all
            .Where(recipe => recipe.Id != id)
            .Take(SimilarLimit)
            .Select(ToCard)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<Recipe> CreateAsync(
        CreateRecipeCommand command,
        Guid authorId,
        CancellationToken ct)
    {
        var errors = new Dictionary<string, string[]>();

        RecipeTitle? title = null;
        try
        {
            title = RecipeTitle.Create(command.Title);
        }
        catch (DomainValidationException exception)
        {
            Merge(errors, exception.Errors);
        }

        var difficulty = Difficulty.Easy;
        try
        {
            difficulty = DifficultyMapper.Parse(command.Difficulty);
        }
        catch (DomainValidationException exception)
        {
            Merge(errors, exception.Errors);
        }

        NutritionFacts? nutrition = null;
        try
        {
            nutrition = NutritionFacts.Create(command.Kcal, command.Protein, command.Fat, command.Carbs);
        }
        catch (DomainValidationException exception)
        {
            Merge(errors, exception.Errors);
        }

        if (command.TimeMinutes is < MinTimeMinutes or > MaxTimeMinutes)
        {
            errors["timeMinutes"] = [ValidationTexts.TimeOutOfRange];
        }

        if (command.Servings is < MinServings or > MaxServings)
        {
            errors["servings"] = [ValidationTexts.ServingsOutOfRange];
        }

        var description = (command.Description ?? string.Empty).Trim();
        if (description.Length == 0)
        {
            errors["description"] = [ValidationTexts.Required];
        }
        else if (description.Length > MaxDescriptionLength)
        {
            errors["description"] = [ValidationTexts.DescriptionMax];
        }

        var imageUrl = (command.ImageUrl ?? string.Empty).Trim();
        if (imageUrl.Length == 0)
        {
            imageUrl = CreateRecipeCommand.DefaultImageUrl;
        }
        else if (imageUrl.Length > MaxImageUrlLength)
        {
            errors["imageUrl"] = [ValidationTexts.ImageUrlMax];
        }

        var ingredients = NormalizeIngredients(command.Ingredients, errors);
        var steps = NormalizeSteps(command.Steps, errors);
        var seasonings = NormalizeList(
            command.Seasonings, "seasonings", MaxSeasonings, ValidationTexts.SeasoningsMax, errors);
        var tags = NormalizeList(
            command.Tags, "tags", MaxTags, ValidationTexts.TagsMax, errors);

        if (errors.Count > 0)
        {
            throw DomainValidationException.InvalidForm(errors);
        }

        var author = await users.FindByIdAsync(authorId, ct)
            ?? throw new UnauthorizedException(Messages.Unauthorized);

        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            Title = title!,
            ImageUrl = imageUrl,
            TimeMinutes = command.TimeMinutes,
            Description = description,
            AuthorId = author.Id,
            AuthorName = author.Name,
            // У зарегистрированных через форму аватара нет — подставляем детерминированный.
            AuthorAvatar = author.AvatarUrl ?? $"https://i.pravatar.cc/150?u={author.Id}",
            Servings = command.Servings,
            Difficulty = difficulty,
            Nutrition = nutrition!,
            Ingredients = ingredients!,
            Seasonings = seasonings,
            Steps = steps!,
            Tags = tags,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        return await recipes.AddAsync(recipe, ct);
    }

    /// <summary>Проверяет границы пагинации; размер страницы — не более 50.</summary>
    private static void ValidatePagination(int page, int pageSize)
    {
        if (page < 1 || pageSize < 1 || pageSize > MaxPageSize)
        {
            throw new DomainValidationException(ValidationTexts.InvalidPagination);
        }
    }

    private static RecipeCard ToCard(Recipe recipe) => new(
        recipe.Id,
        recipe.Title.Value,
        recipe.ImageUrl,
        recipe.TimeMinutes,
        recipe.Nutrition.Kcal,
        recipe.AuthorName);

    /// <summary>Нормализует ингредиенты: обязательны минимум один и оба поля.</summary>
    private static List<RecipeIngredient>? NormalizeIngredients(
        IReadOnlyList<RecipeIngredientInput>? input,
        Dictionary<string, string[]> errors)
    {
        var items = input ?? [];

        if (items.Count == 0)
        {
            errors["ingredients"] = [ValidationTexts.IngredientsRequired];
            return null;
        }

        if (items.Count > MaxIngredients)
        {
            errors["ingredients"] = [ValidationTexts.IngredientsMax];
            return null;
        }

        if (items.Any(item =>
                string.IsNullOrWhiteSpace(item.Name) || string.IsNullOrWhiteSpace(item.Amount)))
        {
            errors["ingredients"] = [ValidationTexts.IngredientsInvalid];
            return null;
        }

        return items
            .Select(item => new RecipeIngredient
            {
                Name = item.Name!.Trim(),
                Amount = item.Amount!.Trim(),
            })
            .ToList();
    }

    /// <summary>Нормализует шаги: обязательны минимум один и оба поля.</summary>
    private static List<RecipeStep>? NormalizeSteps(
        IReadOnlyList<RecipeStepInput>? input,
        Dictionary<string, string[]> errors)
    {
        var items = input ?? [];

        if (items.Count == 0)
        {
            errors["steps"] = [ValidationTexts.StepsRequired];
            return null;
        }

        if (items.Count > MaxSteps)
        {
            errors["steps"] = [ValidationTexts.StepsMax];
            return null;
        }

        if (items.Any(item =>
                string.IsNullOrWhiteSpace(item.Title) || string.IsNullOrWhiteSpace(item.Text)))
        {
            errors["steps"] = [ValidationTexts.StepsInvalid];
            return null;
        }

        return items
            .Select(item => new RecipeStep
            {
                Title = item.Title!.Trim(),
                Text = item.Text!.Trim(),
            })
            .ToList();
    }

    /// <summary>Trim, удаление пустых и дублей; проверка количества и длины пунктов.</summary>
    private static List<string> NormalizeList(
        IReadOnlyList<string>? input,
        string field,
        int maxCount,
        string tooManyText,
        Dictionary<string, string[]> errors)
    {
        var items = (input ?? [])
            .Select(value => (value ?? string.Empty).Trim())
            .Where(value => value.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (items.Count > maxCount)
        {
            errors[field] = [tooManyText];
            return [];
        }

        if (items.Any(value => value.Length > MaxListItemLength))
        {
            errors[field] = [ValidationTexts.ListItemMax];
            return [];
        }

        return items;
    }

    /// <summary>Переносит ошибки поля; последний текст перезаписывает предыдущий.</summary>
    private static void Merge(
        Dictionary<string, string[]> target,
        IReadOnlyDictionary<string, string[]> source)
    {
        foreach (var (field, messages) in source)
        {
            target[field] = messages;
        }
    }
}
