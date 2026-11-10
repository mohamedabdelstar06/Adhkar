import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { I18n } from '@zad/i18n';
import { PlatformTenant } from './tenant.model';
import { TenantState } from './tenant-state';

@Component({
  selector: 'zad-tenant-details',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './tenant-details.html',
  styleUrl: './tenant-details.scss'
})
export class TenantDetails {
  readonly i18n = inject(I18n);
  protected readonly state = inject(TenantState);

  readonly tenant = input.required<PlatformTenant>();
}

