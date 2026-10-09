/**
 * AppShell — оболочка просмотра (каталог и карточка рецепта).
 *
 * Роль в архитектуре: layout-компонент, который даёт «рамку приложения» —
 * шапку с поиском, навигацией и меню пользователя, а также глобальные
 * уведомления. Внутри — <router-outlet /> для фичевых страниц recipes.
 *
 * Прежний HeaderComponent расформирован: его разметка и стили переехали сюда.
 * Здесь чистая оболочка: обработчики переводят намерения в вызовы сервисов.
 */
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { Dropdown } from '../../shared/components/dropdown/dropdown';
import { NotificationToast } from '../../shared/notifications/notification-toast/notification-toast';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, Dropdown, NotificationToast],
  templateUrl: './app-shell.html',
  styleUrl: './app-shell.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppShell {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly currentUser = this.auth.currentUser;

  protected onSearch(event: Event): void {
    const value = (event.target as HTMLInputElement).value.trim();
    void this.router.navigate(['/recipes'], { queryParams: value ? { q: value } : {} });
  }

  protected logout(): void {
    void this.performLogout();
  }

  private async performLogout(): Promise<void> {
    await this.auth.logout();
    await this.router.navigate(['/recipes']);
  }
}
