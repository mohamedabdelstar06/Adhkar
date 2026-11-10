import { HttpClient } from '@angular/common/http';
import { EnvironmentProviders, inject, makeEnvironmentProviders, provideAppInitializer } from '@angular/core';
import { provideTransloco, Translation, TranslocoLoader, TranslocoService } from '@jsverse/transloco';
import { firstValueFrom } from 'rxjs';
import { I18n } from './i18n';
import { TranslocoI18n } from './transloco-i18n';

export const SUPPORTED_LANGUAGES = ['ar', 'en'] as const;

export class TranslocoHttpLoader implements TranslocoLoader {
  private readonly http = inject(HttpClient);

  getTranslation(lang: string) {
    return this.http.get<Translation>(`/i18n/${lang}.json`);
  }
}

export function provideI18n(): EnvironmentProviders {
  return makeEnvironmentProviders([
    provideTransloco({
      config: {
        availableLangs: [...SUPPORTED_LANGUAGES],
        defaultLang: 'ar',
        fallbackLang: 'en',
        reRenderOnLangChange: true,
        prodMode: true,
        missingHandler: {
          useFallbackTranslation: true,
          logMissingKey: false
        }
      },
      loader: TranslocoHttpLoader
    }),
    { provide: I18n, useClass: TranslocoI18n },
    provideAppInitializer(() => firstValueFrom(inject(TranslocoService).load('ar')))
  ]);
}
