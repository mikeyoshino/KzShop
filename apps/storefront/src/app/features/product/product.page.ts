import { CurrencyPipe } from '@angular/common';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { distinctUntilChanged, map, switchMap } from 'rxjs/operators';
import { ProductImage } from '../../core/models/catalog.models';
import { CartService } from '../../core/services/cart.service';
import { CatalogApiService } from '../../core/services/catalog-api.service';
import { LocalizationService } from '../../core/services/localization.service';

@Component({
  selector: 'app-product-page',
  standalone: true,
  imports: [CurrencyPipe, RouterLink],
  templateUrl: './product.page.html',
  styleUrl: './product.page.css',
})
export class ProductPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly catalogApi = inject(CatalogApiService);
  protected readonly cart = inject(CartService);
  protected readonly i18n = inject(LocalizationService);

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
      return this.i18n.t('home.preorderNow');
    }

    if (status === 'Active') {
      return this.i18n.t('product.addToCart');
    }

    if (status === 'Waitlist') {
      return this.i18n.t('product.joinWaitlist');
    }

    return this.i18n.t('product.unavailable');
  }

  protected statusLabel(status: string): string {
    if (status === 'PreOrder') {
      return this.i18n.t('common.preorder');
    }

    if (status === 'Active') {
      return this.i18n.t('common.inStock');
    }

    return status === 'Waitlist' ? this.i18n.t('common.waitlist') : status;
  }

  constructor() {
    effect(() => {
      this.slug();
      this.selectedImageIndex.set(0);
    });
  }
}
