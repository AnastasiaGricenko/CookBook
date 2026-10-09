/**
 * mock/mock-db.ts — мок-БД приложения в localStorage («бэкенд без бэкенда»).
 *
 * Роль в архитектуре: хранилище данных мок-слоя. Единственный компонент,
 * работающий с ключом STORAGE_KEYS.mockDb. Потребляется MockApiClient и
 * обработчиками (через переданный им экземпляр). Импортирует только shared/models.
 *
 * Гарантии:
 *  - ленивая инициализация: первое чтение читает localStorage, при отсутствии
 *    или повреждении данных выполняется защитный повторный сид (без PII в логе);
 *  - read() отдаёт изолированный снимок (structuredClone) — правки наружу не текут;
 *  - mutate() — транзакция: черновик клона при успехе становится состоянием,
 *    при исключении — отбрасывается целиком, состояние БД не меняется.
 */
import { Injectable, inject } from '@angular/core';
import { Recipe, STORAGE_KEYS } from '../shared/models';

/** Внутренняя запись пользователя мок-БД (пароль хранится открыто — это мок). */
export interface MockUserRecord {
  id: string;
  email: string;
  password: string;
  name: string;
  avatarUrl: string | null;
}

export interface MockDbData {
  users: MockUserRecord[];
  recipes: Recipe[];
}

/** Поставщик начальных данных. До подключения БД пуста. */
export type MockDbSeedProvider = () => MockDbData;

/** Пустое состояние БД — используется, пока сид не подключён. */
export function emptyMockDbData(): MockDbData {
  return { users: [], recipes: [] };
}

@Injectable({ providedIn: 'root' })
export class MockDb {
  private cache: MockDbData | null = null;
  private seedProvider: MockDbSeedProvider = emptyMockDbData;

  /** Подключает поставщика сида (вызывается один раз при сборке мок-слоя). */
  configureSeed(provider: MockDbSeedProvider): void {
    this.seedProvider = provider;
    this.cache = null;
  }

  /** Возвращает изолированный снимок данных. */
  read(): MockDbData {
    if (!this.cache) {
      this.cache = this.load();
    }
    return structuredClone(this.cache);
  }

  /** Транзакционная мутация: успех фиксирует клон, исключение откатывает всё. */
  mutate<T>(fn: (data: MockDbData) => T): T {
    const draft = this.read();
    const result = fn(draft);
    this.cache = draft;
    this.persist(draft);
    return result;
  }

  private load(): MockDbData {
    try {
      const raw = localStorage.getItem(STORAGE_KEYS.mockDb);
      if (!raw) {
        return this.seed();
      }
      const parsed = JSON.parse(raw) as MockDbData;
      if (!parsed || !Array.isArray(parsed.users) || !Array.isArray(parsed.recipes)) {
        throw new Error('unexpected mock db shape');
      }
      return parsed;
    } catch {
      // Данные повреждены — молча пересеваем. В логе нет персональных данных.
      console.warn('MockDb: обнаружены повреждённые данные — выполняется повторный сид');
      return this.seed();
    }
  }

  private seed(): MockDbData {
    const data = this.seedProvider();
    this.persist(data);
    return data;
  }

  private persist(data: MockDbData): void {
    try {
      localStorage.setItem(STORAGE_KEYS.mockDb, JSON.stringify(data));
    } catch {
      // localStorage может быть недоступен (приватный режим/квота) — молча пропускаем.
    }
  }
}

/** Подключение поставщика сида в injection-контексте (вызывается из setupMockLayer). */
export function configureMockDbSeed(provider: MockDbSeedProvider): void {
  inject(MockDb).configureSeed(provider);
}
