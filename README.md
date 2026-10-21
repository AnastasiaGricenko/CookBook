# КулиBook — Fullstack

Каталог рецептов «КулиBook»: Angular 20 на фронтенде и ASP.NET Core на бэкенде.
Список рецептов с поиском и пагинацией, карточка рецепта с похожими рецептами,
создание собственного рецепта, вход и регистрация.

- **Фронтенд** (`src/`) — standalone-компоненты, signals, TypeScript `strict`,
  SCSS и собственные дизайн-токены (`src/styles/`), тёмная тема.
- **Бэкенд** (`backend/`) — слоистая архитектура в стиле DDD
  (`Domain → Presentation ← Infrastructure`), REST API `/api/v1`, JWT (HS256)
  в httpOnly-cookie, in-memory заглушка вместо БД, xUnit-тесты.

## Технологии

- Angular 20, `HttpClient` + RxJS (Observable → Promise на границе сервисов);
- TypeScript 5.9 в режиме `strict`, signals, OnPush;
- ASP.NET Core (.NET 10), контроллеры, `ActionResult<T>`, Swagger в Development;
- аутентификация: JWT в httpOnly-cookie `access_token` (`SameSite=Strict`);
- пароли: PBKDF2-SHA256 (210 000 итераций, случайная соль);
- хранилище: in-memory заглушка (без EF Core и без СУБД).

## Быстрый старт (два терминала)

### 1. Бэкенд

```bash
cd backend/CookBook.Api
# один раз: локальный конфиг с секретом (в git не попадает, см. .gitignore)
cp appsettings.Development.json.template appsettings.Development.json
#   …или: dotnet user-secrets set "Auth:JwtKey" "local-dev-jwt-key-please-change-me-32+"
dotnet run
```

- API: <http://localhost:8080/api/v1>
- Swagger UI: <http://localhost:8080/swagger>
- Демо-учётка: **demo@kulibook.ru / demo123** (Мария Козлова, создаётся сидом).

`Auth:JwtKey` — секрет: минимум 32 символа. В `backend/CookBook.Api/appsettings.json`
он пуст, реальное значение живёт только в `appsettings.Development.json`
(gitignored) или в переменной окружения `Auth__JwtKey`. Без ключа приложение
падает на старте с понятным сообщением (`ValidateOnStart`).

### 2. Фронтенд

```bash
npm install
npm start        # http://localhost:4200
npm run build    # production-сборка в dist/
```

Дев-сервер проксирует `/api/*` на `http://localhost:8080` (`proxy.conf.json`,
подключён в `angular.json`), поэтому браузер работает с тем же origin и
httpOnly-cookie сессии ходит с каждым запросом.

## Тесты

```bash
dotnet test backend/CookBook.sln
```

Покрыто: доменные объекты-значения и тексты ошибок, PBKDF2 и JWT, полный CRUD
in-memory репозитория, создание рецепта и его место в каталоге,
интеграционные сценарии API через `WebApplicationFactory`, контракт ошибок и
отсутствие `passwordHash` в ответах.

## API

| Метод, путь | Auth | Успех |
|---|---|---|
| `POST /api/v1/auth/register` | анонимно | 201 + `UserDto` + cookie |
| `POST /api/v1/auth/login` | анонимно | 200 + `UserDto` + cookie |
| `POST /api/v1/auth/logout` | анонимно | 204 |
| `GET /api/v1/auth/me` | cookie | 200 + `UserDto` |
| `GET /api/v1/recipes?query=&page=&pageSize=` | анонимно | 200 + `PagedResult<RecipeCardDto>` |
| `GET /api/v1/recipes/{id}` | анонимно | 200 + `RecipeDto` |
| `GET /api/v1/recipes/{id}/similar` | анонимно | 200 + `RecipeCardDto[]` (≤ 4) |
| `POST /api/v1/recipes` | cookie | 201 + `RecipeDto` (автор — из сессии) |
| `GET /api/v1/health` | анонимно | 200 + `{ status: "ok" }` |

Каталог и похожие рецепты отдаются от новых к старым: только что созданный
рецепт оказывается первым на главной странице.

Единый формат отказа: `{ "message": "…", "errors": { "поле": "текст" } }`
(`errors` — только у 400 валидации формы). Коды: 400/401/404/500.

## Архитектура фронтенда

```
core/
  config/      app.config.ts, app.routes.ts, api-base-url.ts
  http/        api-prefix.interceptor.ts, api-error.interceptor.ts
  navigation/  guards.ts                       (асинхронные authGuard, guestGuard, homeGuard)
  services/    auth.service.ts, recipe.service.ts
shared/        models.ts, api-error.ts, loading/, notifications/, components/, pipes/
layout/        app-root/, app-shell/, guest-shell/
features/      auth/ (login, register), recipes/ (список, создание, карточка, компоненты)
```

Слои и зависимости: `features → core/services + shared`; `layout → core + shared`;
`core/services → HttpClient`; `core/http → shared/models + core/services`.
Все запросы к API — через `HttpClient` с явным методом (`get`/`post`) и
продолжением в `.pipe()`; публичные сигнатуры сервисов остаются `Promise`.

Маршруты фич подключаются лениво (`loadChildren` + `loadComponent`): каждая из
пяти страниц грузится отдельным чанком при первой навигации на свой маршрут.

`loadMe()` мемоизирует `GET /auth/me`: guard'ы и оболочка переиспользуют один
запрос, а после входа/выхода кэш сбрасывается.

## Архитектура бэкенда

```
backend/
  CookBook.sln
  CookBook.Api/
    Program.cs                 # Composition Root: Add* → Build → Seed → middleware
    Domain/                    # сущности, VO, enum, интерфейсы репозиториев и сервисов
    Infrastructure/            # InMemory хранилище, PBKDF2, JWT, Options, сид
    Presentation/              # контроллеры, DTO, маппер, middleware
  CookBook.Tests/              # xUnit: домен, репозитории, API, контракт ошибок
```

Правила слоёв: домен не знает ни про HTTP, ни про хранилище; репозитории
объявлены интерфейсами в домене; реализации и криптография — в инфраструктуре;
контроллеры только парсят запрос и вызывают доменные сервисы.

`RequestLoggingMiddleware` — самый внешний: пишет «до» (`→ метод путь`) и
«после» (`← статус, мс`) обработчика; `ExceptionHandlingMiddleware` превращает
исключения в единый `ErrorResponse`.

## Маршруты фронтенда

| URL | Экран |
| --- | --- |
| `/` | редирект на `/recipes` |
| `/recipes` | каталог рецептов (публично) |
| `/recipes/new` | создание рецепта (только для авторизованных; гость → `/auth/register`) |
| `/recipes/:id` | карточка рецепта (публично) |
| `/auth/login` | вход (только для гостя) |
| `/auth/register` | регистрация (только для гостя) |
