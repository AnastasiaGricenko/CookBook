/**
 * ProgressBar — индикатор прогресса в процентах.
 *
 * Роль в архитектуре: презентационный компонент слоя shared. Значение
 * приходит через `input` и используется в шаблонах фич.
 */
import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-progress-bar',
  templateUrl: './progress-bar.html',
  styleUrl: './progress-bar.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProgressBar {
  readonly progress = input.required<number>();
}
