/**
 * mock/ids.ts — генератор идентификаторов для новых записей мок-БД.
 *
 * Используется доменными обработчиками (регистрация пользователя и т.п.).
 * Предпочитает криптостойкий randomUUID, но не падает там, где его нет.
 */
export function newMockId(): string {
  if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
    return crypto.randomUUID();
  }
  return `mock-${Math.random().toString(36).slice(2)}${Date.now().toString(36)}`;
}
