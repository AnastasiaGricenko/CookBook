/**
 * features/recipes/recipes.routes.ts — маршруты фичи «Рецепты».
 *
 * Роль в архитектуре: агрегатор фичи. Владеет путями списка ('') и карточки
 * рецепта (':id'). Монтируется оболочкой AppShell в core/config/app.routes.ts;
 * сам о вложенности оболочки ничего не знает.
 */
import { Routes } from '@angular/router';
import { RecipesPage } from './pages/recipes-page/recipes-page';
import { RecipeDetailPage } from './pages/recipe-detail/recipe-detail';

export const RECIPES_ROUTES: Routes = [
  { path: '', component: RecipesPage, title: 'КулиBook — Рецепты' },
  { path: ':id', component: RecipeDetailPage, title: 'КулиBook — Рецепт' },
];
