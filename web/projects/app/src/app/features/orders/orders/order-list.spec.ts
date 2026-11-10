import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { computed, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { I18n, Lang } from '@zad/i18n';
import { beforeEach, describe, expect, it } from 'vitest';
import { API_ROUTES } from '../../../api/api-base';
import { OrderList } from './order-list';
import { OrderState } from './order-state';

function createTestI18n(): I18n {
  const lang = signal<Lang>('ar');
  return {
    lang: lang.asReadonly(),
    dir: computed(() => (lang() === 'ar' ? 'rtl' : 'ltr')),
    use: (next: Lang) => lang.set(next),
    t: (key: string) => key,
    has: () => true,
    formatDate: (value: string | Date) => String(value),
    formatNumber: (value: number) => String(value)
  };
}

describe('OrderList', () => {
  let component: OrderList;
  let fixture: ComponentFixture<OrderList>;
  let state: OrderState;
  let httpTesting: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OrderList],
      providers: [
        OrderState,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: I18n, useFactory: createTestI18n }
      ]
    }).compileComponents();

    httpTesting = TestBed.inject(HttpTestingController);
    state = TestBed.inject(OrderState);
    fixture = TestBed.createComponent(OrderList);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load orders and render create and details modals', async () => {
    fixture.detectChanges();

    const listReq = httpTesting.expectOne(`${API_ROUTES.orders}?page=1&pageSize=20`);
    listReq.flush({
      items: [
        {
          id: 'ord-1',
          orderNumber: 'ORD-1001',
          customerEmail: 'buyer@example.com',
          itemCount: 2,
          totalAmount: 50,
          status: 'Placed',
          createdAtUtc: '2026-11-10T10:00:00Z'
        }
      ],
      totalCount: 1,
      page: 1,
      pageSize: 20
    });
    await fixture.whenStable();

    const host = fixture.nativeElement as HTMLElement;
    expect(host.textContent).toContain('ORD-1001');
    expect(host.textContent).toContain('buyer@example.com');

    state.openCreate();
    const productsReq = httpTesting.expectOne(`${API_ROUTES.catalog}/products?page=1&pageSize=100`);
    productsReq.flush({
      items: [
        {
          id: 'prod-1',
          sku: 'ZAD-001',
          name: 'Sample Dates',
          unitPrice: 25,
          stockQuantity: 10,
          isActive: true
        }
      ],
      totalCount: 1,
      page: 1,
      pageSize: 100
    });
    fixture.detectChanges();
    const createModal = host.querySelector('zad-order-create');
    expect(createModal).toBeTruthy();

    const emailInput = createModal!.querySelector('input[type="email"]') as HTMLInputElement;
    emailInput.value = 'new@example.com';
    emailInput.dispatchEvent(new Event('input'));
    const select = createModal!.querySelector('select')!;
    select.value = 'prod-1';
    select.dispatchEvent(new Event('change'));
    const qtyInput = createModal!.querySelector('input[type="number"]') as HTMLInputElement;
    qtyInput.value = '3';
    qtyInput.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    const addButton = createModal!.querySelector('button.btn-secondary') as HTMLButtonElement;
    addButton.click();
    fixture.detectChanges();

    createModal!.querySelector('form')!.dispatchEvent(new Event('submit'));
    const createReq = httpTesting.expectOne(
      (req) => req.method === 'POST' && req.url === API_ROUTES.orders
    );
    expect(createReq.request.body).toEqual({
      customerEmail: 'new@example.com',
      items: [{ productId: 'prod-1', quantity: 3 }]
    });
    createReq.flush({ id: 'ord-2', orderNumber: 'ORD-1002' });
    const reloadReq = httpTesting.expectOne(`${API_ROUTES.orders}?page=1&pageSize=20`);
    reloadReq.flush({ items: state.orders(), totalCount: 1, page: 1, pageSize: 20 });

    state.openDetails('ord-1');
    const detailsReq = httpTesting.expectOne(`${API_ROUTES.orders}/ord-1`);
    detailsReq.flush({
      id: 'ord-1',
      orderNumber: 'ORD-1001',
      customerEmail: 'buyer@example.com',
      status: 'Placed',
      totalAmount: 50,
      createdAtUtc: '2026-11-10T10:00:00Z',
      items: [
        {
          id: 'line-1',
          productId: 'prod-1',
          productName: 'Sample Dates',
          quantity: 2,
          unitPrice: 25,
          lineTotal: 50
        }
      ]
    });
    fixture.detectChanges();
    expect(host.querySelector('zad-order-details')).toBeTruthy();
    expect(host.textContent).toContain('Sample Dates');

    httpTesting.verify();
  });
});
