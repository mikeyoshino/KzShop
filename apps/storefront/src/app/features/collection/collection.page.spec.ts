import { CurrencyPipe } from '@angular/common';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { convertToParamMap, ActivatedRoute, provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { CatalogApiService } from '../../core/services/catalog-api.service';
import { CollectionPageComponent } from './collection.page';

const products = [
  {
    id: '2',
    name: 'Berserker Armor Guts',
    slug: 'berserker-guts',
    categorySlug: 'anime',
    studioName: 'Dark Art Studios',
    priceAmount: 1200,
    currency: 'USD',
    scale: '1/3 Scale',
    status: 'Waitlist',
    isNew: false,
    primaryImageUrl: 'https://placehold.co/800x1000/2c2c2c/e0e0e0?text=Guts+Berserker',
  },
  {
    id: '5',
    name: 'Goku: Ultra Instinct',
    slug: 'goku-ultra-instinct',
    categorySlug: 'anime',
    studioName: 'Bandai Spirits',
    priceAmount: 150,
    currency: 'USD',
    scale: 'Non-Scale',
    status: 'Active',
    isNew: true,
    primaryImageUrl: 'https://placehold.co/600x800/f8fafc/64748b?text=Ultra+Instinct',
  },
];

describe('CollectionPageComponent', () => {
  let fixture: ComponentFixture<CollectionPageComponent>;

  beforeEach(async () => {
    localStorage.clear();

    await TestBed.configureTestingModule({
      imports: [CollectionPageComponent, CurrencyPipe],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            paramMap: of(convertToParamMap({ slug: 'anime' })),
          },
        },
        {
          provide: CatalogApiService,
          useValue: {
            getProducts: () => of({ items: products }),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(CollectionPageComponent);
    fixture.detectChanges();
  });

  it('renders the template PLP header and breadcrumb treatment', () => {
    const element = fixture.nativeElement as HTMLElement;

    expect(element.querySelector('.collection-page__header')).not.toBeNull();
    expect(element.querySelector('.breadcrumb-row')?.textContent).toContain('หน้าแรก');
    expect(element.querySelector('.page-title')?.textContent?.trim()).toBe('Anime');
    expect(element.querySelector('.page-subtitle')?.textContent).toContain('2 รายการใน Archive');
  });

  it('renders checkbox filters, apply button, and sort select like the template', () => {
    const element = fixture.nativeElement as HTMLElement;
    const filters = Array.from(element.querySelectorAll('.filter-panel input[type="checkbox"]'));

    expect(filters.length).toBeGreaterThanOrEqual(6);
    expect(element.querySelector('.filter-panel button')?.textContent).toContain('ใช้ตัวกรอง');
    expect(element.querySelector('.collection-sort select')).not.toBeNull();
    expect(element.querySelector('.collection-sort')?.textContent).toContain('แสดงผลทั้งหมด');
  });

  it('renders PLP cards with image media, quick details overlay, and separate metadata', () => {
    const element = fixture.nativeElement as HTMLElement;
    const cards = Array.from(element.querySelectorAll('.collection-card'));

    expect(cards.length).toBe(2);
    expect(cards[0].querySelector('.collection-card__media img')?.getAttribute('src')).toContain('Guts+Berserker');
    expect(cards[0].querySelector('.collection-card__quick')?.textContent).toContain('ดูรายละเอียด');
    expect(cards[0].querySelector('.collection-card__meta')?.textContent).toContain('Dark Art Studios');
    expect(cards[0].querySelector('.collection-card__meta')?.textContent).toContain('$1,200');
  });
});
