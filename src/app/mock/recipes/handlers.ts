/**
 * mock/recipes/handlers.ts — доменные обработчики мок-слоя для домена recipes.
 *
 * Регистрирует методы recipes.list / recipes.get / recipes.similar /
 * recipes.toggleFavorite. Фильтрация и пагинация живут здесь, а не в сервисах:
 * сервис — тонкая обёртка над MockApiClient.call.
 *
 * Гарантии:
 *  - list фильтрует по подстроке названия или автора (регистронезависимо),
 *    пагинация 1-based, размер страницы RECIPES_PAGE_SIZE;
 *  - get/similar отдают снимки из MockDb; отсутствие рецепта — 404;
 *  - toggleFavorite меняет состояние только через транзакцию db.mutate.
 */
import { PagedResult, RECIPES_PAGE_SIZE, Recipe, RecipeCard } from '../../shared/models';
import { MockApiClient } from '../mock-api-client';
import { MockValidationError } from '../mock-error';

/** Тексты отказов recipes — единый источник для страниц. */
export const RECIPE_ERRORS = {
  notFound: 'Рецепт не найден',
} as const;

interface ListParams {
  query: string;
  page: number;
}

interface IdParams {
  id: string;
}

export function registerRecipesHandlers(client: MockApiClient): void {
  client
    .register<ListParams, PagedResult<RecipeCard>>('recipes.list', (db, params) => {
      const query = (params.query ?? '').trim().toLowerCase();
      const page = params.page && params.page > 0 ? params.page : 1;
      const all = db.read().recipes;
      const filtered = query
        ? all.filter(
            recipe =>
              recipe.title.toLowerCase().includes(query) ||
              recipe.author.toLowerCase().includes(query),
          )
        : all;

      const start = (page - 1) * RECIPES_PAGE_SIZE;
      const items = filtered.slice(start, start + RECIPES_PAGE_SIZE).map(toCard);
      return { items, total: filtered.length };
    })
    .register<IdParams, Recipe>('recipes.get', (db, params) => {
      const recipe = db.read().recipes.find(item => item.id === params.id);
      if (!recipe) {
        throw new MockValidationError(404, RECIPE_ERRORS.notFound);
      }
      return recipe;
    })
    .register<IdParams, RecipeCard[]>('recipes.similar', (db, params) => {
      return db
        .read()
        .recipes.filter(recipe => recipe.id !== params.id)
        .slice(0, 4)
        .map(toCard);
    })
    .register<IdParams, { isFavorite: boolean }>('recipes.toggleFavorite', (db, params) => {
      return db.mutate(data => {
        const recipe = data.recipes.find(item => item.id === params.id);
        if (!recipe) {
          throw new MockValidationError(404, RECIPE_ERRORS.notFound);
        }
        recipe.isFavorite = !recipe.isFavorite;
        return { isFavorite: recipe.isFavorite };
      });
    });
}

/** Проекция полного рецепта в карточку каталога. */
function toCard(recipe: Recipe): RecipeCard {
  return {
    id: recipe.id,
    title: recipe.title,
    imageUrl: recipe.imageUrl,
    timeMinutes: recipe.timeMinutes,
    kcal: recipe.nutrition.kcal,
    author: recipe.author,
  };
}
