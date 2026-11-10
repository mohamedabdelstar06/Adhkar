import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { InputText } from 'primeng/inputtext';

@Component({
  selector: 'zad-text-field',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [InputText],
  template: `
    <label class="field">
      @if (label(); as text) {
        <span class="field-label">{{ text }}</span>
      }
      <input
        pInputText
        [type]="type()"
        [value]="value()"
        [placeholder]="placeholder()"
        [disabled]="disabled()"
        (input)="valueChange.emit($any($event.target).value)"
      />
    </label>
  `,
  styles: `
    .field {
      display: flex;
      flex-direction: column;
      gap: var(--zad-space-1);
      inline-size: 100%;
    }

    .field-label {
      font-size: 0.85rem;
      font-weight: 600;
      color: var(--ds-color-on-surface-muted);
    }
  `
})
export class ZadTextField {
  readonly label = input<string | null>(null);
  readonly value = input<string>('');
  readonly type = input<'text' | 'email' | 'number'>('text');
  readonly placeholder = input<string>('');
  readonly disabled = input(false);
  readonly valueChange = output<string>();
}
