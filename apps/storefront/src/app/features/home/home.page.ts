import { CurrencyPipe } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { map } from 'rxjs/operators';
import { CatalogApiService } from '../../core/services/catalog-api.service';

@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [CurrencyPipe, RouterLink],
  template: `
    <main>
      <section class="hero hero--batman">
        <div class="shell-inner hero__inner">
          <div class="hero__copy">
            <div class="eyebrow-row">
              <span class="badge badge--accent">New Drop</span>
              <span class="eyebrow-muted">Edition Size: 1000</span>
            </div>
            <h1>The Dark Knight Premium Format</h1>
            <p>
              A 1/4 scale cinematic centerpiece with tailored textures, interchangeable
              portraits, and a display footprint built for serious shelves.
            </p>
            <div class="hero__actions">
              <a class="button button--light" routerLink="/products/dark-knight">Pre-order now</a>
              <a class="button button--ghost" routerLink="/collections/dc-batman">View Batman archive</a>
            </div>
          </div>
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
            @for (collection of collections; track collection.slug) {
              <a class="franchise-card" [routerLink]="['/collections', collection.slug]">
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
                <a class="product-card__media" [routerLink]="['/products', product.slug]">
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

  protected readonly featuredProducts = toSignal(
    this.catalogApi.getProducts().pipe(map((response) => response.items.slice(0, 4))),
    { initialValue: [] },
  );

  protected readonly featuredCount = computed(() => this.featuredProducts().length);

  protected readonly collections = [
    {
      slug: 'dc-batman',
      label: 'DC / Batman',
      title: 'DC Heroes',
      description: 'Premium format pieces, Arkham lines, and Gotham shelf anchors.',
    },
    {
      slug: 'anime',
      label: 'Anime',
      title: 'Anime Figures',
      description: 'Scale-heavy statues with painterly finishes and oversized bases.',
    },
    {
      slug: 'dc-batman',
      label: 'Gaming',
      title: 'Gaming Collectibles',
      description: 'Game-inspired variants, darker palettes, and display-led sculpts.',
    },
  ];
}
