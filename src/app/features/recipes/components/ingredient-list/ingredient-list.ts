/**
 * IngredientList — блок ингредиентов рецепта с чек-листом.
 *
 * Роль в архитектуре: презентационный компонент фичи recipes. Получает
 * ингредиенты через input, хранит только локальный UI-флаг «отмечено».
 * Не зависит от сервисов.
 */
import { ChangeDetectionStrategy, Component, input, signal } from '@angular/core';
import { RecipeIngredient } from '../../../../shared/models';

@Component({
  selector: 'app-ingredient-list',
  templateUrl: './ingredient-list.html',
  styleUrl: './ingredient-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IngredientList {
  readonly ingredients = input.required<RecipeIngredient[]>();

  private readonly checked = signal<ReadonlySet<number>>(new Set<number>());

  protected isChecked(index: number): boolean {
    return this.checked().has(index);
  }

  protected toggle(index: number): void {
    this.checked.update(current => {
      const next = new Set(current);
      if (next.has(index)) {
        next.delete(index);
      } else {
        next.add(index);
      }
      return next;
    });
  }
}
