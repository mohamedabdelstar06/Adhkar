import { Routes } from '@angular/router';
import { ProductState } from './product-state';

export const PRODUCT_ROUTES: Routes = [
  {
    path: '',
    providers: [ProductState],
    loadComponent: () => import('./product-list').then((module) => module.ProductList)
  }
];

