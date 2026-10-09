/**
 * RegisterPage — страница регистрации.
 *
 * Роль в архитектуре: страница-контейнер фичи auth. Владеет формой с
 * кросс-полевой валидацией совпадения паролей, вызывает AuthService и
 * сообщает результат. Успешная регистрация авторизует (автовход в мок-слое).
 *
 * Гарантии:
 *  - защита от двойного сабмита через контроллер submitting();
 *  - пароли сверяются валидатором passwordsMatch на уровне группы;
 *  - после успеха — переход на домашний маршрут '/recipes'.
 */
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { toApiError } from '../../../../shared/api-error';
import { submitting } from '../../../../shared/loading/submitting';
import { NotificationService } from '../../../../shared/notifications/notification-service';

/** Кросс-полевая проверка: пароль и подтверждение должны совпадать. */
const passwordsMatch: ValidatorFn = (group: AbstractControl): ValidationErrors | null => {
  const password = group.get('password')?.value;
  const confirmPassword = group.get('confirmPassword')?.value;
  return password === confirmPassword ? null : { passwordsMatch: true };
};

@Component({
  selector: 'app-register-page',
  imports: [ReactiveFormsModule],
  templateUrl: './register-page.html',
  styleUrl: './register-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RegisterPage {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly notifications = inject(NotificationService);

  protected readonly submitting = submitting();

  protected readonly form = this.fb.group(
    {
      name: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: passwordsMatch },
  );

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
        this.auth.register({
          name: value.name ?? '',
          email: value.email ?? '',
          password: value.password ?? '',
        }),
      );
      this.notifications.show('Аккаунт создан. Добро пожаловать!', 'success');
      await this.router.navigate(['/recipes']);
    } catch (error) {
      this.notifications.show(toApiError(error).body.message, 'error');
    }
  }
}
