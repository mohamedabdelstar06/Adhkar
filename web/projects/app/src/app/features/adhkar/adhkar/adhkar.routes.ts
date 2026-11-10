import { Routes } from '@angular/router';
import { AdhkarState } from './adhkar-state';

export const ADHKAR_ROUTES: Routes = [
  {
    path: '',
    providers: [AdhkarState],
    loadComponent: () => import('./adhkar-list').then((module) => module.AdhkarList)
  }
];

