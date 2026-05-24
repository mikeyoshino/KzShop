import { Routes } from '@angular/router';
import { DashboardPageComponent } from './features/dashboard/dashboard.page';
import { ProductsPageComponent } from './features/products/products.page';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: 'dashboard', component: DashboardPageComponent },
  { path: 'products', component: ProductsPageComponent }
];
