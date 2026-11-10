import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { I18n } from '@zad/i18n';
import { ZadButton, ZadPageHeader } from '@zad/ui';
import { TenantCreate } from './tenant-create';
import { TenantDetails } from './tenant-details';
import { TenantState } from './tenant-state';
import { TenantUpdate } from './tenant-update';

@Component({
  selector: 'zad-tenant-list',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [TenantState],
  imports: [ZadPageHeader, ZadButton, TenantCreate, TenantUpdate, TenantDetails],
  templateUrl: './tenant-list.html',
  styleUrl: './tenant-list.scss'
})
export class TenantList implements OnInit {
  readonly i18n = inject(I18n);
  protected readonly state = inject(TenantState);

  ngOnInit(): void {
    this.state.load();
  }
}

