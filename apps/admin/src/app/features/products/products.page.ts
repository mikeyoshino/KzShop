import { CurrencyPipe, NgClass } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { of } from 'rxjs';
import { catchError, distinctUntilChanged, map, switchMap } from 'rxjs/operators';
import { ProductCard } from '../../core/models/catalog.models';
import { CatalogApiService } from '../../core/services/catalog-api.service';

@Component({
  selector: 'app-products-page',
  standalone: true,
  imports: [CurrencyPipe, FormsModule, NgClass, RouterLink],
  templateUrl: './products.page.html',
  styleUrl: './products.page.css',
})
export class ProductsPageComponent {
  private readonly catalogApi = inject(CatalogApiService);

  protected readonly searchTerm = signal('');
  protected readonly selectedStatus = signal('all');
  protected readonly selectedCategory = signal('all');
  protected readonly selectedSlug = signal('');

  protected readonly products = toSignal(
    this.catalogApi.getAdminProducts().pipe(map((response) => response.items)),
    { initialValue: [] as ProductCard[] },
  );

  protected readonly filteredProducts = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const status = this.selectedStatus();
    const category = this.selectedCategory();

    return this.products().filter((product) => {
      const matchesTerm =
        term.length === 0 ||
        product.name.toLowerCase().includes(term) ||
        product.studioName.toLowerCase().includes(term) ||
        product.slug.toLowerCase().includes(term);
      const matchesStatus = status === 'all' || product.status === status;
      const matchesCategory = category === 'all' || product.categorySlug === category;

      return matchesTerm && matchesStatus && matchesCategory;
    });
  });

  protected readonly activeSlug = computed(() => {
    const selected = this.selectedSlug();
    const products = this.filteredProducts();

    if (selected && products.some((product) => product.slug === selected)) {
      return selected;
    }

    return products[0]?.slug ?? '';
  });

  protected readonly selectedProduct = toSignal(
    toObservable(this.activeSlug).pipe(
      distinctUntilChanged(),
      switchMap((slug) => {
        const product = this.products().find((x) => x.slug === slug);
        return product ? this.catalogApi.getAdminProductById(product.id) : of(null);
      }),
      catchError(() => of(null)),
    ),
    { initialValue: null },
  );

  protected readonly statusOptions = computed(() =>
    [...new Set(this.products().map((product) => product.status))].sort(),
  );

  protected readonly categoryOptions = computed(() =>
    [...new Set(this.products().map((product) => product.categorySlug))].sort(),
  );

  protected readonly waitlistCount = computed(
    () => this.filteredProducts().filter((product) => product.status === 'Waitlist').length,
  );

  protected readonly newCount = computed(
    () => this.filteredProducts().filter((product) => product.isNew).length,
  );

  protected statusLabel(status: string): string {
    switch (status) {
      case 'PreOrder':
        return 'Pre-order';
      case 'Waitlist':
        return 'Waitlist';
      default:
        return 'Active';
    }
  }

  protected toneFor(status: string): string {
    switch (status) {
      case 'PreOrder':
        return 'preorder';
      case 'Waitlist':
        return 'waitlist';
      default:
        return 'active';
    }
  }
}
