import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormField, form, maxLength, min, required } from '@angular/forms/signals';
import { I18n } from '@zad/i18n';
import { CreateProductRequest } from './product.model';
import { ProductState } from './product-state';

@Component({
  selector: 'zad-product-create',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormField],
  templateUrl: './product-create.html',
  styleUrl: './product-create.scss'
})
export class ProductCreate {
  readonly i18n = inject(I18n);
  protected readonly state = inject(ProductState);

  protected readonly model = signal<CreateProductRequest>({
    sku: '',
    name: '',
    description: '',
    unitPrice: 100,
    stockQuantity: 10
  });

  protected readonly productForm = form(this.model, (path) => {
    required(path.sku);
    maxLength(path.sku, 50);
    required(path.name);
    maxLength(path.name, 200);
    min(path.unitPrice, 0.01);
    min(path.stockQuantity, 0);
  });

  submit(): void {
    if (this.productForm().invalid() || this.state.saving()) {
      return;
    }

    this.state.createProduct(this.model());
  }
}

