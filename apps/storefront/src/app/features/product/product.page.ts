import { CurrencyPipe, TitleCasePipe } from '@angular/common';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { distinctUntilChanged, map, switchMap } from 'rxjs/operators';
import { ProductImage } from '../../core/models/catalog.models';
import { CatalogApiService } from '../../core/services/catalog-api.service';

@Component({
  selector: 'app-product-page',
  standalone: true,
  imports: [CurrencyPipe, RouterLink, TitleCasePipe],
  template: `
    <main class="section section--tight">
      <div class="shell-inner">
        @if (product(); as product) {
          <div class="breadcrumb-row">
            <a routerLink="/">Home</a>
            <span>/</span>
            <a [routerLink]="['/collections', product.categorySlug]">{{ collectionTitle() }}</a>
            <span>/</span>
            <span>{{ product.name }}</span>
          </div>

          <section class="product-layout">
            <div class="product-gallery">
              <div class="product-gallery__thumbs">
                @for (image of product.images; track image.id; let index = $index) {
                  <button
                    type="button"
                    class="gallery-thumb"
                    [class.is-active]="selectedImageIndex() === index"
                    (click)="selectedImageIndex.set(index)"
                    [style.--product-image]="'url(' + image.publicUrl + ')'"
                  >
                    <span>{{ image.altText || 'View ' + (index + 1) }}</span>
                  </button>
                }
              </div>

              <div
                class="product-hero-card"
                [attr.data-tone]="product.categorySlug"
                [style.--product-image]="'url(' + activeImage().publicUrl + ')'"
              >
                <div class="product-card__badges">
                  <span class="badge" [class.badge--accent]="product.status === 'PreOrder'">
                    {{ product.status === 'PreOrder' ? 'Pre-order' : product.status | titlecase }}
                  </span>
                  @if (product.isNew) {
                    <span class="badge badge--dark">New</span>
                  }
                </div>
                <div class="product-hero-card__content">
                  <strong>{{ activeImage().altText || product.name }}</strong>
                  <span>{{ product.scale }}</span>
                </div>
              </div>
            </div>

            <div class="product-details">
              <div class="product-details__topline">
                <span>{{ product.studioName }}</span>
                <span>ID: {{ product.id.slice(0, 4) }}</span>
              </div>

              <h1 class="page-title">{{ product.name }}</h1>

              <div class="product-price-row">
                <strong>{{ product.priceAmount | currency: product.currency : 'symbol' : '1.0-0' }}</strong>
                <span>Non-refundable deposit required</span>
              </div>

              <p class="product-summary">
                {{ product.shortDescription }}
              </p>

              <p class="product-description">
                {{ product.description }}
              </p>

              <div class="product-actions">
                <button type="button" class="button button--accent" disabled>
                  {{ primaryCtaLabel(product.status) }}
                </button>
                <div class="product-flags">
                  <span>Secure ship</span>
                  <span>Authentic</span>
                </div>
              </div>

              <section class="spec-panel">
                <h2>Specifications</h2>
                <div class="spec-list">
                  <div class="spec-row">
                    <span>Franchise</span>
                    <strong>{{ collectionTitle() }}</strong>
                  </div>
                  <div class="spec-row">
                    <span>Scale</span>
                    <strong>{{ product.scale }}</strong>
                  </div>
                  <div class="spec-row">
                    <span>Materials</span>
                    <strong>{{ product.materials }}</strong>
                  </div>
                  <div class="spec-row">
                    <span>Dimensions</span>
                    <strong>{{ product.dimensionsText }}</strong>
                  </div>
                  <div class="spec-row">
                    <span>Edition Size</span>
                    <strong>{{ product.editionSize }}</strong>
                  </div>
                  <div class="spec-row">
                    <span>Est. Release</span>
                    <strong>{{ product.estimatedReleaseText }}</strong>
                  </div>
                  @for (specification of product.specifications; track specification.label + specification.sortOrder) {
                    <div class="spec-row">
                      <span>{{ specification.label }}</span>
                      <strong>{{ specification.value }}</strong>
                    </div>
                  }
                </div>
              </section>
            </div>
          </section>
        } @else {
          <div class="empty-state">
            <h1 class="page-title">Product unavailable</h1>
            <p>The requested figure is not public in the catalog right now.</p>
            <a class="button button--dark" routerLink="/collections/dc-batman">Browse Batman archive</a>
          </div>
        }
      </div>
    </main>
  `,
})
export class ProductPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly catalogApi = inject(CatalogApiService);

  protected readonly selectedImageIndex = signal(0);

  private readonly slug = toSignal(
    this.route.paramMap.pipe(
      map((params) => (params.get('slug') ?? '').trim()),
      distinctUntilChanged(),
    ),
    { initialValue: 'dark-knight' },
  );

  protected readonly product = toSignal(
    this.route.paramMap.pipe(
      map((params) => (params.get('slug') ?? '').trim()),
      distinctUntilChanged(),
      switchMap((slug) => this.catalogApi.getProductBySlug(slug)),
    ),
    { initialValue: null },
  );

  protected readonly activeImage = computed<ProductImage>(() => {
    const product = this.product();
    if (!product || product.images.length === 0) {
      return { id: 'fallback', publicUrl: '', altText: product?.name ?? 'Display view', sortOrder: 0 };
    }

    return product.images[this.selectedImageIndex()] ?? product.images[0];
  });

  protected readonly collectionTitle = computed(() => {
    const categorySlug = this.product()?.categorySlug ?? '';
    if (categorySlug === 'dc-batman') {
      return 'DC Comics';
    }

    if (categorySlug === 'anime') {
      return 'Anime';
    }

    return categorySlug.replace(/-/g, ' ').toUpperCase();
  });

  protected primaryCtaLabel(status: string): string {
    if (status === 'PreOrder') {
      return 'Pre-order opens soon';
    }

    if (status === 'Active') {
      return 'Catalog only for now';
    }

    if (status === 'Waitlist') {
      return 'Join waitlist soon';
    }

    return 'Unavailable for purchase';
  }

  constructor() {
    effect(() => {
      this.slug();
      this.selectedImageIndex.set(0);
    });
  }
}
