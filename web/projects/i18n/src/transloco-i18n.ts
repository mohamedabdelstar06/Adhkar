import { computed, effect, inject, Injectable, signal } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { I18n, Lang, TextDirection, TranslationParams } from './i18n';

@Injectable({ providedIn: 'root' })
export class TranslocoI18n extends I18n {
  private readonly transloco = inject(TranslocoService);
  private readonly current = signal<Lang>('ar');

  readonly lang = this.current.asReadonly();

  readonly dir = computed<TextDirection>(() => (this.current() === 'ar' ? 'rtl' : 'ltr'));

  constructor() {
    super();

    effect(() => {
      const lang = this.current();

      this.transloco.setActiveLang(lang);

      if (typeof document !== 'undefined') {
        document.documentElement.lang = lang;
        document.documentElement.dir = this.dir();
      }
    });
  }

  use(lang: Lang): void {
    this.current.set(lang);
    this.transloco.load(lang).subscribe();
  }

  t(key: string, params?: TranslationParams): string {
    this.current();

    return this.transloco.translate(key, params);
  }

  has(key: string): boolean {
    return this.transloco.translate(key) !== key;
  }

  formatDate(value: string | Date, options?: Intl.DateTimeFormatOptions): string {
    const date = value instanceof Date ? value : new Date(value);

    return new Intl.DateTimeFormat(this.locale(), options ?? { dateStyle: 'medium' }).format(date);
  }

  formatNumber(value: number, options?: Intl.NumberFormatOptions): string {
    return new Intl.NumberFormat(this.locale(), options).format(value);
  }

  private locale(): string {
    return this.current() === 'ar' ? 'ar-EG' : 'en-US';
  }
}
