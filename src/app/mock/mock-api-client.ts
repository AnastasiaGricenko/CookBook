/**
 * mock/mock-api-client.ts — конвейер мок-вызова, эмулирующий REST-транспорт.
 *
 * Роль в архитектуре: сервисы core вызывают `client.call('домен.операция', params)`,
 * а клиент находит зарегистрированный обработчик, выполняет его синхронно над
 * MockDb и возвращает Promise. Импортирует shared/models, MockDb, MockValidationError.
 *
 * Гарантии:
 *  - каждый вызов длится не менее MOCK_DELAY_MS (эмуляция сети 500 мс);
 *  - MockValidationError транслируется в ApiError, прочие исключения пробрасываются;
 *  - незарегистрированный метод — дефект сборки мок-слоя (Error без задержки).
 */
import { Injectable, inject } from '@angular/core';
import { ApiError, MOCK_DELAY_MS } from '../shared/models';
import { MockDb } from './mock-db';
import { MockValidationError } from './mock-error';

/** Чистый синхронный обработчик: домен над снимком БД. */
export type MockHandler<P, R> = (db: MockDb, params: P) => R;

@Injectable({ providedIn: 'root' })
export class MockApiClient {
  private readonly db = inject(MockDb);
  private readonly handlers = new Map<string, MockHandler<unknown, unknown>>();

  /** Регистрация обработчика. Повторная регистрация перезаписывает прежний. */
  register<P, R>(method: string, handler: MockHandler<P, R>): this {
    this.handlers.set(method, handler as MockHandler<unknown, unknown>);
    return this;
  }

  async call<T>(method: string, params: unknown): Promise<T> {
    const handler = this.handlers.get(method);
    if (!handler) {
      // Это ошибка сборки мок-слоя, а не «ответ сервера» — бросаем без задержки.
      throw new Error(`Мок-метод не зарегистрирован: ${method}`);
    }

    const startedAt = Date.now();
    let result: T | undefined;
    let failure: unknown;
    let failed = false;

    try {
      result = handler(this.db, params) as T;
    } catch (error) {
      failed = true;
      failure = error;
    }

    // Держим нижнюю границу задержки, чтобы сеть эмулировалась и на мгновенных операциях.
    const elapsed = Date.now() - startedAt;
    if (elapsed < MOCK_DELAY_MS) {
      await new Promise<void>(resolve => setTimeout(resolve, MOCK_DELAY_MS - elapsed));
    }

    if (failed) {
      if (failure instanceof MockValidationError) {
        const apiError: ApiError = {
          status: failure.status,
          body: failure.errors
            ? { message: failure.message, errors: failure.errors }
            : { message: failure.message },
        };
        throw apiError;
      }
      throw failure;
    }

    return result as T;
  }
}
