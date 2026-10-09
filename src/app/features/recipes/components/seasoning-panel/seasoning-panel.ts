/**
 * SeasoningPanel — блок приправ рецепта с чек-листом.
 *
 * Роль в архитектуре: презентационный компонент фичи recipes. Получает
 * список приправ через input, хранит только локальный UI-флаг «отмечено».
 */
import { ChangeDetectionStrategy, Component, input, signal } from '@angular/core';

@Component({
  selector: 'app-seasoning-panel',
  templateUrl: './seasoning-panel.html',
  styleUrl: './seasoning-panel.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SeasoningPanel {
  readonly seasonings = input.required<string[]>();

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
