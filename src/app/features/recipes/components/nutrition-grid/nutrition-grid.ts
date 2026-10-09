/**
 * NutritionGrid — блок пищевой ценности (КБЖУ).
 *
 * Роль в архитектуре: презентационный компонент фичи recipes. Получает
 * данные через input и только отображает их. Никакой логики.
 */
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { Nutrition } from '../../../../shared/models';

@Component({
  selector: 'app-nutrition-grid',
  templateUrl: './nutrition-grid.html',
  styleUrl: './nutrition-grid.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NutritionGrid {
  readonly nutrition = input.required<Nutrition>();
}
