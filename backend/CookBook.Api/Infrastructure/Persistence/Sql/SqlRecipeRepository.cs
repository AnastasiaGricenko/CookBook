using CookBook.Api.Domain;
using CookBook.Api.Domain.Entities;
using CookBook.Api.Domain.Repositories;
using CookBook.Api.Domain.ValueObjects;
using Npgsql;

namespace CookBook.Api.Infrastructure.Persistence.Sql;

/// <summary>
/// Репозиторий рецептов на сыром SQL (Npgsql/ADO.NET, без ORM и Dapper).
/// Вторая половина доступа к данным: агрегат из шести таблиц читается JOIN-ами
/// и батч-запросами, а запись выполняется в одной транзакции.
/// </summary>
public sealed class SqlRecipeRepository(NpgsqlDataSource dataSource) : IRecipeRepository
{
    // Явный список колонок: SELECT * запрещён условиями лабораторной.
    private const string SelectRecipesSql = """
        SELECT r.id, r.title, r.image_url, r.time_minutes, r.description,
               r.author_id, r.servings, r.difficulty::text AS difficulty,
               r.kcal, r.protein, r.fat, r.carbs, r.created_at,
               u.name AS author_name,
               COALESCE(u.avatar_url, 'https://i.pravatar.cc/150?u=' || u.id::text) AS author_avatar
        FROM recipes AS r
        JOIN users AS u ON u.id = r.author_id
        ORDER BY r.created_at DESC;
        """;

    private const string SelectRecipeByIdSql = """
        SELECT r.id, r.title, r.image_url, r.time_minutes, r.description,
               r.author_id, r.servings, r.difficulty::text AS difficulty,
               r.kcal, r.protein, r.fat, r.carbs, r.created_at,
               u.name AS author_name,
               COALESCE(u.avatar_url, 'https://i.pravatar.cc/150?u=' || u.id::text) AS author_avatar
        FROM recipes AS r
        JOIN users AS u ON u.id = r.author_id
        WHERE r.id = @id;
        """;

    private const string SelectIngredientsSql = """
        SELECT recipe_id, sort_order, name, amount
        FROM recipe_ingredients
        WHERE recipe_id = ANY(@recipe_ids)
        ORDER BY recipe_id, sort_order;
        """;

    private const string SelectStepsSql = """
        SELECT recipe_id, sort_order, title, text
        FROM recipe_steps
        WHERE recipe_id = ANY(@recipe_ids)
        ORDER BY recipe_id, sort_order;
        """;

    private const string SelectTagsSql = """
        SELECT rt.recipe_id, rt.sort_order, t.name
        FROM recipe_tags AS rt
        JOIN tags AS t ON t.id = rt.tag_id
        WHERE rt.recipe_id = ANY(@recipe_ids)
        ORDER BY rt.recipe_id, rt.sort_order;
        """;

    private const string SelectSeasoningsSql = """
        SELECT rs.recipe_id, rs.sort_order, s.name
        FROM recipe_seasonings AS rs
        JOIN seasonings AS s ON s.id = rs.seasoning_id
        WHERE rs.recipe_id = ANY(@recipe_ids)
        ORDER BY rs.recipe_id, rs.sort_order;
        """;

    private const string InsertRecipeSql = """
        INSERT INTO recipes
            (id, author_id, title, image_url, time_minutes, description, servings,
             difficulty, kcal, protein, fat, carbs, created_at)
        VALUES
            (@id, @author_id, @title, @image_url, @time_minutes, @description, @servings,
             @difficulty::difficulty, @kcal, @protein, @fat, @carbs, @created_at);
        """;

    private const string UpdateRecipeSql = """
        UPDATE recipes
        SET title = @title,
            image_url = @image_url,
            time_minutes = @time_minutes,
            description = @description,
            servings = @servings,
            difficulty = @difficulty::difficulty,
            kcal = @kcal,
            protein = @protein,
            fat = @fat,
            carbs = @carbs
        WHERE id = @id;
        """;

    private const string DeleteRecipeChildrenSql = """
        DELETE FROM recipe_tags       WHERE recipe_id = @id;
        DELETE FROM recipe_seasonings WHERE recipe_id = @id;
        DELETE FROM recipe_ingredients WHERE recipe_id = @id;
        DELETE FROM recipe_steps       WHERE recipe_id = @id;
        """;

    /// <inheritdoc />
    public async Task<IReadOnlyList<Recipe>> GetAllAsync(CancellationToken ct)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        return await ReadRecipesAsync(conn, SelectRecipesSql, id: null, ct);
    }

    /// <inheritdoc />
    public async Task<Recipe?> FindByIdAsync(Guid id, CancellationToken ct)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        var recipes = await ReadRecipesAsync(conn, SelectRecipeByIdSql, id, ct);
        return recipes.Count == 0 ? null : recipes[0];
    }

    /// <inheritdoc />
    public async Task<Recipe> AddAsync(Recipe recipe, CancellationToken ct)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);

        await InsertRecipeAsync(conn, tx, recipe, ct);
        await InsertChildrenAsync(conn, tx, recipe, ct);

        await tx.CommitAsync(ct);
        return recipe;
    }

    /// <inheritdoc />
    public async Task<Recipe> UpdateAsync(Recipe recipe, CancellationToken ct)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);

        var updated = await UpdateRecipeAsync(conn, tx, recipe, ct);
        if (updated == 0)
        {
            // Без CommitAsync транзакция откатится при Dispose.
            throw new NotFoundException(Messages.RecipeNotFound);
        }

        await DeleteChildrenAsync(conn, tx, recipe.Id, ct);
        await InsertChildrenAsync(conn, tx, recipe, ct);

        await tx.CommitAsync(ct);
        return recipe;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand("DELETE FROM recipes WHERE id = @id;", conn);
        cmd.Parameters.AddWithValue("id", id);

        // Дочерние строки удаляются каскадом на уровне схемы.
        return await cmd.ExecuteNonQueryAsync(ct) > 0;
    }

    /// <summary>Читает рецепты и одним батчем поднимает все дочерние коллекции.</summary>
    private static async Task<IReadOnlyList<Recipe>> ReadRecipesAsync(
        NpgsqlConnection conn,
        string sql,
        Guid? id,
        CancellationToken ct)
    {
        var recipes = new List<Recipe>();
        var byId = new Dictionary<Guid, Recipe>();

        await using (var cmd = new NpgsqlCommand(sql, conn))
        {
            if (id.HasValue)
            {
                cmd.Parameters.AddWithValue("id", id.Value);
            }

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var recipe = MapRecipe(reader);
                recipes.Add(recipe);
                byId.Add(recipe.Id, recipe);
            }
        }

        // Для пустого результата не отправляем запросы с ANY(пустой массив).
        if (recipes.Count == 0)
        {
            return recipes;
        }

        var ids = recipes.Select(recipe => recipe.Id).ToArray();

        await LoadIngredientsAsync(conn, ids, byId, ct);
        await LoadStepsAsync(conn, ids, byId, ct);
        await LoadTagsAsync(conn, ids, byId, ct);
        await LoadSeasoningsAsync(conn, ids, byId, ct);

        return recipes;
    }

    private static Recipe MapRecipe(NpgsqlDataReader reader)
    {
        var createdAt = new DateTimeOffset(reader.GetFieldValue<DateTime>(12));

        return new Recipe
        {
            Id = reader.GetGuid(0),
            Title = RecipeTitle.Create(reader.GetString(1)),
            ImageUrl = reader.GetString(2),
            TimeMinutes = reader.GetInt32(3),
            Description = reader.GetString(4),
            AuthorId = reader.GetGuid(5),
            Servings = reader.GetInt32(6),
            Difficulty = Enum.Parse<Difficulty>(reader.GetString(7)),
            Nutrition = NutritionFacts.Create(
                reader.GetInt32(8),
                reader.GetInt32(9),
                reader.GetInt32(10),
                reader.GetInt32(11)),
            CreatedAt = createdAt,
            AuthorName = reader.GetString(13),
            AuthorAvatar = reader.GetString(14),
            Ingredients = [],
            Steps = [],
            Tags = [],
            Seasonings = [],
        };
    }

    private static async Task LoadIngredientsAsync(
        NpgsqlConnection conn,
        Guid[] recipeIds,
        IReadOnlyDictionary<Guid, Recipe> recipes,
        CancellationToken ct)
    {
        await using var cmd = new NpgsqlCommand(SelectIngredientsSql, conn);
        cmd.Parameters.AddWithValue("recipe_ids", recipeIds);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            if (!recipes.TryGetValue(reader.GetGuid(0), out var recipe))
            {
                continue;
            }

            recipe.Ingredients.Add(new RecipeIngredient
            {
                Name = reader.GetString(2),
                Amount = reader.GetString(3),
            });
        }
    }

    private static async Task LoadStepsAsync(
        NpgsqlConnection conn,
        Guid[] recipeIds,
        IReadOnlyDictionary<Guid, Recipe> recipes,
        CancellationToken ct)
    {
        await using var cmd = new NpgsqlCommand(SelectStepsSql, conn);
        cmd.Parameters.AddWithValue("recipe_ids", recipeIds);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            if (!recipes.TryGetValue(reader.GetGuid(0), out var recipe))
            {
                continue;
            }

            recipe.Steps.Add(new RecipeStep
            {
                Title = reader.GetString(2),
                Text = reader.GetString(3),
            });
        }
    }

    private static async Task LoadTagsAsync(
        NpgsqlConnection conn,
        Guid[] recipeIds,
        IReadOnlyDictionary<Guid, Recipe> recipes,
        CancellationToken ct)
    {
        await using var cmd = new NpgsqlCommand(SelectTagsSql, conn);
        cmd.Parameters.AddWithValue("recipe_ids", recipeIds);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            if (recipes.TryGetValue(reader.GetGuid(0), out var recipe))
            {
                recipe.Tags.Add(reader.GetString(2));
            }
        }
    }

    private static async Task LoadSeasoningsAsync(
        NpgsqlConnection conn,
        Guid[] recipeIds,
        IReadOnlyDictionary<Guid, Recipe> recipes,
        CancellationToken ct)
    {
        await using var cmd = new NpgsqlCommand(SelectSeasoningsSql, conn);
        cmd.Parameters.AddWithValue("recipe_ids", recipeIds);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            if (recipes.TryGetValue(reader.GetGuid(0), out var recipe))
            {
                recipe.Seasonings.Add(reader.GetString(2));
            }
        }
    }

    private static async Task InsertRecipeAsync(
        NpgsqlConnection conn,
        NpgsqlTransaction tx,
        Recipe recipe,
        CancellationToken ct)
    {
        await using var cmd = new NpgsqlCommand(InsertRecipeSql, conn, tx);
        AddRecipeParameters(cmd, recipe);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task<int> UpdateRecipeAsync(
        NpgsqlConnection conn,
        NpgsqlTransaction tx,
        Recipe recipe,
        CancellationToken ct)
    {
        await using var cmd = new NpgsqlCommand(UpdateRecipeSql, conn, tx);
        AddRecipeParameters(cmd, recipe);
        return await cmd.ExecuteNonQueryAsync(ct);
    }

    private static void AddRecipeParameters(NpgsqlCommand cmd, Recipe recipe)
    {
        cmd.Parameters.AddWithValue("id", recipe.Id);
        cmd.Parameters.AddWithValue("author_id", recipe.AuthorId);
        cmd.Parameters.AddWithValue("title", recipe.Title.Value);
        cmd.Parameters.AddWithValue("image_url", recipe.ImageUrl);
        cmd.Parameters.AddWithValue("time_minutes", recipe.TimeMinutes);
        cmd.Parameters.AddWithValue("description", recipe.Description);
        cmd.Parameters.AddWithValue("servings", recipe.Servings);
        cmd.Parameters.AddWithValue("difficulty", recipe.Difficulty.ToString());
        cmd.Parameters.AddWithValue("kcal", recipe.Nutrition.Kcal);
        cmd.Parameters.AddWithValue("protein", recipe.Nutrition.Protein);
        cmd.Parameters.AddWithValue("fat", recipe.Nutrition.Fat);
        cmd.Parameters.AddWithValue("carbs", recipe.Nutrition.Carbs);
        cmd.Parameters.AddWithValue("created_at", recipe.CreatedAt.UtcDateTime);
    }

    private static async Task DeleteChildrenAsync(
        NpgsqlConnection conn,
        NpgsqlTransaction tx,
        Guid recipeId,
        CancellationToken ct)
    {
        await using var cmd = new NpgsqlCommand(DeleteRecipeChildrenSql, conn, tx);
        cmd.Parameters.AddWithValue("id", recipeId);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task InsertChildrenAsync(
        NpgsqlConnection conn,
        NpgsqlTransaction tx,
        Recipe recipe,
        CancellationToken ct)
    {
        for (var index = 0; index < recipe.Ingredients.Count; index++)
        {
            await using var cmd = new NpgsqlCommand("""
                INSERT INTO recipe_ingredients (recipe_id, sort_order, name, amount)
                VALUES (@recipe_id, @sort_order, @name, @amount);
                """, conn, tx);

            cmd.Parameters.AddWithValue("recipe_id", recipe.Id);
            cmd.Parameters.AddWithValue("sort_order", (short)(index + 1));
            cmd.Parameters.AddWithValue("name", recipe.Ingredients[index].Name);
            cmd.Parameters.AddWithValue("amount", recipe.Ingredients[index].Amount);

            await cmd.ExecuteNonQueryAsync(ct);
        }

        for (var index = 0; index < recipe.Steps.Count; index++)
        {
            await using var cmd = new NpgsqlCommand("""
                INSERT INTO recipe_steps (recipe_id, sort_order, title, text)
                VALUES (@recipe_id, @sort_order, @title, @text);
                """, conn, tx);

            cmd.Parameters.AddWithValue("recipe_id", recipe.Id);
            cmd.Parameters.AddWithValue("sort_order", (short)(index + 1));
            cmd.Parameters.AddWithValue("title", recipe.Steps[index].Title);
            cmd.Parameters.AddWithValue("text", recipe.Steps[index].Text);

            await cmd.ExecuteNonQueryAsync(ct);
        }

        for (var index = 0; index < recipe.Tags.Count; index++)
        {
            var tagId = await UpsertTagAsync(conn, tx, recipe.Tags[index], ct);

            await using var link = new NpgsqlCommand("""
                INSERT INTO recipe_tags (recipe_id, tag_id, sort_order)
                VALUES (@recipe_id, @tag_id, @sort_order);
                """, conn, tx);

            link.Parameters.AddWithValue("recipe_id", recipe.Id);
            link.Parameters.AddWithValue("tag_id", tagId);
            link.Parameters.AddWithValue("sort_order", (short)(index + 1));

            await link.ExecuteNonQueryAsync(ct);
        }

        for (var index = 0; index < recipe.Seasonings.Count; index++)
        {
            var seasoningId = await UpsertSeasoningAsync(conn, tx, recipe.Seasonings[index], ct);

            await using var link = new NpgsqlCommand("""
                INSERT INTO recipe_seasonings (recipe_id, seasoning_id, sort_order)
                VALUES (@recipe_id, @seasoning_id, @sort_order);
                """, conn, tx);

            link.Parameters.AddWithValue("recipe_id", recipe.Id);
            link.Parameters.AddWithValue("seasoning_id", seasoningId);
            link.Parameters.AddWithValue("sort_order", (short)(index + 1));

            await link.ExecuteNonQueryAsync(ct);
        }
    }

    private static async Task<long> UpsertTagAsync(
        NpgsqlConnection conn,
        NpgsqlTransaction tx,
        string name,
        CancellationToken ct)
    {
        await using var cmd = new NpgsqlCommand("""
            INSERT INTO tags (name) VALUES (@name)
            ON CONFLICT (name) DO UPDATE SET name = EXCLUDED.name
            RETURNING id;
            """, conn, tx);

        cmd.Parameters.AddWithValue("name", name);

        return (long)(await cmd.ExecuteScalarAsync(ct)
            ?? throw new InvalidOperationException("Не удалось получить id тега."));
    }

    private static async Task<long> UpsertSeasoningAsync(
        NpgsqlConnection conn,
        NpgsqlTransaction tx,
        string name,
        CancellationToken ct)
    {
        await using var cmd = new NpgsqlCommand("""
            INSERT INTO seasonings (name) VALUES (@name)
            ON CONFLICT (name) DO UPDATE SET name = EXCLUDED.name
            RETURNING id;
            """, conn, tx);

        cmd.Parameters.AddWithValue("name", name);

        return (long)(await cmd.ExecuteScalarAsync(ct)
            ?? throw new InvalidOperationException("Не удалось получить id приправы."));
    }
}
