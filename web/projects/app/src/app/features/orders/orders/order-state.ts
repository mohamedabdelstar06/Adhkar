import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { API_ROUTES } from '../../../api/api-base';
import {
  CreateOrderRequest,
  OrderDetailsDto,
  OrderListItemDto,
  OrderPagedList,
  OrderProductOption
} from './order.model';

@Injectable()
export class OrderState {
  private readonly http = inject(HttpClient);
  private readonly ordersUrl = API_ROUTES.orders;
  private readonly catalogUrl = API_ROUTES.catalog;

  readonly orders = signal<OrderListItemDto[]>([]);
  readonly availableProducts = signal<OrderProductOption[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly createOpen = signal(false);
  readonly selectedOrder = signal<OrderDetailsDto | null>(null);
  readonly errorMessage = signal('');
  readonly searchQuery = signal('');

  readonly filteredOrders = computed(() => {
    const query = this.searchQuery().trim().toLowerCase();
    const list = this.orders();
    if (!query) {
      return list;
    }

    return list.filter(
      (order) =>
        order.orderNumber.toLowerCase().includes(query) ||
        order.customerEmail.toLowerCase().includes(query)
    );
  });

  readonly placedCount = computed(
    () => this.orders().filter((order) => order.status === 'Placed').length
  );

  readonly totalRevenue = computed(() =>
    this.orders()
      .filter((order) => order.status === 'Placed')
      .reduce((sum, order) => sum + order.totalAmount, 0)
  );

  loadOrders(page = 1, pageSize = 20): void {
    this.loading.set(true);
    this.http
      .get<OrderPagedList<OrderListItemDto>>(`${this.ordersUrl}?page=${page}&pageSize=${pageSize}`)
      .subscribe({
        next: (response) => {
          this.orders.set(response.items);
          this.loading.set(false);
        },
        error: () => this.loading.set(false)
      });
  }

  openCreate(): void {
    this.errorMessage.set('');
    this.selectedOrder.set(null);
    this.loadAvailableProducts();
    this.createOpen.set(true);
  }

  closeCreate(): void {
    this.createOpen.set(false);
    this.errorMessage.set('');
  }

  createOrder(request: CreateOrderRequest): void {
    this.saving.set(true);
    this.errorMessage.set('');
    this.http.post<OrderDetailsDto>(this.ordersUrl, request).subscribe({
      next: () => {
        this.saving.set(false);
        this.closeCreate();
        this.loadOrders();
      },
      error: (err: { error?: { title?: string } }) => {
        this.saving.set(false);
        this.errorMessage.set(err.error?.title || 'Error placing order');
      }
    });
  }

  openDetails(id: string): void {
    this.http.get<OrderDetailsDto>(`${this.ordersUrl}/${id}`).subscribe({
      next: (details) => this.selectedOrder.set(details)
    });
  }

  closeDetails(): void {
    this.selectedOrder.set(null);
  }

  cancelOrder(id: string): void {
    this.http.post<void>(`${this.ordersUrl}/${id}/cancel`, {}).subscribe({
      next: () => this.loadOrders()
    });
  }

  private loadAvailableProducts(): void {
    this.http
      .get<OrderPagedList<OrderProductOption>>(`${this.catalogUrl}/products?page=1&pageSize=100`)
      .subscribe({
        next: (response) =>
          this.availableProducts.set(
            response.items.filter((item) => item.isActive && item.stockQuantity > 0)
          )
      });
  }
}

