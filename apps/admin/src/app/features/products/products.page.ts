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
  template: `
    <section class="page-stack">
      <article class="panel products-panel">
        <div>
          <p class="section-label">Catalog</p>
          <h2>Products</h2>
        </div>
        <p>Manage catalog products, then open full editor for specifications, inventory, and images.</p>
        <div class="toolbar toolbar--meta">
          <a class="button-primary" routerLink="/products/new">Create product</a>
        </div>
      </article>

      <section class="panel products-panel">
        <div class="toolbar">
          <input
            type="search"
            [ngModel]="searchTerm()"
            (ngModelChange)="searchTerm.set($event)"
            aria-label="Search products"
            placeholder="Search product, studio, or slug"
          />

          <select
            [ngModel]="selectedStatus()"
            (ngModelChange)="selectedStatus.set($event)"
            aria-label="Filter by status"
          >
            <option value="all">All statuses</option>
            @for (status of statusOptions(); track status) {
              <option [value]="status">{{ statusLabel(status) }}</option>
            }
          </select>

          <select
            [ngModel]="selectedCategory()"
            (ngModelChange)="selectedCategory.set($event)"
            aria-label="Filter by category"
          >
            <option value="all">All categories</option>
            @for (category of categoryOptions(); track category) {
              <option [value]="category">{{ category }}</option>
            }
          </select>
        </div>

        <div class="toolbar toolbar--meta">
          <span>{{ filteredProducts().length }} products in scope</span>
          <span>{{ waitlistCount() }} waitlist items</span>
          <span>{{ newCount() }} new drops</span>
        </div>

        <div class="table" role="table" aria-label="Catalog products">
          <div class="table-caption">
            <span>Product</span>
            <span>Status</span>
            <span>Studio</span>
            <span>Price</span>
          </div>

          @for (product of filteredProducts(); track product.id) {
            <button
              type="button"
              class="table-row table-row--button"
              [ngClass]="{ 'is-selected': activeSlug() === product.slug }"
              (click)="selectedSlug.set(product.slug)"
            >
              <div>
                <strong>{{ product.name }}</strong>
                <small>{{ product.slug }} | {{ product.scale }}</small>
              </div>
              <span class="status-pill" [ngClass]="'status-pill--' + toneFor(product.status)">
                {{ statusLabel(product.status) }}
              </span>
              <span>{{ product.studioName }}</span>
              <span>{{ product.priceAmount | currency: product.currency : 'symbol' : '1.0-0' }}</span>
            </button>
          } @empty {
            <div class="list-empty">No products match the current filters.</div>
          }
        </div>
      </section>

      <section class="details-grid">
        <article class="panel products-panel detail-panel">
          <div>
            <p class="section-label">Selected product</p>
            <h2>{{ selectedProduct()?.name ?? 'Choose a catalog item' }}</h2>
          </div>

          @if (selectedProduct(); as product) {
            <p>{{ product.shortDescription }}</p>

            <div class="detail-meta">
              <span class="badge" [ngClass]="'badge--' + toneFor(product.status)">
                {{ statusLabel(product.status) }}
              </span>
              <span>{{ product.studioName }}</span>
              <span>{{ product.scale }}</span>
              <span>{{ product.priceAmount | currency: product.currency : 'symbol' : '1.0-0' }}</span>
            </div>

            <div class="spec-grid">
              <div>
                <span class="section-label">Materials</span>
                <strong>{{ product.materials }}</strong>
              </div>
              <div>
                <span class="section-label">Dimensions</span>
                <strong>{{ product.dimensionsText }}</strong>
              </div>
              <div>
                <span class="section-label">Edition</span>
                <strong>{{ product.editionSize }}</strong>
              </div>
              <div>
                <span class="section-label">Release</span>
                <strong>{{ product.estimatedReleaseText }}</strong>
              </div>
            </div>

            <div class="spec-list">
              @for (spec of product.specifications; track spec.label + spec.sortOrder) {
                <div class="spec-list__row">
                  <span>{{ spec.label }}</span>
                  <strong>{{ spec.value }}</strong>
                </div>
              } @empty {
                <div class="list-empty">No additional specifications were returned.</div>
              }
            </div>
          } @else {
            <p>Select a product row to inspect the seeded detail payload.</p>
          }
        </article>

        <article class="panel products-panel detail-panel">
          <div>
            <p class="section-label">Visual coverage</p>
            <h2>Primary image payload</h2>
          </div>

          @if (selectedProduct(); as product) {
            @if (product.images.length > 0) {
              <div class="image-stack">
                @for (image of product.images; track image.id) {
                  <div class="image-chip">
                    <span>Image {{ image.sortOrder + 1 }}</span>
                    <strong>{{ image.altText }}</strong>
                    <small>{{ image.publicUrl || 'No public URL' }}</small>
                  </div>
                }
              </div>
            } @else {
              <div class="list-empty">The selected product has no image entries yet.</div>
            }
          } @else {
            <p>Image metadata appears here after selecting a product.</p>
          }
        </article>
      </section>
    </section>
  `,
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
