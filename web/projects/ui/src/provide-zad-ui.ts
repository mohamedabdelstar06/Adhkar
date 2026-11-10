import { EnvironmentProviders, makeEnvironmentProviders } from '@angular/core';
import { providePrimeNG } from 'primeng/config';
import { ZadPreset } from './theme/zad-preset';

export function provideZadUi(): EnvironmentProviders {
  return makeEnvironmentProviders([
    providePrimeNG({
      theme: {
        preset: ZadPreset,
        options: {
          darkModeSelector: "[data-theme='dark']",
          cssLayer: {
            name: 'primeng',
            order: 'zad-tokens, primeng'
          }
        }
      },
      ripple: false
    })
  ]);
}
