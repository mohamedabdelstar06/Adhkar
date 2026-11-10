import { ChangeDetectionStrategy, Component, effect, inject, input, signal } from '@angular/core';
import { FormField, form, maxLength, min, required } from '@angular/forms/signals';
import { I18n } from '@zad/i18n';
import { ADHKAR_CATEGORIES, AdhkarDto, AdhkarUpsertRequest } from './adhkar.model';
import { AdhkarState } from './adhkar-state';

@Component({
  selector: 'zad-adhkar-update',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormField],
  templateUrl: './adhkar-update.html',
  styleUrl: './adhkar-update.scss'
})
export class AdhkarUpdate {
  readonly i18n = inject(I18n);
  protected readonly state = inject(AdhkarState);
  protected readonly categories = ADHKAR_CATEGORIES;

  readonly dhikr = input.required<AdhkarDto>();

  protected readonly model = signal<AdhkarUpsertRequest>({
    text: '',
    translation: '',
    reference: '',
    category: 'tasbih',
    displayOrder: 1,
    isActive: true
  });

  protected readonly adhkarForm = form(this.model, (path) => {
    required(path.text);
    maxLength(path.text, 1000);
    maxLength(path.translation, 2000);
    maxLength(path.reference, 300);
    required(path.category);
    min(path.displayOrder, 0);
  });

  constructor() {
    effect(() => {
      const current = this.dhikr();
      this.model.set({
        text: current.text,
        translation: current.translation,
        reference: current.reference,
        category: current.category,
        displayOrder: current.displayOrder,
        isActive: current.isActive
      });
    });
  }

  submit(): void {
    if (this.adhkarForm().invalid() || this.state.saving()) {
      return;
    }

    this.state.update(this.dhikr().id, this.model());
  }
}

