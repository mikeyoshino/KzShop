import { CurrencyPipe, TitleCasePipe } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { distinctUntilChanged, map, switchMap } from 'rxjs/operators';
import { CatalogApiService } from '../../core/services/catalog-api.service';

@Component({
  selector: 'app-collection-page',
  standalone: true,
  imports: [CurrencyPipe, RouterLink, TitleCasePipe],
  template: `
    <main class="section section--tight">
      <div class="shell-inner">
        <div class="breadcrumb-row">
          <a routerLink="/">Home</a>
          <span>/</span>
          <span>{{ collectionLabel() }}</span>
        </div>

        <div class="collection-header">
          <div>
            <p class="section-kicker">Curated archive</p>
            <h1 class="page-title">{{ collectionTitle() }}</h1>
            <p class="page-subtitle">{{ products().items.length }} items found in the archive.</p>
          </div>
          <div class="collection-summary">
            <span>Sort by: newest</span>
          </div>
        </div>

        <div class="collection-layout">
          <aside class="filter-panel">
            <div class="filter-group">
              <h2>Status</h2>
              <div class="filter-list">
                @for (status of statuses(); track status.label) {
                  <div class="filter-item">
                    <span>{{ status.label }}</span>
                    <strong>{{ status.count }}</strong>
                  </div>
                }
              </div>
            </div>

            <div class="filter-group">
              <h2>Scale</h2>
              <div class="filter-list">
                @for (scale of scales(); track scale.label) {
                  <div class="filter-item">
                    <span>{{ scale.label }}</span>
                    <strong>{{ scale.count }}</strong>
                  </div>
                }
              </div>
            </div>
          </aside>

          <section class="collection-results">
            @if (products().items.length > 0) {
              <div class="product-grid product-grid--wide">
                @for (product of products().items; track product.id) {
                  <article class="product-card product-card--listing" [attr.data-tone]="product.categorySlug">
                    <a
                      class="product-card__media product-card__media--tall"
                      [routerLink]="['/products', product.slug]"
                      [style.--product-image]="'url(' + (product.primaryImageUrl ?? '') + ')'"
                    >
                      <div class="product-card__badges">
                        @if (product.isNew) {
                          <span class="badge badge--dark">New</span>
                        }
                        <span class="badge" [class.badge--accent]="product.status === 'PreOrder'">
                          {{ product.status === 'PreOrder' ? 'Pre-order' : product.status | titlecase }}
                        </span>
                      </div>
                      <div class="product-card__eyebrow">{{ product.studioName }}</div>
                      <div class="product-card__title">{{ product.name }}</div>
                    </a>
                    <div class="product-card__meta">
                      <p>{{ collectionTitle() }}</p>
                      <a class="product-card__name" [routerLink]="['/products', product.slug]">
                        {{ product.name }}
                      </a>
                      <div class="product-card__price-row">
                        <strong>{{ product.priceAmount | currency: product.currency : 'symbol' : '1.0-0' }}</strong>
                        <span>{{ product.scale }}</span>
                      </div>
                    </div>
                  </article>
                }
              </div>
            } @else {
              <div class="empty-state">
                <h2>No public products are live in this collection yet.</h2>
                <p>Try the anime archive or go back to the latest drops.</p>
                <a class="button button--dark" routerLink="/">Return home</a>
              </div>
            }
          </section>
        </div>
      </div>
    </main>
  `,
})
export class CollectionPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly catalogApi = inject(CatalogApiService);

  private readonly slug = toSignal(
    this.route.paramMap.pipe(
      map((params) => (params.get('slug') ?? '').trim().toLowerCase()),
      distinctUntilChanged(),
    ),
    { initialValue: 'dc-batman' },
  );

  protected readonly products = toSignal(
    this.route.paramMap.pipe(
      map((params) => (params.get('slug') ?? '').trim().toLowerCase()),
      distinctUntilChanged(),
      switchMap((slug) => this.catalogApi.getProducts(slug || undefined)),
    ),
    { initialValue: { items: [] } },
  );

  protected readonly collectionTitle = computed(() => {
    const slug = this.slug();
    const labels: Record<string, string> = {
      'dc-batman': 'DC / Batman',
      anime: 'Anime',
    };

    return labels[slug] ?? slug.replace(/-/g, ' ').toUpperCase();
  });

  protected readonly collectionLabel = computed(() => this.collectionTitle());

  protected readonly statuses = computed(() =>
    this.buildCounts(this.products().items.map((item) => item.status), ['Active', 'PreOrder', 'Waitlist']),
  );

  protected readonly scales = computed(() =>
    this.buildCounts(this.products().items.map((item) => item.scale), ['1/3 Scale', '1/4 Scale', '1/6 Scale']),
  );

  private buildCounts(values: string[], preferredOrder: string[]): Array<{ label: string; count: number }> {
    const counts = new Map<string, number>();

    for (const value of values) {
      counts.set(value, (counts.get(value) ?? 0) + 1);
    }

    return preferredOrder
      .filter((value) => counts.has(value))
      .map((value) => ({ label: this.normalizeLabel(value), count: counts.get(value) ?? 0 }));
  }

  private normalizeLabel(value: string): string {
    return value === 'PreOrder' ? 'Pre-order' : value;
  }
}
