/**
 * StepsSection — блок шагов приготовления.
 *
 * Роль в архитектуре: презентационный компонент фичи recipes. Получает
 * шаги через input и отображает их нумерованным списком. Без логики.
 */
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RecipeStep } from '../../../../shared/models';

@Component({
  selector: 'app-steps-section',
  templateUrl: './steps-section.html',
  styleUrl: './steps-section.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StepsSection {
  readonly steps = input.required<RecipeStep[]>();
}
