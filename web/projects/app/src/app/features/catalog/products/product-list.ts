import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { I18n } from '@zad/i18n';
import { ProductCreate } from './product-create';
import { ProductState } from './product-state';
import { ProductUpdate } from './product-update';

@Component({
  selector: 'zad-product-list',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DecimalPipe, ProductCreate, ProductUpdate],
  templateUrl: './product-list.html',
  styleUrl: './product-list.scss'
})
export class ProductList implements OnInit {
  readonly i18n = inject(I18n);
  protected readonly state = inject(ProductState);

  ngOnInit(): void {
    this.state.loadProducts();
  }

  onSearchInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.state.searchQuery.set(value);
  }

  deactivate(id: string): void {
    if (confirm(this.i18n.t('catalog.deactivateConfirm'))) {
      this.state.deactivateProduct(id);
    }
  }
}

