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
      align-items: flex-start;
      justify-content: space-between;
      gap: var(--ds-space-inline);
      margin-block-end: var(--zad-space-6);
      flex-wrap: wrap;
    }

    h1 {
      margin: 0;
      font-size: 1.5rem;
      font-weight: 700;
      color: var(--ds-color-on-surface);
    }

    p {
      margin: var(--zad-space-1) 0 0;
      color: var(--ds-color-on-surface-muted);
      font-size: 0.9rem;
      max-inline-size: 60ch;
    }

    .page-header-actions {
      display: flex;
      gap: var(--zad-space-2);
      align-items: center;
    }
  `
})
export class ZadPageHeader {
  readonly title = input.required<string>();
  readonly subtitle = input<string | null>(null);
}
