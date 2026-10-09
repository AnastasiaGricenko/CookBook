/**
 * core/config/app.routes.ts — корневая конфигурация маршрутов.
 *
 * Роль в архитектуре: единственное место «сборки» навигации приложения.
 * Монтирует оболочки (AppShell / GuestShell) поверх фичевых маршрутов и
 * подключает guard'ы. Фичевые маршруты приходят агрегаторами RECIPES_ROUTES
 * и AUTH_ROUTES — приложение их не переписывает.
 *
 * Карта URL:
 *  - '/'                     → homeGuard → '/recipes' (канонизация);
 *  - '/recipes'              → AppShell + список (публично);
 *  - '/recipes/:id'          → AppShell + карточка рецепта (публично);
 *  - '/auth' + '/auth/login' | '/auth/register' → GuestShell + формы (guestGuard);
 *  - '**'                    → homeGuard (фолбэк на домашний маршрут роли).
 *
 * Гарантии:
 *  - ленивая загрузка фич через loadChildren (ничего не тянется в стартовый бандл);
 *  - домашний маршрут вычисляется guard'ом по роли (ROLE_HOME).
 */
import { Routes } from '@angular/router';
import { AppShell } from '../../layout/app-shell/app-shell';
import { GuestShell } from '../../layout/guest-shell/guest-shell';
import { AUTH_ROUTES } from '../../features/auth/auth.routes';
import { RECIPES_ROUTES } from '../../features/recipes/recipes.routes';
import { guestGuard, homeGuard } from '../navigation/guards';

export const routes: Routes = [
  // '/' канонизируется на домашний маршрут роли (у гостя и пользователя — '/recipes').
  { path: '', pathMatch: 'full', canActivate: [homeGuard], children: [] },
  // Оболочка просмотра: шапка AppShell; список и карточка рецепта публичны.
  { path: 'recipes', component: AppShell, children: [...RECIPES_ROUTES] },
  // Оболочка auth: гостевая рамка для входа/регистрации (закрыта для авторизованных).
  {
    path: 'auth',
    canActivate: [guestGuard],
    children: [{ path: '', component: GuestShell, children: [...AUTH_ROUTES] }],
  },
  // Фолбэк: любой несуществующий путь ведёт на домашний маршрут роли.
  { path: '**', canActivate: [homeGuard], children: [] },
];
