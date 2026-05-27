import { TestBed } from '@angular/core/testing';
import { ProductDetail } from '../models/catalog.models';
import { CartService } from './cart.service';

const product: ProductDetail = {
  id: '1',
  name: 'The Dark Knight: Premium Format',
  slug: 'dark-knight',
  studioName: 'Prime Studios',
  categorySlug: 'dc-batman',
  priceAmount: 650,
  currency: 'USD',
  description: 'A cinematic masterpiece.',
  shortDescription: 'Premium figure.',
  scale: '1/4 Scale',
  materials: 'Polystone, Fabric, PVC',
  dimensionsText: 'H: 24"',
  editionSize: 'Limited to 1000 pieces',
  estimatedReleaseText: 'Est. Q4 2026',
  status: 'PreOrder',
  isNew: true,
  images: [
    {
      id: 'image-1',
      publicUrl: 'https://placehold.co/800x1000/1a1a1a/ffffff?text=Dark+Knight+1/4',
      altText: 'Dark Knight',
      sortOrder: 0,
    },
  ],
  specifications: [],
};

describe('CartService', () => {
  let service: CartService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CartService);
    service.clear();
  });

  it('adds products, increments quantity, and calculates totals', () => {
    service.addProduct(product);
    service.addProduct(product);

    expect(service.items().length).toBe(1);
    expect(service.items()[0].quantity).toBe(2);
    expect(service.totalItems()).toBe(2);
    expect(service.subtotal()).toBe(1300);
  });

  it('updates and removes cart quantities', () => {
    service.addProduct(product);
    service.updateQuantity(product.id, 3);
    expect(service.items()[0].quantity).toBe(3);

    service.decrement(product.id);
    expect(service.items()[0].quantity).toBe(2);

    service.remove(product.id);
    expect(service.items()).toEqual([]);
  });
});
