import { CurrencyPipe } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { ProductCard } from '../../core/models/catalog.models';
import { CatalogApiService } from '../../core/services/catalog-api.service';

@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [CurrencyPipe, RouterLink],
  template: `
    <main>
      <section class="hero hero--batman">
        <div class="shell-inner hero__inner">
          @if (heroProduct(); as heroProduct) {
            <div class="hero__copy">
              <div class="eyebrow-row">
                @if (heroProduct.isNew) {
                  <span class="badge badge--accent">New Drop</span>
                }
                <span class="eyebrow-muted">{{ heroProduct.scale }} | {{ heroProduct.studioName }}</span>
              </div>
              <h1>{{ heroHeadline(heroProduct.name) }}</h1>
              <p>
                {{ heroDescription(heroProduct) }}
              </p>
              <div class="hero__actions">
                <a class="button button--light" [routerLink]="['/products', heroProduct.slug]">
                  {{ heroProduct.status === 'PreOrder' ? 'Pre-order now' : 'View figure' }}
                </a>
                <a class="button button--ghost" [routerLink]="['/collections', heroProduct.categorySlug]">
                  View archive
                </a>
              </div>
            </div>
          }
        </div>
      </section>

      <section class="ticker-strip" aria-label="Store assurances">
        <div class="shell-inner ticker-strip__inner">
          <span>Authentic collectibles</span>
          <span>Secure packaging</span>
          <span>Global shipping</span>
          <span>Curated allocations</span>
        </div>
      </section>

      <section class="section">
        <div class="shell-inner">
          <div class="section-heading">
            <div>
              <p class="section-kicker">Franchises</p>
              <h2>Browse by world</h2>
            </div>
            <a class="section-link" routerLink="/collections/dc-batman">View all</a>
          </div>

          <div class="franchise-grid">
            @for (collection of collections(); track collection.key) {
              <a class="franchise-card" [routerLink]="collection.route">
                <span class="franchise-card__label">{{ collection.label }}</span>
                <strong>{{ collection.title }}</strong>
                <p>{{ collection.description }}</p>
              </a>
            }
          </div>
        </div>
      </section>

      <section class="section section--muted">
        <div class="shell-inner">
          <div class="section-heading">
            <div>
              <p class="section-kicker">Latest allocations</p>
              <h2>Recently added stock and pre-orders</h2>
            </div>
            <a class="section-link" routerLink="/collections/dc-batman">Shop new</a>
          </div>

          <div class="product-grid">
            @for (product of featuredProducts(); track product.id) {
              <article class="product-card" [attr.data-tone]="product.categorySlug">
                <a
                  class="product-card__media"
                  [routerLink]="['/products', product.slug]"
                  [style.--product-image]="'url(' + (product.primaryImageUrl ?? '') + ')'"
                >
                  <div class="product-card__badges">
                    @if (product.isNew) {
                      <span class="badge badge--dark">New</span>
                    }
                    <span class="badge" [class.badge--accent]="product.status === 'PreOrder'">
                      {{ product.status === 'PreOrder' ? 'Pre-order' : product.status }}
                    </span>
                  </div>
                  <div class="product-card__title">{{ product.name }}</div>
                </a>
                <div class="product-card__meta">
                  <p>{{ product.studioName }}</p>
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
        </div>
      </section>

      <section class="section section--editorial">
        <div class="shell-inner editorial-grid">
          <div class="editorial-copy">
            <p class="section-kicker">The archive standard</p>
            <h2>Curated for the discerning eye.</h2>
            <p>
              We do not sell toys. We build a catalog around museum-grade display pieces with
              disciplined curation, strong product storytelling, and enough technical detail for
              collectors to buy with confidence.
            </p>
            <div class="editorial-metrics">
              <div>
                <strong>{{ featuredCount() }}</strong>
                <span>Live featured drops</span>
              </div>
              <div>
                <strong>3</strong>
                <span>Studios in seed catalog</span>
              </div>
            </div>
          </div>

          <div class="editorial-panel">
            <div class="editorial-panel__frame">
              <div class="editorial-panel__card">
                <span>Craftsmanship detail</span>
              </div>
            </div>
          </div>
        </div>
      </section>
    </main>
  `,
})
export class HomePageComponent {
  private readonly catalogApi = inject(CatalogApiService);
  private readonly heroPreferredSlugs = ['dark-knight', 'joker-arkham-asylum', 'arkham-joker'];
  private readonly featuredPreferredSlugs = ['dark-knight', 'joker-arkham-asylum', 'arkham-joker', 'berserker-guts'];

  private readonly catalogResponse = toSignal(this.catalogApi.getProducts(), {
    initialValue: { items: [] },
  });

  protected readonly featuredProducts = computed(() =>
    this.curateProducts(this.catalogResponse().items, this.featuredPreferredSlugs).slice(0, 4),
  );

  protected readonly heroProduct = computed(() => {
    const items = this.catalogResponse().items;
    return this.pickPreferredProduct(items, this.heroPreferredSlugs) ?? this.featuredProducts()[0] ?? items[0] ?? null;
  });

  protected readonly featuredCount = computed(() => this.featuredProducts().length);

  protected readonly collections = computed(() => {
    const liveCounts = this.catalogResponse().items.reduce<Record<string, number>>((counts, product) => {
      counts[product.categorySlug] = (counts[product.categorySlug] ?? 0) + 1;
      return counts;
    }, {});
    const heroProduct = this.heroProduct();

    const content: Array<{
      key: string;
      route: string[];
      label: string;
      title: string;
      description: string;
    }> = [
      {
        key: 'dc-batman',
        route: ['/collections', 'dc-batman'],
        label: 'DC / Batman',
        title: 'Gotham Icons',
        description: this.describeCollectionCount(liveCounts['dc-batman'], 'Premium format pieces and Arkham shelf anchors'),
      },
      {
        key: 'anime',
        route: ['/collections', 'anime'],
        label: 'Anime',
        title: 'Anime Titans',
        description: this.describeCollectionCount(liveCounts['anime'], 'Scale-heavy statues with painterly finishes'),
      },
      {
        key: 'collector-edit',
        route: heroProduct ? ['/products', heroProduct.slug] : ['/collections', 'dc-batman'],
        label: 'Collector Edit',
        title: 'Flagship Figure',
        description: this.describeCollectionCount(
          heroProduct ? 1 : 0,
          'Lead collectible selected from the live catalog',
        ),
      },
    ];

    return content.map((collection) => ({
      ...collection,
    }));
  });

  protected heroHeadline(name: string): string {
    return name.replace(':', '');
  }

  protected heroDescription(product: { studioName: string; scale: string; status: string }): string {
    const availability = (() => {
      switch (product.status) {
        case 'PreOrder':
          return 'reserved now for the next allocation window';
        case 'Waitlist':
          return 'currently in waitlist status for the next allocation release';
        case 'Active':
          return 'available in the live catalog now';
        default:
          return 'currently being prepared for catalog release';
      }
    })();

    return `${product.scale} display piece from ${product.studioName}, ${availability}, with the cinematic proportions and specification detail expected from serious collector stock.`;
  }

  private curateProducts(products: ProductCard[], preferredSlugs: string[]): ProductCard[] {
    const preferred = preferredSlugs
      .map((slug) => products.find((product) => product.slug === slug))
      .filter((product): product is ProductCard => product !== undefined);

    const seen = new Set(preferred.map((product) => product.slug));
    const remaining = products.filter((product) => !seen.has(product.slug));

    return [...preferred, ...remaining];
  }

  private pickPreferredProduct(products: ProductCard[], preferredSlugs: string[]): ProductCard | null {
    return preferredSlugs
      .map((slug) => products.find((product) => product.slug === slug))
      .find((product): product is ProductCard => product !== undefined) ?? null;
  }

  private describeCollectionCount(count: number | undefined, copy: string): string {
    if (!count) {
      return `${copy}. Live catalog inventory is still building.`;
    }

    const noun = count === 1 ? 'piece' : 'pieces';
    return `${copy}. ${count} ${noun} live now.`;
  }
}
