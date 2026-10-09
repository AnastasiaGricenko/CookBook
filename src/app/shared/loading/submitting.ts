/**
 * shared/loading/submitting.ts — контроллер состояния отправки форм и действий.
 *
 * Роль в архитектуре: единая защита от параллельного повторного запуска одной
 * операции и источник флага `active` для кнопок/спиннеров. Компонент владеет
 * собственным контроллером, наружу отдаётся только read-only сигнал.
 *
 * Гарантии:
 *  - повторный запуск во время активной операции игнорируется (возврат undefined);
 *  - флаг `active` сбрасывается всегда, даже при исключении.
 */
import { Signal, signal } from '@angular/core';

export interface SubmittingController {
  /** true, пока операция выполняется. */
  readonly active: Signal<boolean>;
  /** Выполняет операцию, если ничего не выполняется. Возвращает undefined при занятости. */
  run<T>(fn: () => Promise<T>): Promise<T | undefined>;
}

export function submitting(): SubmittingController {
  const active = signal(false);
  return {
    active: active.asReadonly(),
    async run<T>(fn: () => Promise<T>): Promise<T | undefined> {
      if (active()) {
        return undefined;
      }
      active.set(true);
      try {
        return await fn();
      } finally {
        active.set(false);
      }
    },
  };
}
