/**
 * core/http/api-prefix.interceptor.ts — префикс REST API и cookie-сессия.
 *
 * Роль в архитектуре: транспортный слой core. Сервисы указывают относительный
 * путь ('recipes', 'auth/me'), а префикс и флаг withCredentials добавляются здесь.
 *
 * Гарантии:
 *  - абсолютные URL (http/https) не переписываются;
 *  - повторный префикс не дублируется;
 *  - httpOnly-cookie access_token отправляется с каждым запросом.
 */
import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { API_BASE_URL } from '../config/api-base-url';

export const apiPrefixInterceptor: HttpInterceptorFn = (request, next) => {
  const baseUrl = inject(API_BASE_URL);
  const isAbsolute = /^https?:\/\//i.test(request.url);
  const hasPrefix = request.url.startsWith(baseUrl);

  const url =
    isAbsolute || hasPrefix
      ? request.url
      : `${baseUrl}/${request.url.replace(/^\/+/, '')}`;

  return next(request.clone({ url, withCredentials: true }));
};
