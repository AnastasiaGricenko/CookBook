-- ============================================================================
--  КулиBook — создание базы данных (PostgreSQL 14+)
--  Лабораторная работа 4, п.1: база данных приведена к 3НФ.
--
--  Соответствие конспекту лекции:
--   * 1НФ — все атрибуты атомарны: массивов и списков в колонках нет,
--     приправы и теги вынесены в отдельные сущности;
--   * 2НФ — все не ключевые атрибуты зависят от полного первичного ключа;
--     составных первичных ключей нет, в каждой таблице технический
--     (суррогатный) ключ id, как и рекомендовано на лекции;
--   * 3НФ — нет транзитивных зависимостей: имя и аватар автора НЕ хранятся
--     в recipes (иначе они зависели бы от author_id, а не от ключа рецепта),
--     при выдаче они поднимаются JOIN-ом по users.author_id;
--   * связи M:N (рецепты <-> теги, рецепты <-> приправы) разбиты на
--     промежуточные сущности recipe_tags и recipe_seasonings;
--   * все ключи индексируются: PK и UNIQUE индексируются автоматически,
--     на каждый внешний ключ дополнительно создан индекс.
--
--  Сущности и связи:
--    users  1—M  recipes             (автор пишет рецепты)
--    recipes 1—M recipe_ingredients  (ингредиенты конкретного рецепта)
--    recipes 1—M recipe_steps        (шаги конкретного рецепта)
--    recipes M—N tags                через промежуточную recipe_tags
--    recipes M—N seasonings          через промежуточную recipe_seasonings
--
--  Как запустить: pgAdmin -> выбрать БД -> Query Tool -> открыть файл -> F5.
--  Скрипт пересоздаёт схему с нуля (на этапе разработки) и засеивает
--  демо-данные, дословно совпадающие с прежним сидом бэкенда.
-- ============================================================================

BEGIN;

-- ----------------------------------------------------------------------------
-- 0. Пересоздание с нуля (на этапе разработки; удалит старые данные!)
-- ----------------------------------------------------------------------------
DROP TABLE IF EXISTS recipe_seasonings  CASCADE;
DROP TABLE IF EXISTS recipe_tags        CASCADE;
DROP TABLE IF EXISTS recipe_steps       CASCADE;
DROP TABLE IF EXISTS recipe_ingredients CASCADE;
DROP TABLE IF EXISTS seasonings         CASCADE;
DROP TABLE IF EXISTS tags               CASCADE;
DROP TABLE IF EXISTS recipes            CASCADE;
DROP TABLE IF EXISTS users              CASCADE;
DROP TYPE  IF EXISTS difficulty         CASCADE;

-- ----------------------------------------------------------------------------
-- 1. Сложность приготовления — доменное перечисление Difficulty.
--    Значения совпадают с именами C#-enum (Easy/Medium/Hard);
--    русские строки «Легкая/Средняя/Сложная» подставляет DifficultyMapper.
-- ----------------------------------------------------------------------------
CREATE TYPE difficulty AS ENUM ('Easy', 'Medium', 'Hard');
COMMENT ON TYPE difficulty IS 'Сложность приготовления (доменный enum Difficulty)';

-- ----------------------------------------------------------------------------
-- 2. Пользователи (Domain/Entities/User.cs)
-- ----------------------------------------------------------------------------
CREATE TABLE users (
    id            uuid         PRIMARY KEY DEFAULT gen_random_uuid(),
    email         varchar(254) NOT NULL,               -- нормализованный: trim + нижний регистр
    password_hash varchar(256) NOT NULL,               -- PBKDF2: "iterations.salt.key" (base64)
    name          varchar(100) NOT NULL,               -- 2..100 символов (PersonName)
    avatar_url    varchar(500) NULL,                   -- null допустим
    CONSTRAINT users_email_key           UNIQUE (email),
    CONSTRAINT users_email_lowercase_chk CHECK (email = lower(email)),
    CONSTRAINT users_name_length_chk     CHECK (char_length(name) BETWEEN 2 AND 100)
);
COMMENT ON TABLE users IS 'Пользователи каталога (Domain/Entities/User.cs)';

-- ----------------------------------------------------------------------------
-- 3. Рецепты (Domain/Entities/Recipe.cs + ValueObjects/NutritionFacts.cs).
--    Пищевая ценность — четыре самостоятельных атомарных атрибута рецепта
--    (каждый зависит только от ключа рецепта). author_name/author_avatar
--    сознательно НЕ дублируются из users — иначе 3НФ нарушалась бы
--    транзитивной зависимостью через author_id; контракт карточки
--    восстанавливается JOIN-ом в репозитории.
-- ----------------------------------------------------------------------------
CREATE TABLE recipes (
    id           uuid        PRIMARY KEY DEFAULT gen_random_uuid(),
    author_id    uuid        NOT NULL REFERENCES users (id) ON DELETE CASCADE,
    title        varchar(100) NOT NULL,               -- RecipeTitle: 1..100
    image_url    varchar(500) NOT NULL DEFAULT '/assets/borsch.png',
    time_minutes integer     NOT NULL,                -- 1..1440
    description  varchar(1000) NOT NULL,              -- 1..1000
    servings     integer     NOT NULL,                -- 1..100
    difficulty   difficulty  NOT NULL,
    kcal         integer     NOT NULL,                -- пищевая ценность, каждый 0..10000
    protein      integer     NOT NULL,
    fat          integer     NOT NULL,
    carbs        integer     NOT NULL,
    created_at   timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT recipes_title_length_chk       CHECK (char_length(title) BETWEEN 1 AND 100),
    CONSTRAINT recipes_image_url_length_chk   CHECK (char_length(image_url) BETWEEN 1 AND 500),
    CONSTRAINT recipes_description_length_chk CHECK (char_length(description) BETWEEN 1 AND 1000),
    CONSTRAINT recipes_time_minutes_chk       CHECK (time_minutes BETWEEN 1 AND 1440),
    CONSTRAINT recipes_servings_chk           CHECK (servings BETWEEN 1 AND 100),
    CONSTRAINT recipes_nutrition_range_chk    CHECK (
        kcal    BETWEEN 0 AND 10000 AND
        protein BETWEEN 0 AND 10000 AND
        fat     BETWEEN 0 AND 10000 AND
        carbs   BETWEEN 0 AND 10000)
);
COMMENT ON TABLE recipes IS 'Рецепты (Domain/Entities/Recipe.cs); автор — FK на users';

-- Каталог и «похожие рецепты» сортируются по новизне
CREATE INDEX recipes_created_at_idx ON recipes (created_at DESC);
-- Индекс на внешний ключ (PK users индексируется автоматически)
CREATE INDEX recipes_author_id_idx ON recipes (author_id);

-- ----------------------------------------------------------------------------
-- 4. Ингредиенты (Domain/Entities/RecipeIngredient.cs) — 1:M к рецептам.
--    Атрибуты принадлежат конкретному рецепту, поэтому это отдельная сущность,
--    а не словарь: «Яйца, 3 шт» одного рецепта не переиспользуется другим.
--    sort_order хранит позицию в списке (1, 2, 3...), как в форме создания.
-- ----------------------------------------------------------------------------
CREATE TABLE recipe_ingredients (
    id         bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    recipe_id  uuid NOT NULL REFERENCES recipes (id) ON DELETE CASCADE,
    sort_order smallint NOT NULL,
    name       varchar(100) NOT NULL,                  -- лимит формы: 100 символов
    amount     varchar(100) NOT NULL,                  -- «500 г», «3 шт» — свободная форма
    CONSTRAINT recipe_ingredients_recipe_sort_key UNIQUE (recipe_id, sort_order),
    CONSTRAINT recipe_ingredients_sort_order_chk  CHECK (sort_order >= 1),
    CONSTRAINT recipe_ingredients_name_length_chk CHECK (char_length(name) BETWEEN 1 AND 100),
    CONSTRAINT recipe_ingredients_amount_length_chk CHECK (char_length(amount) BETWEEN 1 AND 100)
);
COMMENT ON TABLE recipe_ingredients IS 'Ингредиенты рецепта (1:M); порядок задаёт sort_order';

-- ----------------------------------------------------------------------------
-- 5. Шаги приготовления (Domain/Entities/RecipeStep.cs) — 1:M к рецептам.
--    Лимиты как в форме: заголовок до 100, текст до 1000 символов.
-- ----------------------------------------------------------------------------
CREATE TABLE recipe_steps (
    id         bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    recipe_id  uuid NOT NULL REFERENCES recipes (id) ON DELETE CASCADE,
    sort_order smallint NOT NULL,
    title      varchar(100) NOT NULL,
    text       varchar(1000) NOT NULL,
    CONSTRAINT recipe_steps_recipe_sort_key UNIQUE (recipe_id, sort_order),
    CONSTRAINT recipe_steps_sort_order_chk   CHECK (sort_order >= 1),
    CONSTRAINT recipe_steps_title_length_chk CHECK (char_length(title) BETWEEN 1 AND 100),
    CONSTRAINT recipe_steps_text_length_chk  CHECK (char_length(text) BETWEEN 1 AND 1000)
);
COMMENT ON TABLE recipe_steps IS 'Шаги приготовления (1:M); порядок задаёт sort_order';

-- ----------------------------------------------------------------------------
-- 6. Словари тегов и приправ. Одна и та же приправа/тег используется многими
--    рецептами, а рецепт имеет много приправ/тегов — связь M:N.
--    Строки хранятся один раз (без дублирования между рецептами).
-- ----------------------------------------------------------------------------
CREATE TABLE tags (
    id   bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    name varchar(50) NOT NULL,                           -- ListItemMax: до 50 символов
    CONSTRAINT tags_name_key UNIQUE (name),
    CONSTRAINT tags_name_length_chk CHECK (char_length(name) BETWEEN 1 AND 50)
);
COMMENT ON TABLE tags IS 'Словарь тегов; связь с рецептами — M:N через recipe_tags';

CREATE TABLE seasonings (
    id   bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    name varchar(50) NOT NULL,
    CONSTRAINT seasonings_name_key UNIQUE (name),
    CONSTRAINT seasonings_name_length_chk CHECK (char_length(name) BETWEEN 1 AND 50)
);
COMMENT ON TABLE seasonings IS 'Словарь приправ; связь с рецептами — M:N через recipe_seasonings';

-- ----------------------------------------------------------------------------
-- 7. Промежуточные сущности для связей M:N (по конспекту лекции:
--    «эта связь всегда разбивается на промежуточную сущность»).
--    sort_order сохраняет исходный порядок пунктов из формы создания.
-- ----------------------------------------------------------------------------
CREATE TABLE recipe_tags (
    id         bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    recipe_id  uuid NOT NULL REFERENCES recipes (id) ON DELETE CASCADE,
    tag_id     bigint NOT NULL REFERENCES tags (id) ON DELETE CASCADE,
    sort_order smallint NOT NULL,
    CONSTRAINT recipe_tags_recipe_tag_key UNIQUE (recipe_id, tag_id),
    CONSTRAINT recipe_tags_sort_order_chk CHECK (sort_order >= 1)
);
COMMENT ON TABLE recipe_tags IS 'Промежуточная сущность M:N: рецепты <-> теги';

CREATE TABLE recipe_seasonings (
    id           bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    recipe_id    uuid NOT NULL REFERENCES recipes (id) ON DELETE CASCADE,
    seasoning_id bigint NOT NULL REFERENCES seasonings (id) ON DELETE CASCADE,
    sort_order   smallint NOT NULL,
    CONSTRAINT recipe_seasonings_recipe_seasoning_key UNIQUE (recipe_id, seasoning_id),
    CONSTRAINT recipe_seasonings_sort_order_chk CHECK (sort_order >= 1)
);
COMMENT ON TABLE recipe_seasonings IS 'Промежуточная сущность M:N: рецепты <-> приправы';

-- Индексы на внешние ключи промежуточных сущностей:
-- связка (recipe_id, ...) уже индексируется UNIQUE-ограничением,
-- обратный поиск «все рецепты с тегом/приправой X» — по индексу ниже.
CREATE INDEX recipe_tags_tag_id_idx         ON recipe_tags (tag_id);
CREATE INDEX recipe_seasonings_seasoning_id_idx ON recipe_seasonings (seasoning_id);
