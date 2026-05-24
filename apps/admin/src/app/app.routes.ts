import { Routes } from '@angular/router';
import { adminAuthGuard } from './core/guards/admin-auth.guard';
import { LoginPageComponent } from './features/auth/login.page';
import { RegisterPageComponent } from './features/auth/register.page';
import { CategoriesPageComponent } from './features/categories/categories.page';
import { DashboardPageComponent } from './features/dashboard/dashboard.page';
import { ProductFormPageComponent } from './features/product-form/product-form.page';
import { ProductsPageComponent } from './features/products/products.page';
import { StudiosPageComponent } from './features/studios/studios.page';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: 'login', component: LoginPageComponent },
  { path: 'register', component: RegisterPageComponent },
  { path: 'dashboard', component: DashboardPageComponent, canActivate: [adminAuthGuard] },
  { path: 'products', component: ProductsPageComponent, canActivate: [adminAuthGuard] },
  { path: 'products/new', component: ProductFormPageComponent, canActivate: [adminAuthGuard] },
  { path: 'products/:id', component: ProductFormPageComponent, canActivate: [adminAuthGuard] },
  { path: 'studios', component: StudiosPageComponent, canActivate: [adminAuthGuard] },
  { path: 'categories', component: CategoriesPageComponent, canActivate: [adminAuthGuard] },
];
