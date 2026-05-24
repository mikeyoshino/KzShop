import { CurrencyPipe, NgClass } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs/operators';
import { ProductCard } from '../../core/models/catalog.models';
import { CatalogApiService } from '../../core/services/catalog-api.service';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [CurrencyPipe, NgClass],
  template: `
    <section class="page-stack">
      <article class="panel hero-panel">
        <div>
          <p class="section-label">Daily overview</p>
          <h2>Catalog operations are reading the seeded archive in real time.</h2>
        </div>
        <p>
          The first admin slice stays read-only, but it now reflects the live seeded catalog for
          release review, studio coverage, and merchandised status checks.
        </p>
      </article>

      <section class="metrics-grid">
        <article class="panel metric-card">
          <span class="metric-card__label">Products</span>
          <strong>{{ products().length }}</strong>
          <p class="metric-card__meta">Public catalog items currently exposed by the API.</p>
        </article>

        <article class="panel metric-card">
          <span class="metric-card__label">Studios</span>
          <strong>{{ studioCount() }}</strong>
          <p class="metric-card__meta">Studios represented across the seeded figure lineup.</p>
        </article>

        <article class="panel metric-card">
          <span class="metric-card__label">Pre-orders</span>
          <strong>{{ preOrderCount() }}</strong>
          <p class="metric-card__meta">Items currently merchandised in a preorder state.</p>
        </article>
      </section>

      <section class="panel list-panel">
        <div>
          <p class="section-label">Status board</p>
          <h2>Catalog mix</h2>
        </div>

        @for (metric of statusBreakdown(); track metric.label) {
          <div class="list-row list-row--metric">
            <div>
              <strong>{{ metric.label }}</strong>
              <p>{{ metric.copy }}</p>
            </div>
            <span class="badge" [ngClass]="'badge--' + metric.tone">{{ metric.count }}</span>
            <small>{{ metric.share }}</small>
          </div>
        } @empty {
          <div class="list-empty">No catalog data is available from the API.</div>
        }
      </section>

      <section class="panel list-panel">
        <div>
          <p class="section-label">Recent read model</p>
          <h2>Products in view</h2>
        </div>

        @for (product of products(); track product.id) {
          <div class="list-row">
            <div>
              <strong>{{ product.name }}</strong>
              <p>{{ product.studioName }} | {{ product.scale }} | {{ product.categorySlug }}</p>
            </div>
            <span class="badge" [ngClass]="'badge--' + toneFor(product.status)">
              {{ labelFor(product.status) }}
            </span>
            <small>{{ product.priceAmount | currency: product.currency : 'symbol' : '1.0-0' }}</small>
          </div>
        } @empty {
          <div class="list-empty">No catalog data is available from the API.</div>
        }
      </section>
    </section>
  `,
})
export class DashboardPageComponent {
  private readonly catalogApi = inject(CatalogApiService);

  protected readonly products = toSignal(
    this.catalogApi.getProducts().pipe(map((response) => response.items)),
    { initialValue: [] as ProductCard[] },
  );

  protected readonly studioCount = computed(
    () => new Set(this.products().map((product) => product.studioName)).size,
  );

  protected readonly preOrderCount = computed(
    () => this.products().filter((product) => product.status === 'PreOrder').length,
  );

  protected readonly statusBreakdown = computed(() => {
    const products = this.products();
    const total = products.length || 1;
    const specs = [
      {
        status: 'Active',
        label: 'Active release',
        copy: 'Public products available for the current catalog window.',
        tone: 'active',
      },
      {
        status: 'PreOrder',
        label: 'Pre-order queue',
        copy: 'Products still in the forward allocation and launch pipeline.',
        tone: 'preorder',
      },
      {
        status: 'Waitlist',
        label: 'Waitlist watch',
        copy: 'Demand exceeds current availability and needs allocation review.',
        tone: 'waitlist',
      },
    ] as const;

    return specs.map((spec) => {
      const count = products.filter((product) => product.status === spec.status).length;
      return {
        ...spec,
        count,
        share: `${Math.round((count / total) * 100)}% of visible catalog`,
      };
    });
  });

  protected labelFor(status: string): string {
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
