import { CurrencyPipe } from '@angular/common';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { CatalogApiService } from '../../core/services/catalog-api.service';
import { HomePageComponent } from './home.page';

const products = [
  {
    id: '1',
    name: 'The Dark Knight: Premium Format',
    slug: 'dark-knight',
    categorySlug: 'dc-batman',
    studioName: 'Prime Studios',
    priceAmount: 650,
    currency: 'USD',
    scale: '1/4 Scale',
    status: 'PreOrder',
    isNew: true,
    primaryImageUrl: 'https://placehold.co/800x1000/1a1a1a/ffffff?text=Dark+Knight+1/4',
  },
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
    id: '3',
    name: 'Cyber Ninja V',
    slug: 'cyber-ninja-v',
    categorySlug: 'gaming',
    studioName: 'Hot Toys',
    priceAmount: 280,
    currency: 'USD',
    scale: '1/6 Scale',
    status: 'Active',
    isNew: true,
    primaryImageUrl: 'https://placehold.co/600x800/1e293b/94a3b8?text=Cyber+Ninja',
  },
  {
    id: '4',
    name: 'Joker: Arkham Asylum',
    slug: 'joker-arkham-asylum',
    categorySlug: 'dc-batman',
    studioName: 'Hot Toys',
    priceAmount: 320,
    currency: 'USD',
    scale: '1/6 Scale',
    status: 'Active',
    isNew: false,
    primaryImageUrl: 'https://placehold.co/600x800/2d1b2e/d8b4e2?text=Arkham+Joker',
  },
];

describe('HomePageComponent', () => {
  let fixture: ComponentFixture<HomePageComponent>;

  beforeEach(async () => {
    localStorage.clear();

    await TestBed.configureTestingModule({
      imports: [HomePageComponent, CurrencyPipe],
      providers: [
        provideRouter([]),
        {
          provide: CatalogApiService,
          useValue: {
            getProducts: () => of({ items: products }),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(HomePageComponent);
    fixture.detectChanges();
  });

  it('renders the template hero with cinematic background image, badge, headline, and preorder CTA', () => {
    const element = fixture.nativeElement as HTMLElement;
    const hero = element.querySelector('.hero') as HTMLElement;

    expect(hero).withContext('hero section exists').not.toBeNull();
    expect(hero.querySelector('.hero__background')?.getAttribute('src')).toContain(
      'Cinematic+Batman+Shot',
    );
    expect(hero.querySelector('.badge--accent')?.textContent?.trim()).toBe('สินค้าใหม่');
    expect(hero.querySelector('h1')?.textContent).toContain('The Dark Knight');
    expect(hero.querySelector('h1')?.textContent).toContain('Premium Format');
    expect(hero.querySelector('.button--light')?.textContent).toContain('พรีออร์เดอร์ทันที');
  });

  it('renders image-backed franchise cards and template section headings', () => {
    const element = fixture.nativeElement as HTMLElement;
    const cards = Array.from(element.querySelectorAll('.franchise-card'));

    expect(element.querySelector('.collections-heading h2')?.textContent).toContain('เลือกตามจักรวาล');
    expect(cards.length).toBe(3);
    expect(cards[0].querySelector('img')?.getAttribute('src')).toContain('DC+Heroes');
    expect(cards[1].querySelector('img')?.getAttribute('src')).toContain('Anime+Figures');
    expect(cards[2].querySelector('img')?.getAttribute('src')).toContain('Gaming+Collectibles');
  });

  it('renders latest allocation cards with media image and separate product metadata', () => {
    const element = fixture.nativeElement as HTMLElement;
    const cards = Array.from(element.querySelectorAll('.product-card'));

    expect(element.querySelector('.allocations-heading h2')?.textContent).toContain('รอบจัดสรรล่าสุด');
    expect(cards.length).toBe(4);
    expect(cards[0].querySelector('.product-card__media img')?.getAttribute('src')).toContain(
      'Dark+Knight',
    );
    expect(cards[0].querySelector('.product-card__title')).toBeNull();
    expect(cards[0].querySelector('.product-card__name')?.textContent).toContain(
      'The Dark Knight: Premium Format',
    );
    expect(cards[0].querySelector('.product-card__price-row')?.textContent).toContain('$650');
  });
});
