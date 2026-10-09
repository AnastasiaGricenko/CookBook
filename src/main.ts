/**
 * main.ts — точка входа приложения (zone-based bootstrap).
 *
 * Роль в архитектуре: только запуск корневого компонента AppRoot с общими
 * провайдерами appConfig. Мок-слой поднимается внутри appConfig через
 * provideAppInitializer (см. core/config/app.config.ts).
 */
import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/core/config/app.config';
import { AppRoot } from './app/layout/app-root/app-root';

bootstrapApplication(AppRoot, appConfig).catch((error: unknown) => console.error(error));
