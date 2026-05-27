import { Injectable, computed, signal } from '@angular/core';
import { ProductDetail } from '../models/catalog.models';

export interface CartItem {
  productId: string;
  slug: string;
  name: string;
  studioName: string;
  scale: string;
  status: string;
  priceAmount: number;
  currency: string;
  imageUrl: string | null;
  quantity: number;
}

@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly cartItems = signal<CartItem[]>([]);

  readonly items = this.cartItems.asReadonly();
  readonly totalItems = computed(() => this.items().reduce((total, item) => total + item.quantity, 0));
  readonly subtotal = computed(() =>
    this.items().reduce((total, item) => total + item.priceAmount * item.quantity, 0),
  );
  readonly currency = computed(() => this.items()[0]?.currency ?? 'USD');

  addProduct(product: ProductDetail): void {
    this.addItem({
      productId: product.id,
      slug: product.slug,
      name: product.name,
      studioName: product.studioName,
      scale: product.scale,
      status: product.status,
      priceAmount: product.priceAmount,
      currency: product.currency,
      imageUrl: product.images[0]?.publicUrl ?? null,
      quantity: 1,
    });
  }

  addItem(item: CartItem): void {
    this.cartItems.update((items) => {
      const existing = items.find((cartItem) => cartItem.productId === item.productId);

      if (!existing) {
        return [...items, { ...item, quantity: Math.max(1, item.quantity) }];
      }

      return items.map((cartItem) =>
        cartItem.productId === item.productId
          ? { ...cartItem, quantity: cartItem.quantity + Math.max(1, item.quantity) }
          : cartItem,
      );
    });
  }

  increment(productId: string): void {
    this.cartItems.update((items) =>
      items.map((item) => (item.productId === productId ? { ...item, quantity: item.quantity + 1 } : item)),
    );
  }

  decrement(productId: string): void {
    this.cartItems.update((items) =>
      items
        .map((item) => (item.productId === productId ? { ...item, quantity: item.quantity - 1 } : item))
        .filter((item) => item.quantity > 0),
    );
  }

  updateQuantity(productId: string, quantity: number): void {
    this.cartItems.update((items) =>
      quantity <= 0
        ? items.filter((item) => item.productId !== productId)
        : items.map((item) => (item.productId === productId ? { ...item, quantity } : item)),
    );
  }

  remove(productId: string): void {
    this.cartItems.update((items) => items.filter((item) => item.productId !== productId));
  }

  clear(): void {
    this.cartItems.set([]);
  }
}
