import { ChangeDetectionStrategy, Component, effect, inject, input, signal } from '@angular/core';
import { FormField, form, maxLength, min, required } from '@angular/forms/signals';
import { I18n } from '@zad/i18n';
import { ProductDto, UpdateProductRequest } from './product.model';
import { ProductState } from './product-state';

@Component({
  selector: 'zad-product-update',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormField],
  templateUrl: './product-update.html',
  styleUrl: './product-update.scss'
})
export class ProductUpdate {
  readonly i18n = inject(I18n);
  protected readonly state = inject(ProductState);

  readonly product = input.required<ProductDto>();

  protected readonly model = signal<UpdateProductRequest>({
    name: '',
    description: '',
    unitPrice: 0
  });

  protected readonly productForm = form(this.model, (path) => {
    required(path.name);
    maxLength(path.name, 200);
    min(path.unitPrice, 0.01);
  });

  constructor() {
    effect(() => {
      const current = this.product();
      this.model.set({
        name: current.name,
        description: current.description,
        unitPrice: current.unitPrice
      });
    });
  }

  submit(): void {
    if (this.productForm().invalid() || this.state.saving()) {
      return;
    }

    this.state.updateProduct(this.product().id, this.model());
  }
}

