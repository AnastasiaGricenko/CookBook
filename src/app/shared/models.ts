/**
 * shared/models.ts — единый источник транспортных типов и констант «КулиBook».
 *
 * Роль в архитектуре: фундамент слоя shared. Потребляется всеми слоями
 * (core, features, layout), но сам не импортирует ничего, кроме Angular
 * и самого shared. Здесь описаны только транспортные формы REST-контракта
 * с сервером и неизменяемые константы приложения; доменная логика живёт
 * в других слоях.
 *
 * Гарантии:
 *  - ни одного импорта из core/features/layout (иначе возникнет цикл слоёв);
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

/** Тело запроса создания рецепта: автора сервер берёт из сессии. */
export interface CreateRecipeParams {
  title: string;
  imageUrl: string;
  timeMinutes: number;
  description: string;
  servings: number;
  difficulty: Difficulty;
  nutrition: Nutrition;
  ingredients: RecipeIngredient[];
  seasonings: string[];
  steps: RecipeStep[];
  tags: string[];
}

export interface PagedResult<T> {
  items: T[];
  total: number;
}

/** Ошибка транспорта: нормализованный отказ REST API, поднимаемый до страниц. */
export interface ApiError {
  status: number;
  body: { message: string; errors?: Record<string, string> };
}

/** Размер страницы каталога рецептов. */
export const RECIPES_PAGE_SIZE = 8;
