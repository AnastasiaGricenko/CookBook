/**
 * core/http/api-error.interceptor.ts — единая нормализация транспортных ошибок.
 *
 * Роль в архитектуре: транспортный слой core. Любой HttpErrorResponse
 * превращается в доменную ApiError { status, body: { message, errors? } },
 * поэтому страницы работают с одним форматом отказа.
 *
 * Гарантии:
 *  - текст отказа берётся из тела ответа, иначе — 'Неизвестная ошибка';
 *  - errors приходят как Record<string, string> (сервер уже отдаёт строки);
 *  - 401 вне /auth/ сбрасывает кэш текущего пользователя, но не редиректит.
 */
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ApiError } from '../../shared/models';
import { AuthService } from '../services/auth.service';

const FALLBACK_MESSAGE = 'Неизвестная ошибка';

export const apiErrorInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthService);

  return next(request).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse)) {
        return throwError(() => error);
      }

      const isAuthRequest = request.url.includes('/auth/');
      if (error.status === 401 && !isAuthRequest) {
        auth.handleUnauthorized();
      }

      const apiError: ApiError = {
        status: error.status,
        body: normalizeBody(error.error),
      };

      return throwError(() => apiError);
    }),
  );
};

/** Приводит тело отказа сервера к { message, errors? }. */
function normalizeBody(raw: unknown): ApiError['body'] {
  if (typeof raw !== 'object' || raw === null) {
    return { message: FALLBACK_MESSAGE };
  }

  const candidate = raw as { message?: unknown; errors?: unknown };
  const message = typeof candidate.message === 'string' ? candidate.message : FALLBACK_MESSAGE;

  if (isStringRecord(candidate.errors)) {
    return { message, errors: candidate.errors };
  }

  return { message };
}

/** Проверка формы errors: объект «поле → строка». */
function isStringRecord(value: unknown): value is Record<string, string> {
  if (typeof value !== 'object' || value === null || Array.isArray(value)) {
    return false;
  }

  return Object.values(value as Record<string, unknown>).every(item => typeof item === 'string');
}
