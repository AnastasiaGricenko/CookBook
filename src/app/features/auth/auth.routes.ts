/**
 * features/auth/auth.routes.ts — маршруты раздела авторизации.
 *
 * Роль в архитектуре: агрегатор фичи. Владеет путями login/register и
 * корневым редиректом на форму входа. Монтируется оболочкой GuestShell
 * в core/config/app.routes.ts; сам об оболочке ничего не знает.
 */
import { Routes } from '@angular/router';
import { LoginPage } from './pages/login-page/login-page';
import { RegisterPage } from './pages/register-page/register-page';

export const AUTH_ROUTES: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: 'login', component: LoginPage, title: 'КулиBook — Вход' },
  { path: 'register', component: RegisterPage, title: 'КулиBook — Регистрация' },
];
