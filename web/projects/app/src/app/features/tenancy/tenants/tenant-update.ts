import { ChangeDetectionStrategy, Component, effect, inject, input, signal } from '@angular/core';
import { FormField, form, maxLength, required } from '@angular/forms/signals';
import { I18n } from '@zad/i18n';
import {
  COMMON_TIME_ZONES,
  PLATFORM_LANGUAGES,
  PlatformTenant,
  UpdatePlatformTenantRequest
} from './tenant.model';
import { TenantState } from './tenant-state';

@Component({
  selector: 'zad-tenant-update',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormField],
  templateUrl: './tenant-update.html',
  styleUrl: './tenant-update.scss'
})
export class TenantUpdate {
  readonly i18n = inject(I18n);
  protected readonly state = inject(TenantState);
  protected readonly timeZones = COMMON_TIME_ZONES;
  protected readonly languages = PLATFORM_LANGUAGES;

  readonly tenant = input.required<PlatformTenant>();

  protected readonly model = signal<UpdatePlatformTenantRequest>({
    host: '',
    defaultLanguage: 'ar',
    timeZone: 'Africa/Cairo'
  });

  protected readonly tenantForm = form(this.model, (path) => {
    required(path.host);
    maxLength(path.host, 255);
  });

  constructor() {
    effect(() => {
      const current = this.tenant();
      this.model.set({
        host: current.host,
        defaultLanguage: current.defaultLanguage || 'ar',
        timeZone: current.timeZone || 'UTC'
      });
    });
  }

  submit(): void {
    if (this.tenantForm().invalid() || this.state.updating()) {
      return;
    }

    this.state.update(this.tenant().tenantId, this.model());
  }
}

