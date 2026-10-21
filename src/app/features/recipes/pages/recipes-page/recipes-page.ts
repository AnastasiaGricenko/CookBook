/**
 * RecipesPage — страница каталога рецептов.
 *
 * Роль в архитектуре: страница-контейнер фичи recipes. Владеет поиском и
 * состоянием списка, вызывает RecipeService. Никакой фильтрации тут нет —
 * её выполняет сервер, а сервис лишь прокидывает параметры.
 *
 * Гарантии:
 *  - поиск дебаунсится (400 мс) и не дублирует одинаковые запросы;
 *  - значение из query-параметра ?q синхронизирует поле без повторного debounce;
 *  - данные приходят через async/await (fire-and-forget с void this.load()).
 */
import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { RecipeService } from '../../../../core/services/recipe.service';
import { RecipeCard as RecipeCardModel } from '../../../../shared/models';
import { ProgressBar } from '../../../../shared/components/progress-bar/progress-bar';
import { RecipeCard } from '../../../../shared/components/recipe-card/recipe-card';

@Component({
  selector: 'app-recipes-page',
  imports: [ReactiveFormsModule, RecipeCard, ProgressBar],
  templateUrl: './recipes-page.html',
  styleUrl: './recipes-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RecipesPage implements OnInit {
  private readonly recipesService = inject(RecipeService);
  private readonly route = inject(ActivatedRoute);

  protected readonly searchControl = new FormControl('', { nonNullable: true });

  private readonly recipesState = signal<RecipeCardModel[]>([]);
  private readonly totalState = signal(0);
  private readonly loadingState = signal(false);

  protected readonly recipes = this.recipesState.asReadonly();
  protected readonly total = this.totalState.asReadonly();
  protected readonly loading = this.loadingState.asReadonly();

  ngOnInit(): void {
    this.searchControl.valueChanges
      .pipe(debounceTime(400), distinctUntilChanged())
      .subscribe(query => void this.load(query));

    this.route.queryParamMap.subscribe(params => {
      const query = params.get('q') ?? '';
      if (query !== this.searchControl.value) {
        // emitEvent: false — иначе значение повторно пройдёт через debounceTime.
        this.searchControl.setValue(query, { emitEvent: false });
      }
      void this.load(query);
    });
  }

  private async load(query: string): Promise<void> {
    this.loadingState.set(true);
    try {
      const result = await this.recipesService.getList({ query, page: 1 });
      this.recipesState.set(result.items);
      this.totalState.set(result.total);
    } finally {
      this.loadingState.set(false);
    }
  }
}
