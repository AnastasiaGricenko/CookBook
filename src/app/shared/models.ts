/**
 * shared/models.ts — единый источник транспортных типов и констант «КулиBook».
 *
 * Роль в архитектуре: фундамент слоя shared. Потребляется всеми слоями
 * (mock, core, features, layout), но сам не импортирует ничего, кроме Angular
 * и самого shared. Здесь описаны только формы данных REST-подобного контракта
 * и неизменяемые константы приложения; доменная логика живёт в других слоях.
 *
 * Гарантии:
 *  - ни одного импорта из core/mock/features/layout (иначе возникнет цикл слоёв);
 *  - транспортные формы и константы — единственный источник для всех слоёв.
 */

export type Difficulty = 'Легкая' | 'Средняя' | 'Сложная';

export interface Nutrition {
  kcal: number;
  protein: number;
  fat: number;
  carbs: number;
}

export interface RecipeIngredient {
  name: string;
  amount: string;
}

export interface RecipeStep {
  title: string;
  text: string;
}

export interface RecipeCard {
  id: string;
  title: string;
  imageUrl: string;
  timeMinutes: number;
  kcal: number;
  author: string;
}

export interface Recipe extends RecipeCard {
  description: string;
  authorId: string;
  authorAvatar: string;
  servings: number;
  difficulty: Difficulty;
  nutrition: Nutrition;
  ingredients: RecipeIngredient[];
  seasonings: string[];
  steps: RecipeStep[];
  tags: string[];
  isFavorite: boolean;
}

export interface User {
  id: string;
  email: string;
  name: string;
  avatarUrl: string | null;
}

/** Транспортная форма текущего пользователя (ответ auth.me / login / register). */
export type MeDto = User;

export interface LoginParams {
  email: string;
  password: string;
}

export interface RegisterParams {
  name: string;
  email: string;
  password: string;
}

export interface PagedResult<T> {
  items: T[];
  total: number;
}

/** Ошибка транспорта: сигнал отказа мок-слоя, поднимаемый до страниц. */
export interface ApiError {
  status: number;
  body: { message: string; errors?: Record<string, string> };
}

/** Минимальная длительность любого мок-вызова — эмуляция задержки сети. */
export const MOCK_DELAY_MS = 500;

/** Размер страницы каталога рецептов. */
export const RECIPES_PAGE_SIZE = 8;

/** Ключи localStorage, зарезервированные мок-слоем. */
export const STORAGE_KEYS = {
  mockDb: 'mock.db.v1',
  session: 'mock.session.userId',
} as const;
