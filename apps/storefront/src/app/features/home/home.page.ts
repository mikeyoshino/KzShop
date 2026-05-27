import { CurrencyPipe } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { ProductCard } from '../../core/models/catalog.models';
import { CatalogApiService } from '../../core/services/catalog-api.service';
import { LocalizationService } from '../../core/services/localization.service';

@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [CurrencyPipe, RouterLink],
  templateUrl: './home.page.html',
  styleUrl: './home.page.css',
})
export class HomePageComponent {
  private readonly catalogApi = inject(CatalogApiService);
  protected readonly i18n = inject(LocalizationService);
  private readonly heroPreferredSlugs = ['dark-knight', 'joker-arkham-asylum', 'arkham-joker'];
  private readonly featuredPreferredSlugs = ['dark-knight', 'joker-arkham-asylum', 'arkham-joker', 'berserker-guts'];
  protected readonly tickerItems = Array(10)
    .fill(['Authentic Collectibles', 'Secure Packaging', 'Global Shipping'])
    .flat();

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
    const content: Array<{
      key: string;
      route: string[];
      label: string;
      title: string;
      imageUrl: string;
      description: string;
    }> = [
      {
        key: 'dc-batman',
        route: ['/collections', 'dc-batman'],
        label: 'DC / Batman',
        title: 'DC / Batman',
        imageUrl: 'https://placehold.co/600x800/222222/555555?text=DC+Heroes',
        description: this.describeCollectionCount(liveCounts['dc-batman'], 'Premium format pieces and Arkham shelf anchors'),
      },
      {
        key: 'anime',
        route: ['/collections', 'anime'],
        label: 'Anime',
        title: 'Anime',
        imageUrl: 'https://placehold.co/600x800/1e293b/475569?text=Anime+Figures',
        description: this.describeCollectionCount(liveCounts['anime'], 'Scale-heavy statues with painterly finishes'),
      },
      {
        key: 'gaming',
        route: ['/collections', 'gaming'],
        label: 'Gaming',
        title: 'Gaming',
        imageUrl: 'https://placehold.co/600x800/27272a/52525b?text=Gaming+Collectibles',
        description: this.describeCollectionCount(liveCounts['gaming'], 'Game-world collectibles with cinematic shelf presence'),
      },
    ];

    return content.map((collection) => ({
      ...collection,
    }));
  });

  protected heroHeadline(name: string): string {
    return name.replace(':', '');
  }

  protected heroHeadlineParts(name: string): { primary: string; secondary: string } {
    const [primary, secondary] = name.split(':').map((part) => part.trim());
    return {
      primary,
      secondary: secondary || 'Premium Format',
    };
  }

  protected statusLabel(status: string): string {
    switch (status) {
      case 'PreOrder':
        return this.i18n.t('common.preorder');
      case 'Waitlist':
        return this.i18n.t('common.waitlist');
      case 'Active':
        return this.i18n.t('common.inStock');
      default:
        return status;
    }
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
