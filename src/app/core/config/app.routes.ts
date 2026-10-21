/**
 * core/config/app.routes.ts — корневая конфигурация маршрутов.
 *
 * Роль в архитектуре: единственное место «сборки» навигации приложения.
 * Монтирует оболочки (AppShell / GuestShell) поверх фичевых маршрутов и
 * подключает guard'ы. Фичевые маршруты подключаются лениво через loadChildren:
 * чанк фичи грузится при первой навигации на её маршрут.
 *
 * Карта URL:
 *  - '/'                     → homeGuard → '/recipes' (канонизация);
 *  - '/recipes'              → AppShell + список/создание/карточка (ленивый чанк);
 *  - '/auth' + '/auth/login' | '/auth/register' → GuestShell + формы (guestGuard,
 *    ленивый чанк);
 *  - '**'                    → homeGuard (фолбэк на домашний маршрут роли).
 *
 * Гарантии:
 *  - фичи не тянутся в стартовый бандл: страницы подключены loadChildren,
 *    а внутри фич — loadComponent (по чанку на страницу);
 *  - canActivate вызывается роутером ПОСЛЕ загрузки ленивой конфигурации,
 *    поэтому guard'ы защищают маршруты до отрисовки страницы;
 *  - домашний маршрут вычисляется guard'ом по роли (ROLE_HOME).
 */
import { Routes } from '@angular/router';
import { AppShell } from '../../layout/app-shell/app-shell';
import { GuestShell } from '../../layout/guest-shell/guest-shell';
import { guestGuard, homeGuard } from '../navigation/guards';

export const routes: Routes = [
  // '/' канонизируется на домашний маршрут роли (у гостя и пользователя — '/recipes').
  { path: '', pathMatch: 'full', canActivate: [homeGuard], children: [] },
  // Оболочка просмотра: шапка AppShell; содержимое фичи — ленивый чанк.
  {
    path: 'recipes',
    component: AppShell,
    loadChildren: () =>
      import('../../features/recipes/recipes.routes').then(m => m.RECIPES_ROUTES),
  },
  // Оболочка auth: гостевая рамка (закрыта для авторизованных); фича — ленивый чанк.
  {
    path: 'auth',
    canActivate: [guestGuard],
    children: [
      {
        path: '',
        component: GuestShell,
        loadChildren: () =>
          import('../../features/auth/auth.routes').then(m => m.AUTH_ROUTES),
      },
    ],
  },
  // Фолбэк: любой несуществующий путь ведёт на домашний маршрут роли.
  { path: '**', canActivate: [homeGuard], children: [] },
];
