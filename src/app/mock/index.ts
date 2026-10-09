/**
 * mock/index.ts — агрегатор сборки мок-слоя.
 *
 * setupMockLayer() подключает сид и регистрирует все доменные обработчики.
 * Вызывается один раз при бутстрапе через provideAppInitializer в
 * core/config/app.config.ts — до первого обращения страниц к сервисам.
 * Импортируется только из core (app.config) — правило слоёв.
 */
import { inject } from '@angular/core';
import { MockApiClient } from './mock-api-client';
import { configureMockDbSeed } from './mock-db';
import { seedFixtures } from './seed';
import { registerAuthHandlers } from './auth/handlers';
import { registerRecipesHandlers } from './recipes/handlers';

export function setupMockLayer(client?: MockApiClient): void {
  const target = client ?? inject(MockApiClient);
  configureMockDbSeed(seedFixtures);
  registerAuthHandlers(target);
  registerRecipesHandlers(target);
}
