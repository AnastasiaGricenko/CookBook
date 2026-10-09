/**
 * RecipeDetailPage — страница отдельного рецепта.
 *
 * Роль в архитектуре: страница-контейнер фичи recipes. Получает :id как input
 * (withComponentInputBinding), загружает рецепт и похожие, управляет избранным.
 * Составные блоки вынесены в components/ и получают данные через input.
 *
 * Гарантии:
 *  - id реактивен (effect перезагружает данные при переходах между рецептами);
 *  - рецепт и похожие грузятся параллельно;
 *  - ошибка не роняет страницу: показывается уведомление, состояние сбрасывается.
 */
import { ChangeDetectionStrategy, Component, effect, inject, input, signal } from '@angular/core';
import { toApiError } from '../../../../shared/api-error';
import { Recipe, RecipeCard as RecipeCardModel } from '../../../../shared/models';
import { NotificationService } from '../../../../shared/notifications/notification-service';
import { RecipeService } from '../../../../core/services/recipe.service';
import { IngredientList } from '../../components/ingredient-list/ingredient-list';
import { NutritionGrid } from '../../components/nutrition-grid/nutrition-grid';
import { SeasoningPanel } from '../../components/seasoning-panel/seasoning-panel';
import { SimilarRecipes } from '../../components/similar-recipes/similar-recipes';
import { StepsSection } from '../../components/steps-section/steps-section';

@Component({
  selector: 'app-recipe-detail',
  imports: [NutritionGrid, IngredientList, SeasoningPanel, StepsSection, SimilarRecipes],
  templateUrl: './recipe-detail.html',
  styleUrl: './recipe-detail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RecipeDetailPage {
  private readonly recipesService = inject(RecipeService);
  private readonly notifications = inject(NotificationService);

  /** Идентификатор рецепта из маршрута (component input binding). */
  readonly id = input.required<string>();

  private readonly recipeState = signal<Recipe | null>(null);
  private readonly similarState = signal<RecipeCardModel[]>([]);

  protected readonly recipe = this.recipeState.asReadonly();
  protected readonly similar = this.similarState.asReadonly();

  constructor() {
    // id реактивен: переход между рецептами переиспользует компонент.
    effect(() => {
      const id = this.id();
      void this.load(id);
    });
  }

  protected toggleFavorite(): void {
    const recipe = this.recipeState();
    if (!recipe) {
      return;
    }
    void this.toggle(recipe.id);
  }

  private async toggle(id: string): Promise<void> {
    try {
      const result = await this.recipesService.toggleFavorite(id);
      this.recipeState.update(current =>
        current ? { ...current, isFavorite: result.isFavorite } : current,
      );
    } catch (error) {
      this.notifications.show(toApiError(error).body.message, 'error');
    }
  }

  private async load(id: string): Promise<void> {
    try {
      const [recipe, similar] = await Promise.all([
        this.recipesService.get(id),
        this.recipesService.getSimilar(id),
      ]);
      this.recipeState.set(recipe);
      this.similarState.set(similar);
    } catch (error) {
      this.recipeState.set(null);
      this.similarState.set([]);
      this.notifications.show(toApiError(error).body.message, 'error');
    }
  }
}
