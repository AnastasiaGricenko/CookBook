/**
 * core/services/recipe.service.ts — сервис рецептов.
 *
 * Роль в архитектуре: тонкая обёртка над MockApiClient для домена recipes.
 * Никакой фильтрации/пагинации — это ответственность обработчиков мок-слоя.
 * Потребляется страницами фичи recipes.
 *
 * Гарантии:
 *  - методы возвращают Promise (без потоков RxJS для данных);
 *  - toggleFavorite меняет состояние в мок-БД через транзакцию обработчика.
 */
import { Injectable, inject } from '@angular/core';
import { PagedResult, Recipe, RecipeCard } from '../../shared/models';
import { MockApiClient } from '../../mock/mock-api-client';

export interface RecipeListParams {
  query: string;
  page: number;
}

export interface FavoriteResult {
  isFavorite: boolean;
}

@Injectable({ providedIn: 'root' })
export class RecipeService {
  private readonly client = inject(MockApiClient);

  getList(params: RecipeListParams): Promise<PagedResult<RecipeCard>> {
    return this.client.call<PagedResult<RecipeCard>>('recipes.list', params);
  }

  get(id: string): Promise<Recipe> {
    return this.client.call<Recipe>('recipes.get', { id });
  }

  getSimilar(id: string): Promise<RecipeCard[]> {
    return this.client.call<RecipeCard[]>('recipes.similar', { id });
  }

  toggleFavorite(id: string): Promise<FavoriteResult> {
    return this.client.call<FavoriteResult>('recipes.toggleFavorite', { id });
  }
}
