/**
 * shared/notifications/notification-service.ts — сервис всплывающих уведомлений.
 *
 * Роль в архитектуре: единственная точка показа пользовательских сообщений
 * (успех / ошибка / инфо). Потребляется страницами и оболочками; сам ничего
 * не знает о доменах. Состояние — сигнал, наружу отдаётся read-only список.
 *
 * Гарантии:
 *  - уведомление автоматически закрывается через ограниченное время;
 *  - закрытие по клику не зависит от таймера (id-адресация).
 */
import { Injectable, signal } from '@angular/core';

export type NotificationType = 'success' | 'error' | 'info';

export interface Notification {
  id: number;
  message: string;
  type: NotificationType;
}

const AUTO_DISMISS_MS = 4000;

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly state = signal<Notification[]>([]);
  private nextId = 1;

  readonly notifications = this.state.asReadonly();

  show(message: string, type: NotificationType = 'info'): void {
    const notification: Notification = { id: this.nextId++, message, type };
    this.state.update(list => [...list, notification]);
    // Автозакрытие: уведомления не должны накапливаться на экране.
    setTimeout(() => this.dismiss(notification.id), AUTO_DISMISS_MS);
  }

  dismiss(id: number): void {
    this.state.update(list => list.filter(item => item.id !== id));
  }
}
