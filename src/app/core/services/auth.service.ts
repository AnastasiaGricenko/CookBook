/**
 * core/services/auth.service.ts — сервис аутентификации.
 *
 * Роль в архитектуре: тонкая обёртка над MockApiClient для домена auth.
 * Страницы и оболочки зависят только от этого сервиса, а не от мок-слоя.
 * Единственное место, где приложение читает текущего пользователя.
 *
 * Гарантии:
 *  - currentUser — read-only сигнал, обновляется login/register/loadMe/logout;
 *  - isAuthenticated синхронен (сессия в localStorage) — безопасен для guard;
 *  - loadMe идемпотентен и безопасен после F5: при отсутствии сессии → null.
 */
import { Injectable, inject, signal } from '@angular/core';
import { LoginParams, MeDto, RegisterParams } from '../../shared/models';
import { MockApiClient } from '../../mock/mock-api-client';
import { SessionStore } from '../../mock/auth/session-store';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly client = inject(MockApiClient);
  private readonly session = inject(SessionStore);
  private readonly currentUserState = signal<MeDto | null>(null);

  readonly currentUser = this.currentUserState.asReadonly();

  /** Синхронная проверка наличия сессии — используется guard'ами. */
  isAuthenticated(): boolean {
    return this.session.hasUser();
  }

  async login(params: LoginParams): Promise<MeDto> {
    const user = await this.client.call<MeDto>('auth.login', params);
    this.currentUserState.set(user);
    return user;
  }

  async register(params: RegisterParams): Promise<MeDto> {
    const user = await this.client.call<MeDto>('auth.register', params);
    this.currentUserState.set(user);
    return user;
  }

  async logout(): Promise<void> {
    await this.client.call<null>('auth.logout', null);
    this.currentUserState.set(null);
  }

  /** Восстанавливает текущего пользователя по сессии (например, после F5). */
  async loadMe(): Promise<MeDto | null> {
    if (!this.session.hasUser()) {
      return null;
    }
    try {
      const user = await this.client.call<MeDto>('auth.me', null);
      this.currentUserState.set(user);
      return user;
    } catch {
      // Сессия оказалась невалидной — очищаем её и продолжаем как гость.
      this.session.clear();
      this.currentUserState.set(null);
      return null;
    }
  }
}
