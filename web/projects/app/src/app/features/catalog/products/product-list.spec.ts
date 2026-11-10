import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { computed, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { I18n, Lang } from '@zad/i18n';
import { beforeEach, describe, expect, it } from 'vitest';
import { API_ROUTES } from '../../../api/api-base';
import { ProductList } from './product-list';
import { ProductState } from './product-state';

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

describe('ProductList', () => {
  let component: ProductList;
  let fixture: ComponentFixture<ProductList>;
  let state: ProductState;
  let httpTesting: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductList],
      providers: [
        ProductState,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: I18n, useFactory: createTestI18n }
      ]
    }).compileComponents();

    httpTesting = TestBed.inject(HttpTestingController);
    state = TestBed.inject(ProductState);
    fixture = TestBed.createComponent(ProductList);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load products and render create and update forms', async () => {
    fixture.detectChanges();

    const listReq = httpTesting.expectOne(`${API_ROUTES.catalog}/products?page=1&pageSize=20`);
    listReq.flush({
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
      pageSize: 20
    });
    await fixture.whenStable();

    const host = fixture.nativeElement as HTMLElement;
    expect(host.textContent).toContain('ZAD-001');
    expect(host.textContent).toContain('Sample Dates');

    state.openCreate();
    fixture.detectChanges();
    const createModal = host.querySelector('zad-product-create');
    expect(createModal).toBeTruthy();

    const inputs = createModal!.querySelectorAll('input');
    inputs[0].value = 'SKU-99';
    inputs[0].dispatchEvent(new Event('input'));
    inputs[1].value = 'New Item';
    inputs[1].dispatchEvent(new Event('input'));
    inputs[2].value = '49.5';
    inputs[2].dispatchEvent(new Event('input'));
    inputs[3].value = '12';
    inputs[3].dispatchEvent(new Event('input'));
    fixture.detectChanges();

    createModal!.querySelector('form')!.dispatchEvent(new Event('submit'));
    const createReq = httpTesting.expectOne(
      (req) => req.method === 'POST' && req.url === `${API_ROUTES.catalog}/products`
    );
    expect(createReq.request.body).toEqual({
      sku: 'SKU-99',
      name: 'New Item',
      description: '',
      unitPrice: 49.5,
      stockQuantity: 12
    });
    createReq.flush({ ...state.products()[0], id: 'prod-2', sku: 'SKU-99', name: 'New Item' });
    const reloadReq = httpTesting.expectOne(`${API_ROUTES.catalog}/products?page=1&pageSize=20`);
    reloadReq.flush({ items: state.products(), totalCount: 1, page: 1, pageSize: 20 });

    const product = state.products()[0];
    state.openUpdate(product);
    const detailsReq = httpTesting.expectOne(`${API_ROUTES.catalog}/products/prod-1`);
    detailsReq.flush({ ...product, description: 'Fresh dates' });
    fixture.detectChanges();
    expect(host.querySelector('zad-product-update')).toBeTruthy();

    httpTesting.verify();
  });
});
