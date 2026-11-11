import { HttpClient } from '@angular/common/http';
import { EnvironmentProviders, inject, makeEnvironmentProviders, provideAppInitializer } from '@angular/core';
import { provideTransloco, Translation, TranslocoLoader, TranslocoService } from '@jsverse/transloco';
import { firstValueFrom, forkJoin } from 'rxjs';
import { I18n } from './i18n';
import { TranslocoI18n } from './transloco-i18n';

export const SUPPORTED_LANGUAGES = ['ar', 'en'] as const;

const FALLBACK_BUILD_VERSION = Date.now().toString(36);

function resolveBundleVersion(): string {
  if (typeof document !== 'undefined') {
    const mainScript = document.querySelector('script[src*="main-"]')?.getAttribute('src');
    if (mainScript) {
      return mainScript.replace(/[^a-zA-Z0-9._-]/g, '');
    }
  }

  return FALLBACK_BUILD_VERSION;
}

export class TranslocoHttpLoader implements TranslocoLoader {
  private readonly http = inject(HttpClient);
  private readonly version = resolveBundleVersion();

  getTranslation(lang: string) {
    return this.http.get<Translation>(`/i18n/${lang}.json?v=${encodeURIComponent(this.version)}`);
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
    provideAppInitializer(() => {
      const transloco = inject(TranslocoService);
      return firstValueFrom(forkJoin([transloco.load('ar'), transloco.load('en')]));
    })
  ]);
}
