/**
 * core/services/auth.service.ts — сервис аутентификации.
 *
 * Роль в архитектуре: тонкий слой над REST API домена auth. Страницы и
 * оболочки зависят только от этого сервиса, а не от HttpClient напрямую.
 * Единственное место, где приложение читает текущего пользователя.
 *
 * Гарантии:
 *  - currentUser — read-only сигнал, обновляется login/register/loadMe/logout;
 *  - loadMe мемоизируется: повторные вызовы guard'ов не порождают запросов;
 *  - 401 при восстановлении сессии означает гостя, а не ошибку экрана;
 *  - httpOnly-cookie сессии ставится сервером, токен в JS не попадает.
 */
import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { catchError, firstValueFrom, of, tap } from 'rxjs';
import { LoginParams, MeDto, RegisterParams } from '../../shared/models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly currentUserState = signal<MeDto | null>(null);

  /** Кэш восстановления сессии: Promise выполняется один раз. */
  private mePromise: Promise<MeDto | null> | null = null;

  readonly currentUser = this.currentUserState.asReadonly();

  /** Вход: сервер проверяет данные и ставит httpOnly-cookie. */
  async login(params: LoginParams): Promise<MeDto> {
    return firstValueFrom(
      this.http.post<MeDto>('auth/login', params).pipe(
        tap(user => this.setCurrentUser(user)),
      ),
    );
  }

  /** Регистрация с автовходом (сервер тоже ставит cookie). */
  async register(params: RegisterParams): Promise<MeDto> {
    return firstValueFrom(
      this.http.post<MeDto>('auth/register', params).pipe(
        tap(user => this.setCurrentUser(user)),
      ),
    );
  }

  /** Выход: сервер удаляет cookie; локальное состояние сбрасывается в любом случае. */
  async logout(): Promise<void> {
    try {
      await firstValueFrom(
        this.http.post<void>('auth/logout', null).pipe(
          catchError(() => of(void 0)),
        ),
      );
    } finally {
      this.handleUnauthorized();
    }
  }

  /** Восстанавливает текущего пользователя по cookie (например, после F5). */
  async loadMe(): Promise<MeDto | null> {
    this.mePromise ??= firstValueFrom(
      this.http.get<MeDto>('auth/me').pipe(
        catchError(() => of<MeDto | null>(null)),
      ),
    ).then(user => {
      this.currentUserState.set(user);
      return user;
    });

    return this.mePromise;
  }

  /** Сброс кэша пользователя — вызывается интерцептором ошибок при 401. */
  handleUnauthorized(): void {
    this.mePromise = null;
    this.currentUserState.set(null);
  }

  private setCurrentUser(user: MeDto): void {
    this.mePromise = Promise.resolve(user);
    this.currentUserState.set(user);
  }
}
