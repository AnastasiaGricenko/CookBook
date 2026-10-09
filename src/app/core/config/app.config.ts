/**
 * core/config/app.config.ts — конфигурация провайдеров приложения.
 *
 * Роль в архитектуре: корневой композиционный корень. Здесь собраны роутер
 * и единственный bootstrap-хук, поднимающий мок-слой до первого обращения
 * страниц к сервисам. HTTP-провайдеров нет принципиально (только мок-слой).
 *
 * Гарантии:
 *  - setupMockLayer выполняется через provideAppInitializer (после DI, до старта);
 *  - withComponentInputBinding позволяет странице рецепта получать :id как input.
 */
import { ApplicationConfig, provideAppInitializer } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { setupMockLayer } from '../../mock';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes, withComponentInputBinding()),
    // Мок-слой поднимается до первого рендера: сид + регистрация обработчиков.
    provideAppInitializer(() => setupMockLayer()),
  ],
};
