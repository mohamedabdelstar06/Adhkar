import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormField, form, maxLength, min, required } from '@angular/forms/signals';
import { I18n } from '@zad/i18n';
import { ADHKAR_CATEGORIES, AdhkarUpsertRequest } from './adhkar.model';
import { AdhkarState } from './adhkar-state';

@Component({
  selector: 'zad-adhkar-create',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormField],
  templateUrl: './adhkar-create.html',
  styleUrl: './adhkar-create.scss'
})
export class AdhkarCreate {
  readonly i18n = inject(I18n);
  protected readonly state = inject(AdhkarState);
  protected readonly categories = ADHKAR_CATEGORIES;

  protected readonly model = signal<AdhkarUpsertRequest>({
    text: '',
    translation: '',
    reference: '',
    category: 'tasbih',
    displayOrder: this.state.items().length + 1,
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

  submit(): void {
    if (this.adhkarForm().invalid() || this.state.saving()) {
      return;
    }

    this.state.create(this.model());
  }
}

