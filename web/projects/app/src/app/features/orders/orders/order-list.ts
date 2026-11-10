import { DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { I18n } from '@zad/i18n';
import { OrderCreate } from './order-create';
import { OrderDetails } from './order-details';
import { OrderState } from './order-state';

@Component({
  selector: 'zad-order-list',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, DecimalPipe, OrderCreate, OrderDetails],
  templateUrl: './order-list.html',
  styleUrl: './order-list.scss'
})
export class OrderList implements OnInit {
  readonly i18n = inject(I18n);
  protected readonly state = inject(OrderState);

  ngOnInit(): void {
    this.state.loadOrders();
  }

  onSearchInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.state.searchQuery.set(value);
  }

  cancelOrder(id: string): void {
    if (confirm(this.i18n.t('orders.cancelConfirm'))) {
      this.state.cancelOrder(id);
    }
  }
}

