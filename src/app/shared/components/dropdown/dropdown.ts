/**
 * Dropdown — переиспользуемый выпадающий список с проекцией содержимого.
 *
 * Роль в архитектуре: презентационный компонент слоя shared; используется
 * шапкой AppShell для меню пользователя. Состояние открытия — локальный сигнал.
 * Слоты задаются через селекторы `[trigger]` и `[menu]`.
 */
import { ChangeDetectionStrategy, Component, signal } from '@angular/core';

@Component({
  selector: 'app-dropdown',
  templateUrl: './dropdown.html',
  styleUrl: './dropdown.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Dropdown {
  protected readonly isOpen = signal(false);

  protected toggle(): void {
    this.isOpen.update(open => !open);
  }

  protected close(): void {
    this.isOpen.set(false);
  }
}
