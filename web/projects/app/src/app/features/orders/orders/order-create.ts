import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormField, email, form, required } from '@angular/forms/signals';
import { I18n } from '@zad/i18n';
import { CartLineItem, CreateOrderRequest } from './order.model';
import { OrderState } from './order-state';

@Component({
  selector: 'zad-order-create',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DecimalPipe, FormField],
  templateUrl: './order-create.html',
  styleUrl: './order-create.scss'
})
export class OrderCreate {
  readonly i18n = inject(I18n);
  protected readonly state = inject(OrderState);

  protected readonly customerModel = signal({ customerEmail: '' });
  protected readonly emailForm = form(this.customerModel, (path) => {
    required(path.customerEmail);
    email(path.customerEmail);
  });

  protected readonly selectedProductId = signal('');
  protected readonly itemQuantity = signal(1);
  protected readonly cartItems = signal<CartLineItem[]>([]);

  protected readonly cartTotal = computed(() =>
    this.cartItems().reduce((sum, item) => sum + item.product.unitPrice * item.quantity, 0)
  );

  onProductChange(event: Event): void {
    const id = (event.target as HTMLSelectElement).value;
    this.selectedProductId.set(id);
    this.itemQuantity.set(1);
  }

  onQuantityChange(event: Event): void {
    const raw = Number((event.target as HTMLInputElement).value);
    this.itemQuantity.set(Number.isFinite(raw) ? raw : 1);
  }

  addLineItem(): void {
    const prod = this.state
      .availableProducts()
      .find((item) => item.id === this.selectedProductId());
    if (!prod || this.itemQuantity() <= 0) {
      return;
    }

    const quantityToAdd = this.itemQuantity();
    this.cartItems.update((current) => {
      const existingIndex = current.findIndex((item) => item.product.id === prod.id);
      if (existingIndex === -1) {
        return [...current, { product: prod, quantity: quantityToAdd }];
      }

      return current.map((item, idx) =>
        idx === existingIndex ? { ...item, quantity: item.quantity + quantityToAdd } : item
      );
    });

    this.selectedProductId.set('');
    this.itemQuantity.set(1);
  }

  removeLineItem(index: number): void {
    this.cartItems.update((current) => current.filter((_, idx) => idx !== index));
  }

  submit(): void {
    const customerEmail = this.customerModel().customerEmail.trim();
    if (!customerEmail || this.emailForm().invalid()) {
      this.state.errorMessage.set('يرجى كتابة البريد الإلكتروني للعميل / Customer email is required');
      return;
    }

    if (this.cartItems().length === 0) {
      this.state.errorMessage.set('يرجى إضافة صنف واحد على الأقل / Order must contain at least one item');
      return;
    }

    const request: CreateOrderRequest = {
      customerEmail,
      items: this.cartItems().map((item) => ({
        productId: item.product.id,
        quantity: item.quantity
      }))
    };

    this.state.createOrder(request);
  }
}

