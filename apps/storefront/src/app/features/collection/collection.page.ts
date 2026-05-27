import { CurrencyPipe } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { distinctUntilChanged, map, switchMap } from 'rxjs/operators';
import { CatalogApiService } from '../../core/services/catalog-api.service';
import { LocalizationService } from '../../core/services/localization.service';

@Component({
  selector: 'app-collection-page',
  standalone: true,
  imports: [CurrencyPipe, RouterLink],
  templateUrl: './collection.page.html',
  styleUrl: './collection.page.css',
})
export class CollectionPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly catalogApi = inject(CatalogApiService);
  protected readonly i18n = inject(LocalizationService);

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

  protected readonly statusFilters = ['In Stock', 'Pre-order', 'Waitlist'];

  protected readonly scaleFilters = ['1/3 Scale', '1/4 Scale', '1/6 Scale'];

  protected readonly statusFilterKeys = ['common.inStock', 'common.preorder', 'common.waitlist'];

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

  protected statusLabel(status: string): string {
    if (status === 'PreOrder') {
      return this.i18n.t('common.preorder');
    }

    if (status === 'Active') {
      return this.i18n.t('common.inStock');
    }

    return this.normalizeLabel(status);
  }
}
