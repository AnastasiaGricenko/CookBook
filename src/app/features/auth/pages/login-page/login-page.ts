/**
 * LoginPage — страница входа.
 *
 * Роль в архитектуре: страница-контейнер фичи auth. Владеет формой, вызывает
 * AuthService и показывает пользователю результат. Никакой транспортной
 * логики: ошибки приходят как ApiError и превращаются в уведомления.
 *
 * Гарантии:
 *  - защита от двойного сабмита через контроллер submitting();
 *  - после успеха — переход на домашний маршрут '/recipes';
 *  - кнопка Google — заглушка (мок), сообщает об этом уведомлением.
 */
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { toApiError } from '../../../../shared/api-error';
import { submitting } from '../../../../shared/loading/submitting';
import { NotificationService } from '../../../../shared/notifications/notification-service';

@Component({
  selector: 'app-login-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login-page.html',
  styleUrl: './login-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginPage {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly notifications = inject(NotificationService);

  protected readonly submitting = submitting();

  protected readonly form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  protected onGoogle(): void {
    this.notifications.show('Вход через Google скоро будет доступен', 'info');
  }

  protected onSubmit(): void {
    if (this.form.invalid || this.submitting.active()) {
      this.form.markAllAsTouched();
      return;
    }
    void this.submit();
  }

  private async submit(): Promise<void> {
    try {
      const value = this.form.getRawValue();
      await this.submitting.run(() =>
        this.auth.login({ email: value.email ?? '', password: value.password ?? '' }),
      );
      this.notifications.show('Вы вошли в аккаунт', 'success');
      await this.router.navigate(['/recipes']);
    } catch (error) {
      this.notifications.show(toApiError(error).body.message, 'error');
    }
  }
}
