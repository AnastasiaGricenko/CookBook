/**
 * RecipeCard — компактная карточка рецепта для каталога и блока «похожие».
 *
 * Роль в архитектуре: чистый презентационный компонент слоя shared.
 * Получает данные через `input` и ведёт на страницу детали через routerLink.
 * Потребляется фичей recipes; сам не зависит от доменных сервисов.
 */
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { RecipeCard as RecipeCardModel } from '../../models';

@Component({
  selector: 'app-recipe-card',
  imports: [RouterLink],
  templateUrl: './recipe-card.html',
  styleUrl: './recipe-card.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RecipeCard {
  readonly recipe = input.required<RecipeCardModel>();
}
