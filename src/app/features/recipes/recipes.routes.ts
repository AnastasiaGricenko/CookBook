/**
 * features/recipes/recipes.routes.ts — маршруты фичи «Рецепты».
 *
 * Роль в архитектуре: агрегатор фичи. Владеет путями списка (''), создания
 * ('new') и карточки рецепта (':id'). Монтируется оболочкой AppShell в
 * core/config/app.routes.ts; сам о вложенности оболочки ничего не знает.
 *
 * Гарантии:
 *  - 'new' объявлен раньше ':id', иначе создание перехватится как идентификатор;
 *  - создание закрыто authGuard: гость уходит на /auth/register.
 */
import { Routes } from '@angular/router';
import { authGuard } from '../../core/navigation/guards';
import { RecipesPage } from './pages/recipes-page/recipes-page';
import { RecipeDetailPage } from './pages/recipe-detail/recipe-detail';
import { RecipeCreatePage } from './pages/recipe-create/recipe-create';

export const RECIPES_ROUTES: Routes = [
  { path: '', component: RecipesPage, title: 'КулиBook — Рецепты' },
  {
    path: 'new',
    component: RecipeCreatePage,
    canActivate: [authGuard],
    title: 'КулиBook — Новый рецепт',
  },
  { path: ':id', component: RecipeDetailPage, title: 'КулиBook — Рецепт' },
];
