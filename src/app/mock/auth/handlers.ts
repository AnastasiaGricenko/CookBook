/**
 * mock/auth/handlers.ts — доменные обработчики мок-слоя для домена auth.
 *
 * Регистрирует методы auth.login / auth.register / auth.logout / auth.me.
 * Сессия управляется через SessionStore, захваченный при регистрации
 * (injection-контекст setupMockLayer), поэтому обработчики остаются
 * чистыми функциями (db, params) => result.
 *
 * Гарантии:
 *  - успешный login/register атомарно ставит сессию (register — автовход);
 *  - logout очищает сессию; me восстанавливает пользователя после F5;
 *  - тексты отказов — константы AUTH_ERRORS (единый источник для страниц).
 */
import { inject } from '@angular/core';
import { LoginParams, MeDto, RegisterParams } from '../../shared/models';
import { MockUserRecord } from '../mock-db';
import { MockApiClient } from '../mock-api-client';
import { MockValidationError } from '../mock-error';
import { newMockId } from '../ids';
import { SessionStore } from './session-store';

/** Тексты отказов auth — сравниваются страницами, не дублируются литералами. */
export const AUTH_ERRORS = {
  invalidCredentials: 'Неверный email или пароль',
  emailTaken: 'Этот email уже зарегистрирован',
  unauthorized: 'Требуется вход',
} as const;

export function registerAuthHandlers(client: MockApiClient): void {
  const session = inject(SessionStore);

  client
    .register<LoginParams, MeDto>('auth.login', (db, params) => {
      const email = params.email.trim().toLowerCase();
      const user = db
        .read()
        .users.find(record => record.email.toLowerCase() === email && record.password === params.password);
      if (!user) {
        throw new MockValidationError(401, AUTH_ERRORS.invalidCredentials);
      }
      session.set(user.id);
      return toMe(user);
    })
    .register<RegisterParams, MeDto>('auth.register', (db, params) => {
      const email = params.email.trim().toLowerCase();
      const exists = db.read().users.some(record => record.email.toLowerCase() === email);
      if (exists) {
        throw new MockValidationError(400, AUTH_ERRORS.emailTaken);
      }
      const user: MockUserRecord = {
        id: newMockId(),
        email: params.email.trim(),
        password: params.password,
        name: params.name.trim(),
        avatarUrl: null,
      };
      const created = db.mutate(data => {
        data.users.push(user);
        return user;
      });
      // Успешная регистрация сразу авторизует пользователя.
      session.set(created.id);
      return toMe(created);
    })
    .register<null, null>('auth.logout', (_db, _params) => {
      session.clear();
      return null;
    })
    .register<null, MeDto>('auth.me', (db, _params) => {
      const userId = session.get();
      if (!userId) {
        throw new MockValidationError(401, AUTH_ERRORS.unauthorized);
      }
      const user = db.read().users.find(record => record.id === userId);
      if (!user) {
        // Сессия ссылается на несуществующего пользователя — очищаем и отказываем.
        session.clear();
        throw new MockValidationError(401, AUTH_ERRORS.unauthorized);
      }
      return toMe(user);
    });
}

/** Проекция внутренней записи пользователя в транспортную форму. */
function toMe(user: MockUserRecord): MeDto {
  return { id: user.id, email: user.email, name: user.name, avatarUrl: user.avatarUrl };
}
