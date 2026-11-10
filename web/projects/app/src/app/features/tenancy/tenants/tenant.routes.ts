import { Routes } from '@angular/router';
import { TenantState } from './tenant-state';

export const TENANT_ROUTES: Routes = [
  {
    path: '',
    providers: [TenantState],
    loadComponent: () => import('./tenant-list').then((module) => module.TenantList)
  }
];

