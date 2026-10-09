/**
 * mock/mock-error.ts — сигнал отказа доменного обработчика мок-слоя.
 *
 * Обработчики не бросают транспортные ошибки напрямую: они кидают
 * MockValidationError, а MockApiClient транслирует его в ApiError.
 * Это разделяет «доменную» причину отказа и его транспортную форму.
 */
export class MockValidationError extends Error {
  constructor(
    readonly status: number,
    message: string,
    readonly errors?: Record<string, string>,
  ) {
    super(message);
    this.name = 'MockValidationError';
  }
}
