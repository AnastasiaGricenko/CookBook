/**
 * core/config/app.config.ts — конфигурация провайдеров приложения.
 *
 * Роль в архитектуре: корневой композиционный корень. Здесь собраны роутер
 * и HTTP-провайдеры: HttpClient с интерцепторами префикса API и ошибок.
 *
 * Гарантии:
 *  - все запросы идут через HttpClient (fetch/XMLHttpRequest не используются);
 *  - withComponentInputBinding позволяет странице рецепта получать :id как input;
 *  - транспортные правила (префикс, cookie, ApiError) заданы в интерцепторах.
 */
import { ApplicationConfig } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { apiErrorInterceptor } from '../http/api-error.interceptor';
import { apiPrefixInterceptor } from '../http/api-prefix.interceptor';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(withInterceptors([apiPrefixInterceptor, apiErrorInterceptor])),
  ],
};
