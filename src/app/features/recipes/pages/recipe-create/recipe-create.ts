/**
 * RecipeCreatePage — страница создания рецепта.
 *
 * Роль в архитектуре: страница-контейнер фичи recipes. Владеет формой с
 * динамическими списками ингредиентов и шагов, вызывает RecipeService и
 * уводит автора на карточку созданного рецепта.
 *
 * Гарантии:
 *  - страница доступна только авторизованным (authGuard на маршруте);
 *  - защита от двойного сабмита через контроллер submitting();
 *  - автора сервер берёт из сессии — в теле запроса его нет;
 *  - списки «приправы» и «теги» вводятся строкой через запятую и нормализуются.
 */
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { RecipeService } from '../../../../core/services/recipe.service';
import { toApiError } from '../../../../shared/api-error';
import { submitting } from '../../../../shared/loading/submitting';
import { NotificationService } from '../../../../shared/notifications/notification-service';
import { Difficulty } from '../../../../shared/models';

/** Доступные изображения: своих ассетов у пользователя нет, выбор из каталога. */
const IMAGE_OPTIONS = [
  { value: '/assets/borsch.png', label: 'Борщ' },
  { value: '/assets/pasta-karbonara.png', label: 'Паста' },
  { value: '/assets/plov.png', label: 'Плов' },
  { value: '/assets/olivie.png', label: 'Салат' },
  { value: '/assets/bliny.png', label: 'Блины' },
  { value: '/assets/shashlik.png', label: 'Шашлык' },
  { value: '/assets/napoleon.png', label: 'Десерт' },
  { value: '/assets/solyanka.png', label: 'Солянка' },
] as const;

/** Строка «через запятую» → нормализованный список без пустых и дублей. */
function splitList(value: string): string[] {
  const seen = new Set<string>();

  return value
    .split(',')
    .map(item => item.trim())
    .filter(item => item.length > 0)
    .filter(item => {
      const key = item.toLowerCase();
      if (seen.has(key)) {
        return false;
      }
      seen.add(key);
      return true;
    });
}

@Component({
  selector: 'app-recipe-create',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './recipe-create.html',
  styleUrl: './recipe-create.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RecipeCreatePage {
  private readonly fb = inject(FormBuilder);
  private readonly recipesService = inject(RecipeService);
  private readonly router = inject(Router);
  private readonly notifications = inject(NotificationService);

  protected readonly submitting = submitting();
  protected readonly imageOptions = IMAGE_OPTIONS;

  protected readonly form = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(100)]],
    imageUrl: [IMAGE_OPTIONS[0].value, [Validators.required]],
    timeMinutes: [30, [Validators.required, Validators.min(1), Validators.max(1440)]],
    servings: [2, [Validators.required, Validators.min(1), Validators.max(100)]],
    difficulty: ['Средняя' as Difficulty, [Validators.required]],
    description: ['', [Validators.required, Validators.maxLength(1000)]],
    nutrition: this.fb.group({
      kcal: [0, [Validators.required, Validators.min(0), Validators.max(10000)]],
      protein: [0, [Validators.required, Validators.min(0), Validators.max(10000)]],
      fat: [0, [Validators.required, Validators.min(0), Validators.max(10000)]],
      carbs: [0, [Validators.required, Validators.min(0), Validators.max(10000)]],
    }),
    ingredients: this.fb.array([this.ingredientRow()]),
    seasonings: [''],
    steps: this.fb.array([this.stepRow()]),
    tags: [''],
  });

  protected get ingredients(): FormArray<FormGroup> {
    return this.form.controls.ingredients;
  }

  protected get steps(): FormArray<FormGroup> {
    return this.form.controls.steps;
  }

  protected addIngredient(): void {
    this.ingredients.push(this.ingredientRow());
  }

  protected removeIngredient(index: number): void {
    if (this.ingredients.length > 1) {
      this.ingredients.removeAt(index);
    }
  }

  protected addStep(): void {
    this.steps.push(this.stepRow());
  }

  protected removeStep(index: number): void {
    if (this.steps.length > 1) {
      this.steps.removeAt(index);
    }
  }

  protected onSubmit(): void {
    if (this.form.invalid || this.submitting.active()) {
      this.form.markAllAsTouched();
      return;
    }
    void this.submit();
  }

  private async submit(): Promise<void> {
    try {
      const value = this.form.getRawValue();

      const created = await this.submitting.run(() =>
        this.recipesService.create({
          title: value.title ?? '',
          imageUrl: value.imageUrl ?? '',
          timeMinutes: Number(value.timeMinutes ?? 0),
          description: value.description ?? '',
          servings: Number(value.servings ?? 0),
          difficulty: (value.difficulty ?? 'Средняя') as Difficulty,
          nutrition: {
            kcal: Number(value.nutrition.kcal ?? 0),
            protein: Number(value.nutrition.protein ?? 0),
            fat: Number(value.nutrition.fat ?? 0),
            carbs: Number(value.nutrition.carbs ?? 0),
          },
          ingredients: value.ingredients.map(row => ({
            name: (row['name'] as string | null) ?? '',
            amount: (row['amount'] as string | null) ?? '',
          })),
          seasonings: splitList(value.seasonings ?? ''),
          steps: value.steps.map(row => ({
            title: (row['title'] as string | null) ?? '',
            text: (row['text'] as string | null) ?? '',
          })),
          tags: splitList(value.tags ?? ''),
        }),
      );

      if (!created) {
        return;
      }

      this.notifications.show('Рецепт добавлен', 'success');
      await this.router.navigate(['/recipes', created.id]);
    } catch (error) {
      this.notifications.show(toApiError(error).body.message, 'error');
    }
  }

  private ingredientRow(): FormGroup {
    return this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      amount: ['', [Validators.required, Validators.maxLength(100)]],
    });
  }

  private stepRow(): FormGroup {
    return this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(100)]],
      text: ['', [Validators.required, Validators.maxLength(1000)]],
    });
  }
}
