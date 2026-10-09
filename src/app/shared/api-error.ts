/**
 * shared/api-error.ts — утилиты распознавания и нормализации транспортных ошибок.
 *
 * Роль в архитектуре: потребляется страницами (features) при обработке catch,
 * чтобы достать человекочитаемый текст отказа (`toApiError(error).body.message`).
 * Импортирует только типы из shared/models.
 */
import { ApiError } from './models';

/** Проверка, что неизвестное значение уже является транспортной ошибкой. */
export function isApiError(error: unknown): error is ApiError {
  if (typeof error !== 'object' || error === null) {
    return false;
  }
  const candidate = error as Partial<ApiError>;
  return (
    typeof candidate.status === 'number' &&
    typeof candidate.body === 'object' &&
    candidate.body !== null
  );
}

/** Приводит любое значение к ApiError, подставляя безопасный текст по умолчанию. */
export function toApiError(error: unknown): ApiError {
  if (isApiError(error)) {
    return error;
  }
  return { status: 0, body: { message: 'Неизвестная ошибка' } };
}
