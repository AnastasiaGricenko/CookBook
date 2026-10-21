/**
 * features/recipes/recipes.routes.ts — маршруты фичи «Рецепты».
 *
 * Роль в архитектуре: агрегатор фичи. Владеет путями списка (''), создания
 * ('new') и карточки рецепта (':id'). Монтируется оболочкой AppShell в
 * core/config/app.routes.ts; сам о вложенности оболочки ничего не знает.
 *
 * Гарантии:
 *  - 'new' объявлен раньше ':id', иначе создание перехватится как идентификатор;
 *  - создание закрыто authGuard: гость уходит на /auth/register;
 *  - каждая страница подключена loadComponent — чанк грузится при первой
 *    навигации; компоненты страниц и их зависимости в стартовый бандл
 *    не попадают.
 */
import { Routes } from '@angular/router';
import { authGuard } from '../../core/navigation/guards';

export const RECIPES_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/recipes-page/recipes-page').then(m => m.RecipesPage),
    title: 'КулиBook — Рецепты',
  },
  {
    path: 'new',
    loadComponent: () =>
      import('./pages/recipe-create/recipe-create').then(m => m.RecipeCreatePage),
    canActivate: [authGuard],
    title: 'КулиBook — Новый рецепт',
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./pages/recipe-detail/recipe-detail').then(m => m.RecipeDetailPage),
    title: 'КулиBook — Рецепт',
  },
];
