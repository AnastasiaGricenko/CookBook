# КулиBook — Frontend

Каталог рецептов на Angular 20 с авторизацией: список рецептов с поиском, карточка
рецепта с похожими рецептами и избранным, вход и регистрация.

Приложение полностью **frontend-only**. Роль бэкенда играет браузерный мок-слой:
мок-БД в `localStorage`, доменные обработчики, эмуляция задержки сети 500 мс и
HTTP-ошибок (400/401/404). Никаких сетевых запросов и серверного кода нет.

## Технологии

- Angular 20, standalone-компоненты, TypeScript в режиме `strict`;
- состояние — signals, данные — `async/await` над `Promise` (без `HttpClient` и
  RxJS-Observable в сервисах);
- SCSS и собственные дизайн-токены (`src/styles/`), тёмная тема.

## Запуск

```bash
npm install
npm start        # http://localhost:4200
npm run build    # production-сборка в dist/
```

Демо-учётка: **demo@kulibook.ru / demo123**.

## Архитектура

Код приложения лежит только в пяти ветках `src/app`:

```
core/      # конфигурация и сервисы
  config/      app.config.ts, app.routes.ts   (провайдеры и композиция маршрутов + guards)
  navigation/  guards.ts                       (authGuard, guestGuard, homeGuard, ROLE_HOME)
  services/    auth.service.ts, recipe.service.ts
mock/      # мок-слой («бэкенд без бэкенда»)
  mock-api-client.ts, mock-db.ts, mock-error.ts, ids.ts, seed.ts, index.ts
  auth/        handlers.ts, session-store.ts
  recipes/     handlers.ts
shared/    # переиспользуемое
  models.ts, api-error.ts, loading/, notifications/, components/, pipes/
layout/    # оболочки
  app-root/ (только <router-outlet />), app-shell/, guest-shell/
features/  # фичи
  auth/    auth.routes.ts, pages/login-page/, pages/register-page/
  recipes/ recipes.routes.ts, pages/, components/
```

### Мок-слой

- `MockDb` хранит данные в `localStorage` под ключом `mock.db.v1`; `read()` отдаёт
  изолированный снимок, `mutate()` — транзакция с откатом при исключении.
- `MockApiClient.call('домен.операция', params)` находит обработчик, выполняет его
  над мок-БД и гарантирует минимальную задержку `MOCK_DELAY_MS = 500`.
- Доменные обработчики — чистые синхронные функции: `auth.*` и `recipes.*`.
- Сессия пользователя хранится отдельно, под ключом `mock.session.userId`, и
  переживает перезагрузку страницы.

### Слои и зависимости

`features → core/services + shared`; `core/services → mock`; `layout → core + shared`;
`mock → shared/models`. Импорт `mock/` допускается только из `core`.

## Маршруты

| URL                | Экран                                  |
| ------------------ | -------------------------------------- |
| `/`                | редирект на `/recipes`                 |
| `/recipes`         | каталог рецептов (публично)            |
| `/recipes/:id`     | карточка рецепта (публично)            |
| `/auth/login`      | вход (только для гостя)                |
| `/auth/register`   | регистрация (только для гостя)         |
