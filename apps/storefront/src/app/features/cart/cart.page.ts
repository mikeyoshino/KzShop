import { CurrencyPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CartService } from '../../core/services/cart.service';
import { LocalizationService } from '../../core/services/localization.service';

@Component({
  selector: 'app-cart-page',
  standalone: true,
  imports: [CurrencyPipe, RouterLink],
  templateUrl: './cart.page.html',
  styleUrl: './cart.page.css',
})
export class CartPageComponent {
  protected readonly cart = inject(CartService);
  protected readonly i18n = inject(LocalizationService);

  protected statusLabel(status: string): string {
    return status === 'PreOrder' ? this.i18n.t('cart.preorderItem') : this.i18n.t('cart.inStock');
  }
}
