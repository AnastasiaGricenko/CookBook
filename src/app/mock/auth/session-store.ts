/**
 * mock/auth/session-store.ts — сессия мок-пользователя в localStorage.
 *
 * Роль в архитектуре: единственное место работы с ключом STORAGE_KEYS.session.
 * Потребляется обработчиками auth и сервисом AuthService (только через
 * сессионные методы), но ключ localStorage трогает только этот класс.
 *
 * Гарантии:
 *  - сессия переживает F5 (это обязательное поведение);
 *  - сессия не хранится в мок-БД — у неё отдельный ключ.
 */
import { Injectable } from '@angular/core';
import { STORAGE_KEYS } from '../../shared/models';

@Injectable({ providedIn: 'root' })
export class SessionStore {
  set(userId: string): void {
    localStorage.setItem(STORAGE_KEYS.session, userId);
  }

  get(): string | null {
    return localStorage.getItem(STORAGE_KEYS.session);
  }

  hasUser(): boolean {
    return this.get() !== null;
  }

  clear(): void {
    localStorage.removeItem(STORAGE_KEYS.session);
  }
}
