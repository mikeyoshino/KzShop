import { CurrencyPipe } from '@angular/common';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { CatalogApiService } from '../../core/services/catalog-api.service';
import { ProductPageComponent } from './product.page';

const product = {
  id: '0002',
  name: 'Berserker Armor Guts',
  slug: 'berserker-guts',
  studioName: 'Dark Art Studios',
  categorySlug: 'anime',
  priceAmount: 1200,
  currency: 'USD',
  description:
    'The ultimate representation of the Black Swordsman. Featuring LED light-up eyes, real metal chains, and a brutal display base.',
  shortDescription: 'A massive 1/3 scale anime centerpiece with armor and light-up detail.',
  scale: '1/3 Scale',
  materials: 'Polystone, Metal, LED',
  dimensionsText: 'H: 38" x W: 25" x D: 22"',
  editionSize: 'Limited to 500 pieces',
  estimatedReleaseText: 'Waitlist Open',
  status: 'Waitlist',
  isNew: false,
  images: [
    {
      id: 'main',
      publicUrl: 'https://placehold.co/800x1000/2c2c2c/e0e0e0?text=Guts+Berserker',
      altText: 'Guts Berserker',
      sortOrder: 0,
    },
    {
      id: 'detail',
      publicUrl: 'https://placehold.co/800x1000/111111/ffffff?text=Detail+Shot',
      altText: 'Detail Shot',
      sortOrder: 1,
    },
  ],
  specifications: [],
};

describe('ProductPageComponent', () => {
  let fixture: ComponentFixture<ProductPageComponent>;

  beforeEach(async () => {
    localStorage.clear();

    await TestBed.configureTestingModule({
      imports: [ProductPageComponent, CurrencyPipe],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            paramMap: of(convertToParamMap({ slug: 'berserker-guts' })),
          },
        },
        {
          provide: CatalogApiService,
          useValue: {
            getProductBySlug: () => of(product),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ProductPageComponent);
    fixture.detectChanges();
  });

  it('renders the template PDP breadcrumb and gallery with real images', () => {
    const element = fixture.nativeElement as HTMLElement;

    expect(element.querySelector('.breadcrumb-row')?.textContent).toContain('หน้าแรก');
    expect(element.querySelector('.product-gallery__thumbs img')?.getAttribute('src')).toContain('Guts+Berserker');
    expect(element.querySelector('.product-gallery__main img')?.getAttribute('src')).toContain('Guts+Berserker');
    expect(element.querySelector('.product-gallery__main .product-card__badges')?.textContent).toContain('รอคิว');
  });

  it('renders the sticky template info column with CTA, trust flags, and specifications', () => {
    const element = fixture.nativeElement as HTMLElement;
    const details = element.querySelector('.product-details') as HTMLElement;

    expect(details.classList).toContain('product-details--sticky');
    expect(details.querySelector('.product-details__header')?.textContent).toContain('Dark Art Studios');
    expect(details.querySelector('.page-title')?.textContent).toContain('Berserker Armor Guts');
    expect(details.querySelector('.product-price-row')?.textContent).toContain('$1,200');
    expect(details.querySelector('.product-purchase .button')?.textContent).toContain('เข้าร่วมคิวรอ');
    expect(details.querySelector('.product-flags')?.textContent).toContain('จัดส่งปลอดภัย');
    expect(details.querySelector('.spec-panel')?.textContent).toContain('ข้อมูลจำเพาะ');
    expect(details.querySelector('.spec-panel')?.textContent).toContain('Polystone, Metal, LED');
  });
});
