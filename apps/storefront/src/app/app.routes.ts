import { Routes } from '@angular/router';
import { CollectionPageComponent } from './features/collection/collection.page';
import { HomePageComponent } from './features/home/home.page';
import { ProductPageComponent } from './features/product/product.page';

export const routes: Routes = [
  { path: '', component: HomePageComponent },
  { path: 'collections/:slug', component: CollectionPageComponent },
  { path: 'products/:slug', component: ProductPageComponent }
];
