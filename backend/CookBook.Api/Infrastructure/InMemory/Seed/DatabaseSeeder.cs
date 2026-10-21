using CookBook.Api.Domain.Repositories;
using CookBook.Api.Domain.Security;

namespace CookBook.Api.Infrastructure.InMemory.Seed;

/// <summary>
/// Сид демо-данных при старте. Работает через те же интерфейсы репозиториев, что и
/// приложение, поэтому замена хранилища его не ломает. Идемпотентен в рамках процесса.
/// </summary>
public sealed class DatabaseSeeder(
    IUserRepository users,
    IRecipeRepository recipes,
    IPasswordHasher passwordHasher)
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _completed;

    /// <summary>Заполняет заглушку БД демо-данными; повторный вызов ничего не делает.</summary>
    public async Task SeedAsync(CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct);
        try
        {
            if (_completed)
            {
                return;
            }

            if (await users.FindByEmailAsync(SeedData.DemoEmail, ct) is null)
            {
                // Открытый пароль живёт только здесь и сразу превращается в хеш.
                var demoUser = SeedData.CreateDemoUser(passwordHasher.Hash(SeedData.DemoPassword));
                await users.AddAsync(demoUser, ct);
            }

            var stored = await recipes.GetAllAsync(ct);
            var storedIds = stored.Select(recipe => recipe.Id).ToHashSet();

            foreach (var recipe in SeedData.CreateRecipes())
            {
                if (storedIds.Contains(recipe.Id))
                {
                    continue;
                }

                await recipes.AddAsync(recipe, ct);
            }

            _completed = true;
        }
        finally
        {
            _gate.Release();
        }
    }
}
