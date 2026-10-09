/**
 * GuestShell — оболочка раздела авторизации.
 *
 * Роль в архитектуре: layout-компонент гостевой рамки. Показывает бренд и
 * табы «Вход / Регистрация», а внутри — <router-outlet /> для страниц auth.
 * Глобальные уведомления подключены и здесь, чтобы формы могли сообщать
 * об ошибках. Собственной бизнес-логики не несёт.
 */
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { NotificationToast } from '../../shared/notifications/notification-toast/notification-toast';

@Component({
  selector: 'app-guest-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, NotificationToast],
  templateUrl: './guest-shell.html',
  styleUrl: './guest-shell.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GuestShell {}
