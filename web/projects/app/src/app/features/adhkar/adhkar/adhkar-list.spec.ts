import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { computed, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { I18n, Lang } from '@zad/i18n';
import { beforeEach, describe, expect, it } from 'vitest';
import { API_ROUTES } from '../../../api/api-base';
import { AdhkarList } from './adhkar-list';
import { AdhkarState } from './adhkar-state';

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

describe('AdhkarList', () => {
  let component: AdhkarList;
  let fixture: ComponentFixture<AdhkarList>;
  let state: AdhkarState;
  let httpTesting: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdhkarList],
      providers: [
        AdhkarState,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: I18n, useFactory: createTestI18n }
      ]
    }).compileComponents();

    httpTesting = TestBed.inject(HttpTestingController);
    state = TestBed.inject(AdhkarState);
    fixture = TestBed.createComponent(AdhkarList);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load adhkar and render create and update drawers', async () => {
    fixture.detectChanges();

    const listReq = httpTesting.expectOne(`${API_ROUTES.adhkar}?includeInactive=true`);
    listReq.flush([
      {
        id: 'dhikr-1',
        text: 'سبحان الله',
        translation: 'Glory be to Allah',
        reference: 'Sahih Muslim',
        category: 'General',
        isActive: true,
        displayOrder: 1
      }
    ]);
    await fixture.whenStable();

    const host = fixture.nativeElement as HTMLElement;
    expect(host.textContent).toContain('سبحان الله');

    state.openCreate();
    fixture.detectChanges();
    const createModal = host.querySelector('zad-adhkar-create');
    expect(createModal).toBeTruthy();

    const textarea = createModal!.querySelector('textarea')!;
    textarea.value = 'الحمد لله';
    textarea.dispatchEvent(new Event('input'));
    const inputs = createModal!.querySelectorAll('input');
    inputs[0].value = 'All praise is due to Allah';
    inputs[0].dispatchEvent(new Event('input'));
    inputs[1].value = 'General';
    inputs[1].dispatchEvent(new Event('input'));
    inputs[2].value = '2';
    inputs[2].dispatchEvent(new Event('input'));
    inputs[3].checked = false;
    inputs[3].dispatchEvent(new Event('input'));
    const select = createModal!.querySelector('select')!;
    select.value = 'tahmid';
    select.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    createModal!.querySelector('form')!.dispatchEvent(new Event('submit'));
    const createReq = httpTesting.expectOne(
      (req) => req.method === 'POST' && req.url === API_ROUTES.adhkar
    );
    expect(createReq.request.body).toEqual({
      text: 'الحمد لله',
      translation: 'All praise is due to Allah',
      reference: 'General',
      category: 'tahmid',
      displayOrder: 2,
      isActive: false
    });
    createReq.flush({ id: 'dhikr-2', ...createReq.request.body });
    const reloadReq = httpTesting.expectOne(`${API_ROUTES.adhkar}?includeInactive=true`);
    reloadReq.flush(state.items());

    state.openUpdate(state.items()[0]);
    fixture.detectChanges();
    expect(host.querySelector('zad-adhkar-update')).toBeTruthy();

    httpTesting.verify();
  });
});
