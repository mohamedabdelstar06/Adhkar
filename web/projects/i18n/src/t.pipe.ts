import { Pipe, PipeTransform, inject } from '@angular/core';
import { I18n, TranslationParams } from './i18n';

@Pipe({
  name: 't',
  pure: false
})
export class TPipe implements PipeTransform {
  private readonly i18n = inject(I18n);

  transform(key: string, params?: TranslationParams): string {
    return this.i18n.t(key, params);
  }
}
