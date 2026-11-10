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
  `
})
export class ZadButton {
  readonly label = input.required<string>();
  readonly variant = input<ZadButtonVariant>('primary');
  readonly size = input<'small' | 'large'>('small');
  readonly loading = input(false);
  readonly disabled = input(false);
  readonly pressed = output<void>();

  protected readonly severity = computed(() => severityByVariant[this.variant()]);
}
