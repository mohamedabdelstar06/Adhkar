import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'zad-page-header',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <header class="page-header">
      <div class="page-header-text">
        <h1>{{ title() }}</h1>
        @if (subtitle(); as text) {
          <p>{{ text }}</p>
        }
      </div>
      <div class="page-header-actions">
        <ng-content></ng-content>
      </div>
    </header>
  `,
  styles: `
    .page-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 0.75rem 1rem;
      margin-block-end: var(--zad-space-6);
      flex-wrap: wrap;
    }

    .page-header-text {
      min-inline-size: 0;
    }

    h1 {
      margin: 0;
      font-size: 1.5rem;
      font-weight: 700;
      color: var(--ds-color-primary);
    }

    p {
      margin: var(--zad-space-1) 0 0;
      color: var(--ds-color-on-surface-muted);
      font-size: 0.875rem;
      max-inline-size: 60ch;
    }

    .page-header-actions {
      display: flex;
      gap: var(--zad-space-2);
      align-items: center;
      flex-wrap: wrap;
    }

    @media (max-width: 640px) {
      .page-header {
        flex-direction: column;
        align-items: stretch;
        margin-block-end: var(--zad-space-4);
      }

      h1 {
        font-size: 1.25rem;
      }

      .page-header-actions {
        justify-content: flex-start;
      }
    }
  `
})
export class ZadPageHeader {
  readonly title = input.required<string>();
  readonly subtitle = input<string | null>(null);
}
