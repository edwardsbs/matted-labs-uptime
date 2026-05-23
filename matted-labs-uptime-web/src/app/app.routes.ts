import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', loadComponent: () => import('./pages/dashboard/dashboard').then(m => m.DashboardComponent) },
  { path: 'history/:id', loadComponent: () => import('./pages/history/history').then(m => m.HistoryComponent) },
  { path: 'settings', loadComponent: () => import('./pages/settings/settings').then(m => m.SettingsComponent) },
  { path: '**', redirectTo: '' }
];
