/**
 * core/navigation/guards.ts — функциональные guard'ы и их домашние маршруты.
 *
 * Роль в архитектуре: слой навигации core. Guard'ы не мутируют конфигурацию
 * маршрутов, а подключаются декларативно (canActivate). Точечное включение
 * реализовано немутирующим хелпером withGuards.
 *
 * Гарантии:
 *  - authGuard закрывает приватные маршруты и уводит гостя на /auth/login;
 *  - guestGuard не пускает авторизованного в раздел auth (ведёт на ROLE_HOME);
 *  - homeGuard канонизирует вход «/» и «**» на домашний маршрут роли;
 *  - проверка синхронна (сессия в localStorage), потому переживает F5.
 */
import { inject } from '@angular/core';
import { CanActivateFn, Router, Routes } from '@angular/router';
import { AuthService } from '../services/auth.service';

/** Домашний маршрут роли: и гость, и пользователь видят каталог рецептов. */
export const ROLE_HOME: Record<'guest' | 'user', string> = {
  guest: '/recipes',
  user: '/recipes',
};

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  return auth.isAuthenticated() ? true : router.createUrlTree(['/auth/login']);
};

export const guestGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  return auth.isAuthenticated() ? router.createUrlTree([ROLE_HOME.user]) : true;
};

export const homeGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const role = auth.isAuthenticated() ? 'user' : 'guest';
  return router.createUrlTree([ROLE_HOME[role]]);
};

/**
 * Немутирующий хелпер: добавляет guard к маршруту(ам) с подходящим path,
 * не изменяя исходный массив. Нужен для точечного включения защиты.
 */
export function withGuards(
  routes: Routes,
  pathMatcher: string | ((path: string) => boolean),
  guard: CanActivateFn,
): Routes {
  const matches =
    typeof pathMatcher === 'function' ? pathMatcher : (path: string) => path === pathMatcher;
  return routes.map(route =>
    route.path !== undefined && matches(route.path)
      ? { ...route, canActivate: [...(route.canActivate ?? []), guard] }
      : route,
  );
}
