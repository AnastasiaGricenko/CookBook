/**
 * features/auth/auth.routes.ts — маршруты раздела авторизации.
 *
 * Роль в архитектуре: агрегатор фичи. Владеет путями login/register и
 * корневым редиректом на форму входа. Монтируется оболочкой GuestShell
 * в core/config/app.routes.ts; сам об оболочке ничего не знает.
 *
 * Гарантии:
 *  - страницы подключены loadComponent — чанк auth-фичи грузится при
 *    первой навигации на вход/регистрацию;
 *  - редирект '' → 'login' сохранён.
 */
import { Routes } from '@angular/router';

export const AUTH_ROUTES: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  {
    path: 'login',
    loadComponent: () =>
      import('./pages/login-page/login-page').then(m => m.LoginPage),
    title: 'КулиBook — Вход',
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./pages/register-page/register-page').then(m => m.RegisterPage),
    title: 'КулиBook — Регистрация',
  },
];
