/**
 * core/config/api-base-url.ts — единственное место с префиксом REST API.
 *
 * Роль в архитектуре: конфигурационный токен core. Значение подставляется
 * api-prefix-интерцептором ко всем относительным запросам HttpClient.
 *
 * Гарантии:
 *  - в дев-режиме префикс проксируется на бэкенд (proxy.conf.json);
 *  - в production тот же путь обслуживает nginx (/api/ → api:8080/api/).
 */
import { InjectionToken } from '@angular/core';

/** Базовый префикс REST API. */
export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL', {
  providedIn: 'root',
  factory: () => '/api/v1',
});
