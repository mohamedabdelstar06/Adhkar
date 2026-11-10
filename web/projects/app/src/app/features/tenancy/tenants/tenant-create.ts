import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormField, form, maxLength, required } from '@angular/forms/signals';
import { I18n } from '@zad/i18n';
import {
  COMMON_TIME_ZONES,
  PLATFORM_LANGUAGES,
  ProvisionTenantRequest
} from './tenant.model';
import { TenantState } from './tenant-state';

@Component({
  selector: 'zad-tenant-create',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormField],
  templateUrl: './tenant-create.html',
  styleUrl: './tenant-create.scss'
})
export class TenantCreate {
  readonly i18n = inject(I18n);
  protected readonly state = inject(TenantState);
  protected readonly timeZones = COMMON_TIME_ZONES;
  protected readonly languages = PLATFORM_LANGUAGES;

  protected readonly model = signal<ProvisionTenantRequest>({
    slug: '',
    host: '',
    defaultLanguage: 'ar',
    timeZone: 'Africa/Cairo'
  });

  protected readonly tenantForm = form(this.model, (path) => {
    required(path.slug);
    maxLength(path.slug, 63);
    required(path.host);
    maxLength(path.host, 255);
  });

  onSlugInput(event: Event): void {
    const raw = (event.target as HTMLInputElement).value;
    const slug = raw.trim().toLowerCase();
    const currentRoot = this.resolveCurrentRootDomain();
    const host = slug ? `${slug}.${currentRoot}` : '';

    this.model.update((current) => ({
      ...current,
      slug,
      host
    }));
  }

  submit(): void {
    if (this.tenantForm().invalid() || this.state.saving()) {
      return;
    }

    this.state.create(this.model());
  }

  private resolveCurrentRootDomain(): string {
    if (typeof window === 'undefined') {
      return 'localhost';
    }

    const currentHost = window.location.hostname.toLowerCase();
    if (currentHost.startsWith('admin.')) {
      return currentHost.substring('admin.'.length);
    }

    return currentHost || 'localhost';
  }
}

