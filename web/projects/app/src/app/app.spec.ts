import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { computed, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { I18n, Lang } from '@zad/i18n';
import { beforeEach, describe, expect, it } from 'vitest';
import { API_ROUTES } from './api/api-base';
import { App } from './app';
import { routes } from './app.routes';
import { detectPlatformHost } from './platform-host';

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

describe('App', () => {
  let httpTesting: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: I18n, useFactory: createTestI18n }
      ]
    }).compileComponents();

    httpTesting = TestBed.inject(HttpTestingController);
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should render the adhkar toast outside the header', async () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();

    const toastReq = httpTesting.expectOne(`${API_ROUTES.adhkar}/current`);
    toastReq.flush({
      id: 'dhikr-1',
      text: 'سبحان الله',
      translation: 'Glory be to Allah',
      reference: 'Sahih Muslim',
      category: 'tasbih',
      displayOrder: 1,
      isActive: true
    });
    await fixture.whenStable();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.app-header zad-adhkar-toast')).toBeNull();
    expect(compiled.querySelector('zad-adhkar-toast')).toBeTruthy();
    expect(compiled.textContent).toContain('سبحان الله');
    expect(compiled.querySelector('a[href="/tenants"]')).toBeNull();
    httpTesting.verify();
  });

  it('should detect platform host strictly for admin subdomains and guard tenants route', () => {
    expect(detectPlatformHost('admin.folll.duckdns.org')).toBe(true);
    expect(detectPlatformHost('admin.localhost')).toBe(true);
    expect(detectPlatformHost('epsilon.folll.duckdns.org')).toBe(false);
    expect(detectPlatformHost('demo.localhost')).toBe(false);
    expect(detectPlatformHost('localhost')).toBe(false);

    const tenantsRoute = routes.find((r) => r.path === 'tenants');
    expect(tenantsRoute?.canMatch?.length).toBe(1);
    const guard = tenantsRoute!.canMatch![0] as () => boolean;
    expect(guard()).toBe(false);

    const defaultRoute = routes.find((r) => r.path === '');
    const wildcardRoute = routes.find((r) => r.path === '**');
    expect(defaultRoute?.redirectTo).toBe('orders');
    expect(wildcardRoute?.redirectTo).toBe('orders');
  });
});
