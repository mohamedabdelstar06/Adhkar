import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { I18n } from '@zad/i18n';
import { API_ROUTES } from '../../../api/api-base';
import {
  PlatformTenant,
  ProvisionTenantRequest,
  UpdatePlatformTenantRequest
} from './tenant.model';

@Injectable()
export class TenantState {
  private readonly http = inject(HttpClient);
  private readonly i18n = inject(I18n);
  private readonly baseUrl = API_ROUTES.platformTenants;

  readonly tenants = signal<PlatformTenant[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly updating = signal(false);
  readonly deleting = signal(false);
  readonly toggling = signal(false);

  readonly createOpen = signal(false);
  readonly editingTenant = signal<PlatformTenant | null>(null);
  readonly viewingTenant = signal<PlatformTenant | null>(null);
  readonly deletingTenant = signal<PlatformTenant | null>(null);
  readonly togglingTenant = signal<{ tenant: PlatformTenant; targetActive: boolean } | null>(null);

  readonly error = signal<string | null>(null);
  readonly notice = signal<string | null>(null);

  load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.http.get<PlatformTenant[]>(this.baseUrl).subscribe({
      next: (tenants) => {
        this.tenants.set(tenants);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(this.describe(err));
        this.loading.set(false);
      }
    });
  }

  isTenantActive(tenant: PlatformTenant): boolean {
    return tenant.status === 'Active';
  }

  buildTenantUrl(host: string): string {
    if (typeof window === 'undefined') {
      return `https://${host}`;
    }

    const protocol = window.location.protocol || 'https:';
    const port = window.location.port ? `:${window.location.port}` : '';
    return `${protocol}//${host}${port}`;
  }

  openCreate(): void {
    this.editingTenant.set(null);
    this.viewingTenant.set(null);
    this.createOpen.set(true);
    this.error.set(null);
    this.notice.set(null);
  }

  closeCreate(): void {
    this.createOpen.set(false);
  }

  create(request: ProvisionTenantRequest): void {
    this.saving.set(true);
    this.error.set(null);
    this.notice.set(null);

    this.http
      .post<{ tenantId: string }>(this.baseUrl, {
        slug: request.slug.trim().toLowerCase(),
        host: request.host.trim().toLowerCase(),
        defaultLanguage: request.defaultLanguage.trim() || 'ar',
        timeZone: request.timeZone.trim() || 'UTC'
      })
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.createOpen.set(false);
          this.notice.set(this.i18n.t('platform.created'));
          this.load();
        },
        error: (err) => {
          this.saving.set(false);
          this.error.set(this.describe(err));
        }
      });
  }

  openUpdate(tenant: PlatformTenant): void {
    this.createOpen.set(false);
    this.viewingTenant.set(null);
    this.editingTenant.set(tenant);
    this.error.set(null);
    this.notice.set(null);
  }

  closeUpdate(): void {
    this.editingTenant.set(null);
  }

  update(tenantId: string, request: UpdatePlatformTenantRequest): void {
    this.updating.set(true);
    this.error.set(null);
    this.notice.set(null);

    this.http
      .put<void>(`${this.baseUrl}/${tenantId}`, {
        host: request.host.trim().toLowerCase(),
        defaultLanguage: request.defaultLanguage.trim() || 'ar',
        timeZone: request.timeZone.trim() || 'UTC'
      })
      .subscribe({
        next: () => {
          this.updating.set(false);
          this.editingTenant.set(null);
          this.notice.set(this.i18n.t('platform.updated'));
          this.load();
        },
        error: (err) => {
          this.updating.set(false);
          this.error.set(this.describe(err));
        }
      });
  }

  openDetails(tenant: PlatformTenant): void {
    this.viewingTenant.set(tenant);
  }

  closeDetails(): void {
    this.viewingTenant.set(null);
  }

  promptDelete(tenant: PlatformTenant): void {
    this.deletingTenant.set(tenant);
    this.error.set(null);
    this.notice.set(null);
  }

  cancelDelete(): void {
    this.deletingTenant.set(null);
  }

  submitDelete(): void {
    const tenant = this.deletingTenant();
    if (!tenant) {
      return;
    }

    this.deleting.set(true);
    this.error.set(null);
    this.notice.set(null);

    this.http.delete<void>(`${this.baseUrl}/${tenant.tenantId}`).subscribe({
      next: () => {
        this.deleting.set(false);
        this.deletingTenant.set(null);
        this.notice.set(this.i18n.t('platform.deleted'));
        this.load();
      },
      error: (err) => {
        this.deleting.set(false);
        this.error.set(this.describe(err));
      }
    });
  }

  promptToggle(tenant: PlatformTenant): void {
    const currentlyActive = this.isTenantActive(tenant);
    this.togglingTenant.set({
      tenant,
      targetActive: !currentlyActive
    });
    this.error.set(null);
    this.notice.set(null);
  }

  cancelToggle(): void {
    this.togglingTenant.set(null);
  }

  submitToggle(): void {
    const target = this.togglingTenant();
    if (!target) {
      return;
    }

    this.toggling.set(true);
    this.error.set(null);
    this.notice.set(null);

    const url = target.targetActive
      ? `${this.baseUrl}/${target.tenant.tenantId}/activate`
      : `${this.baseUrl}/${target.tenant.tenantId}/suspend`;

    this.http.post<void>(url, {}).subscribe({
      next: () => {
        this.toggling.set(false);
        this.togglingTenant.set(null);
        this.notice.set(
          target.targetActive
            ? this.i18n.t('platform.activated')
            : this.i18n.t('platform.deactivated')
        );
        this.load();
      },
      error: (err) => {
        this.toggling.set(false);
        this.error.set(this.describe(err));
      }
    });
  }

  private describe(error: unknown): string {
    const problem = error as {
      error?: {
        title?: string;
        code?: string;
        errors?: Record<string, string[]>;
      };
    };

    const validation = problem?.error?.errors
      ? Object.values(problem.error.errors).flat().join(' ')
      : undefined;

    return validation ?? problem?.error?.title ?? problem?.error?.code ?? 'Request failed';
  }
}

