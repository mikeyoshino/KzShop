import { CurrencyPipe } from '@angular/common';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { CatalogApiService } from '../../core/services/catalog-api.service';
import { ProductsPageComponent } from './products.page';

describe('ProductsPageComponent', () => {
  let fixture: ComponentFixture<ProductsPageComponent>;
  let api: jasmine.SpyObj<CatalogApiService>;

  beforeEach(async () => {
    api = jasmine.createSpyObj<CatalogApiService>('CatalogApiService', [
      'getAdminProducts',
      'getAdminProductById',
    ]);
    api.getAdminProducts.and.returnValue(
      of({
        items: [
          {
            id: '1',
            name: 'Berserker Armor Guts',
            slug: 'berserker-guts',
            categorySlug: 'anime',
            studioName: 'Dark Art Studios',
            priceAmount: 1200,
            currency: 'USD',
            scale: '1/3 Scale',
            status: 'Waitlist',
            isNew: true,
            primaryImageUrl: null,
          },
        ],
      }),
    );
    api.getAdminProductById.and.returnValue(
      of({
        id: '1',
        name: 'Berserker Armor Guts',
        slug: 'berserker-guts',
        shortDescription: 'Massive centerpiece.',
        description: 'Massive centerpiece.',
        categoryId: 'cat-1',
        categoryName: 'Anime',
        studioId: 'studio-1',
        studioName: 'Dark Art Studios',
        sku: 'GUTS-001',
        priceAmount: 1200,
        currency: 'USD',
        status: 'Waitlist' as never,
        isNew: true,
        isFeatured: false,
        scale: '1/3 Scale',
        materials: 'Polystone, Resin',
        dimensionsText: 'H: 30 x W: 18 x D: 14',
        editionSize: '500',
        estimatedReleaseText: 'Waitlist',
        inventoryOnHand: 0,
        inventoryReserved: 0,
        inventoryAvailable: 0,
        images: [{ id: 'img-1', publicUrl: '/uploads/guts.jpg', altText: 'Guts', sortOrder: 0 }],
        specifications: [{ id: 'spec-1', label: 'Franchise', value: 'Berserk', sortOrder: 0 }],
      }),
    );

    await TestBed.configureTestingModule({
      imports: [ProductsPageComponent, CurrencyPipe],
      providers: [provideRouter([]), { provide: CatalogApiService, useValue: api }],
    }).compileComponents();

    fixture = TestBed.createComponent(ProductsPageComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
  });

  it('renders inventory with separated table columns and detail panels', () => {
    const element = fixture.nativeElement as HTMLElement;

    expect(element.querySelector('.inventory-page')).not.toBeNull();
    expect(element.querySelector('.inventory-table__head')).not.toBeNull();
    expect(element.querySelectorAll('.inventory-table__head span').length).toBe(4);
    expect(element.querySelector('.inventory-row')).not.toBeNull();
    expect(element.querySelector('.inventory-row__product')?.textContent).toContain('Berserker Armor Guts');
    expect(element.querySelector('.inventory-details')).not.toBeNull();
    expect(element.querySelector('.inventory-image-panel')).not.toBeNull();
  });
});
