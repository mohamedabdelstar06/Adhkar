import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { computed, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { I18n, Lang } from '@zad/i18n';
import { beforeEach, describe, expect, it } from 'vitest';
import { API_ROUTES } from '../../../api/api-base';
import { TenantList } from './tenant-list';
import { TenantState } from './tenant-state';

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

describe('TenantList', () => {
  let component: TenantList;
  let fixture: ComponentFixture<TenantList>;
  let state: TenantState;
  let httpTesting: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TenantList],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: I18n, useFactory: createTestI18n }
      ]
    }).compileComponents();

    httpTesting = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(TenantList);
    component = fixture.componentInstance;
    state = fixture.debugElement.injector.get(TenantState);
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load tenants and render create, update and details modals', async () => {
    fixture.detectChanges();

    const listReq = httpTesting.expectOne(API_ROUTES.platformTenants);
    listReq.flush([
      {
        tenantId: 'ten-1',
        slug: 'demo',
        host: 'demo.localhost',
        defaultLanguage: 'ar',
        timeZone: 'Africa/Cairo',
        status: 'Active',
        createdAtUtc: '2026-11-10T10:00:00Z'
      }
    ]);
    await fixture.whenStable();

    const host = fixture.nativeElement as HTMLElement;
    expect(host.textContent).toContain('demo');

    state.openCreate();
    fixture.detectChanges();
    const createModal = host.querySelector('zad-tenant-create');
    expect(createModal).toBeTruthy();

    const slugInput = createModal!.querySelector('input[placeholder="acme"]') as HTMLInputElement;
    slugInput.value = 'Acme';
    slugInput.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    const hostInput = createModal!.querySelectorAll('input')[1];
    expect(hostInput.value).toBe('acme.localhost');

    createModal!.querySelector('form')!.dispatchEvent(new Event('submit'));
    const createReq = httpTesting.expectOne(
      (req) => req.method === 'POST' && req.url === API_ROUTES.platformTenants
    );
    expect(createReq.request.body).toEqual({
      slug: 'acme',
      host: 'acme.localhost',
      defaultLanguage: 'ar',
      timeZone: 'Africa/Cairo'
    });
    createReq.flush({ tenantId: 'ten-2' });
    const reloadReq = httpTesting.expectOne(API_ROUTES.platformTenants);
    reloadReq.flush(state.tenants());

    const tenant = state.tenants()[0];
    state.openUpdate(tenant);
    fixture.detectChanges();
    expect(host.querySelector('zad-tenant-update')).toBeTruthy();

    state.openDetails(tenant);
    fixture.detectChanges();
    expect(host.querySelector('zad-tenant-details')).toBeTruthy();

    httpTesting.verify();
  });
});
