/**
 * RelativeTimePipe — человекочитаемое относительное время («2 часа назад»,
 * «вчера», «3 дня назад»). Чистый пайп без состояния; потребляется фичами
 * в шаблонах. Зависит только от Angular.
 */
import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'relativeTime' })
export class RelativeTimePipe implements PipeTransform {
  transform(value: string | Date): string {
    const date = typeof value === 'string' ? new Date(value) : value;
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMinutes = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMinutes < 1) {
      return 'только что';
    }
    if (diffMinutes < 60) {
      return `${diffMinutes} ${this.plural(diffMinutes, 'минуту', 'минуты', 'минут')} назад`;
    }
    if (diffHours < 24) {
      return `${diffHours} ${this.plural(diffHours, 'час', 'часа', 'часов')} назад`;
    }
    if (diffDays === 1) {
      return 'вчера';
    }
    if (diffDays < 7) {
      return `${diffDays} ${this.plural(diffDays, 'день', 'дня', 'дней')} назад`;
    }
    return date.toLocaleDateString('ru-RU');
  }

  private plural(n: number, one: string, few: string, many: string): string {
    const mod10 = n % 10;
    const mod100 = n % 100;
    if (mod10 === 1 && mod100 !== 11) {
      return one;
    }
    if (mod10 >= 2 && mod10 <= 4 && (mod100 < 10 || mod100 >= 20)) {
      return few;
    }
    return many;
  }
}
