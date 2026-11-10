import { Signal } from '@angular/core';

export type Lang = 'ar' | 'en';

export type TextDirection = 'ltr' | 'rtl';

export type TranslationParams = Readonly<Record<string, string | number>>;

export interface TranslationKeys {}

export type TranslationKey = keyof TranslationKeys & string;

export abstract class I18n {
  abstract readonly lang: Signal<Lang>;

  abstract readonly dir: Signal<TextDirection>;

  abstract use(lang: Lang): void;

  abstract t(key: string, params?: TranslationParams): string;

  abstract has(key: string): boolean;

  abstract formatDate(value: string | Date, options?: Intl.DateTimeFormatOptions): string;

  abstract formatNumber(value: number, options?: Intl.NumberFormatOptions): string;
}
