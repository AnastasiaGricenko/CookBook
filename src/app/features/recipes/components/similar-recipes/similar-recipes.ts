/**
 * SimilarRecipes — блок похожих рецептов.
 *
 * Роль в архитектуре: презентационный компонент фичи recipes. Получает
 * карточки через input и раскладывает их сеткой через общий RecipeCard.
 */
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RecipeCard as RecipeCardModel } from '../../../../shared/models';
import { RecipeCard } from '../../../../shared/components/recipe-card/recipe-card';

@Component({
  selector: 'app-similar-recipes',
  imports: [RecipeCard],
  templateUrl: './similar-recipes.html',
  styleUrl: './similar-recipes.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SimilarRecipes {
  readonly recipes = input.required<RecipeCardModel[]>();
}
