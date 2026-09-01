import { Routes } from '@angular/router';
import { Home } from './features/home/home';
import { Login } from './features/auth/login/login';
import { Dashboard } from './features/admin/dashboard/dashboard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'home',
    pathMatch: 'full'
  },
  {
    path: 'home',
    component: Home
  },
  {
    path: 'login',
    component: Login
  },
  {
    path: 'admin/dashboard',
    component: Dashboard
  },
  {
    path: '**',
    redirectTo: 'home'
  }
];