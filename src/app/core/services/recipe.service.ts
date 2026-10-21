/**
 * core/services/recipe.service.ts — сервис рецептов.
 *
 * Роль в архитектуре: тонкий слой над REST API домена recipes. Поиск,
 * пагинация и порядок выдачи — ответственность сервера; сервис только
 * формулирует запрос и возвращает Promise страницам.
 *
 * Гарантии:
 *  - все методы возвращают Promise (Observable живёт до firstValueFrom);
 *  - каждый вызов — явный HTTP-метод (get/post) с продолжением в .pipe();
 *  - относительные пути без ведущего слэша: префикс добавит интерцептор.
 */
import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom, map } from 'rxjs';
import {
  CreateRecipeParams,
  PagedResult,
  RECIPES_PAGE_SIZE,
  Recipe,
  RecipeCard,
} from '../../shared/models';

export interface RecipeListParams {
  query: string;
  page: number;
}

@Injectable({ providedIn: 'root' })
export class RecipeService {
  private readonly http = inject(HttpClient);

  getList(params: RecipeListParams): Promise<PagedResult<RecipeCard>> {
    const httpParams = new HttpParams()
      .set('query', params.query)
      .set('page', params.page)
      .set('pageSize', RECIPES_PAGE_SIZE);

    return firstValueFrom(
      this.http.get<PagedResult<RecipeCard>>('recipes', { params: httpParams }).pipe(
        map(page => ({ ...page, items: page.items ?? [] })),
      ),
    );
  }

  get(id: string): Promise<Recipe> {
    return firstValueFrom(this.http.get<Recipe>(`recipes/${id}`));
  }

  getSimilar(id: string): Promise<RecipeCard[]> {
    return firstValueFrom(
      this.http.get<RecipeCard[]>(`recipes/${id}/similar`).pipe(
        map(cards => cards ?? []),
      ),
    );
  }

  /** Создаёт рецепт от имени текущего пользователя (требует cookie сессии). */
  create(params: CreateRecipeParams): Promise<Recipe> {
    return firstValueFrom(
      this.http.post<Recipe>('recipes', params).pipe(
        map(recipe => ({
          ...recipe,
          ingredients: recipe.ingredients ?? [],
          seasonings: recipe.seasonings ?? [],
          steps: recipe.steps ?? [],
          tags: recipe.tags ?? [],
        })),
      ),
    );
  }
}
