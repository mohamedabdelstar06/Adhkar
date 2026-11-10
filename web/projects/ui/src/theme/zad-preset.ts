import { definePreset } from '@primeuix/themes';
import Aura from '@primeuix/themes/aura';

export const ZadPreset = definePreset(Aura, {
  semantic: {
    primary: {
      50: 'var(--zad-color-teal-500)',
      100: 'var(--zad-color-teal-500)',
      200: 'var(--zad-color-teal-500)',
      300: 'var(--zad-color-teal-500)',
      400: 'var(--zad-color-teal-500)',
      500: 'var(--zad-color-teal-500)',
      600: 'var(--zad-color-teal-600)',
      700: 'var(--zad-color-teal-700)',
      800: 'var(--zad-color-teal-700)',
      900: 'var(--zad-color-teal-700)',
      950: 'var(--zad-color-teal-700)'
    },
    colorScheme: {
      light: {
        primary: {
          color: 'var(--ds-color-primary)',
          contrastColor: 'var(--ds-color-on-primary)',
          hoverColor: 'var(--ds-color-primary-strong)',
          activeColor: 'var(--ds-color-primary-strong)'
        },
        surface: {
          0: 'var(--ds-color-surface)',
          50: 'var(--ds-color-surface-muted)',
          100: 'var(--ds-color-surface-muted)',
          200: 'var(--ds-color-border)',
          300: 'var(--ds-color-border)',
          400: 'var(--ds-color-on-surface-muted)',
          500: 'var(--ds-color-on-surface-muted)',
          600: 'var(--ds-color-on-surface-muted)',
          700: 'var(--ds-color-on-surface)',
          800: 'var(--ds-color-on-surface)',
          900: 'var(--ds-color-on-surface)',
          950: 'var(--ds-color-on-surface)'
        }
      }
    }
  }
});
