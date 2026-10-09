/**
 * AppRoot — корневой компонент приложения.
 *
 * Роль в архитектуре: единственная точка входа бутстрапа, содержит ТОЛЬКО
 * <router-outlet />. Никакой шапки/рамки на корневом уровне — их задают
 * оболочки (AppShell / GuestShell), выбранные маршрутом.
 *
 * Здесь же один раз за жизнь приложения гидратируется текущий пользователь
 * (loadMe) по сохранённой сессии, чтобы после F5 оболочки сразу знали роль.
 */
import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app-root.html',
  styleUrl: './app-root.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppRoot implements OnInit {
  private readonly auth = inject(AuthService);

  ngOnInit(): void {
    // Однократная гидратация пользователя по сессии (после F5).
    void this.auth.loadMe();
  }
}
