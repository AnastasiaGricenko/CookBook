/**
 * NotificationToast — визуальный слой уведомлений NotificationService.
 *
 * Роль в архитектуре: встраивается в оболочки (AppShell / GuestShell) и
 * отображает текущий список уведомлений. Сам ничего не решает — только
 * рисует и позволяет закрыть уведомление кликом. Потребляет shared.
 */
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { NotificationService } from '../notification-service';

@Component({
  selector: 'app-notification-toast',
  templateUrl: './notification-toast.html',
  styleUrl: './notification-toast.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotificationToast {
  private readonly notificationsService = inject(NotificationService);

  protected readonly notifications = this.notificationsService.notifications;

  protected dismiss(id: number): void {
    this.notificationsService.dismiss(id);
  }
}
