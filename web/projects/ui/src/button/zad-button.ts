import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { Button } from 'primeng/button';

export type ZadButtonVariant = 'primary' | 'secondary' | 'danger';

const severityByVariant = {
  primary: undefined,
  secondary: 'secondary',
  danger: 'danger'
} as const;

@Component({
  selector: 'zad-button',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Button],
  template: `
    <p-button
      [label]="label()"
      [severity]="severity()"
      [outlined]="variant() === 'secondary'"
      [loading]="loading()"
      [disabled]="disabled()"
      [size]="size()"
      (onClick)="pressed.emit()"
    />
  `,
  styles: `
    :host {
      display: inline-block;
    }

    :host ::ng-deep .p-button {
      font-size: 0.875rem;
      font-weight: 600;
      line-height: 1.5;
      padding: 0.625rem 1.25rem;
      border-radius: 6px;
      gap: 0.5rem;
      white-space: nowrap;
    }

    :host ::ng-deep .p-button.p-button-sm {
      font-size: 0.8125rem;
      padding: 0.45rem 0.9rem;
    }
  `
})
export class ZadButton {
  readonly label = input.required<string>();
  readonly variant = input<ZadButtonVariant>('primary');
  readonly size = input<'small' | 'large' | undefined>(undefined);
  readonly loading = input(false);
  readonly disabled = input(false);
  readonly pressed = output<void>();

  protected readonly severity = computed(() => severityByVariant[this.variant()]);
}
