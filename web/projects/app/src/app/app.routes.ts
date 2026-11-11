import { Routes } from '@angular/router';
import { detectPlatformHost } from './platform-host';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'orders' },
  {
    path: 'adhkar',
    loadChildren: () => import('./features/adhkar/adhkar').then((module) => module.ADHKAR_ROUTES)
  },
  {
    path: 'tenants',
    canMatch: [() => detectPlatformHost()],
    loadChildren: () => import('./features/tenancy/tenants').then((module) => module.TENANT_ROUTES)
  },
  {
    path: 'catalog',
    loadChildren: () => import('./features/catalog/products').then((module) => module.PRODUCT_ROUTES)
  },
  {
    path: 'orders',
    loadChildren: () => import('./features/orders/orders').then((module) => module.ORDER_ROUTES)
  },
  { path: '**', redirectTo: 'orders' }
];
