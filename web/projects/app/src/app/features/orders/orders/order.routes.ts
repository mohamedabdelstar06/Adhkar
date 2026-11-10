import { Routes } from '@angular/router';
import { OrderState } from './order-state';

export const ORDER_ROUTES: Routes = [
  {
    path: '',
    providers: [OrderState],
    loadComponent: () => import('./order-list').then((module) => module.OrderList)
  }
];

