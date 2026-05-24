import { Routes } from '@angular/router';
import { CategoriesPageComponent } from './features/categories/categories.page';
import { DashboardPageComponent } from './features/dashboard/dashboard.page';
import { ProductFormPageComponent } from './features/product-form/product-form.page';
import { ProductsPageComponent } from './features/products/products.page';
import { StudiosPageComponent } from './features/studios/studios.page';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: 'dashboard', component: DashboardPageComponent },
  { path: 'products', component: ProductsPageComponent },
  { path: 'products/new', component: ProductFormPageComponent },
  { path: 'products/:id', component: ProductFormPageComponent },
  { path: 'studios', component: StudiosPageComponent },
  { path: 'categories', component: CategoriesPageComponent },
];
