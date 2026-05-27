import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { CartService } from './core/services/cart.service';
import { LocalizationService } from './core/services/localization.service';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([])],
    }).compileComponents();
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render title', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.brand__mark')?.textContent).toContain('Archive');
    expect(compiled.querySelector('.brand__sub')?.textContent).toContain('Collectibles');
    expect(compiled.querySelector('.site-nav')?.textContent).toContain('สินค้าใหม่');
    expect(compiled.querySelector('.cart-action .fa-cart-shopping')).not.toBeNull();
    expect(compiled.querySelector('.site-footer')?.textContent).toContain('ARCHIVE COLLECTIBLES');
  });

  it('shows a modern flag language dropdown and switches menu text to English', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const trigger = compiled.querySelector('.language-switcher__trigger') as HTMLButtonElement;

    expect(trigger.querySelector('.language-switcher__flag svg')).not.toBeNull();
    expect(trigger.querySelector('.language-switcher__flag')?.getAttribute('aria-label')).toBe('Thailand');
    expect(trigger.textContent).toContain('TH');
    expect(trigger.getAttribute('aria-expanded')).toBe('false');
    expect(compiled.querySelector('.language-switcher__menu')).toBeNull();
    expect(compiled.querySelector('.site-nav')?.textContent).toContain('สินค้าใหม่');

    trigger.click();
    fixture.detectChanges();

    const options = Array.from(compiled.querySelectorAll('.language-switcher__menu [role="option"]')) as HTMLButtonElement[];
    expect(trigger.getAttribute('aria-expanded')).toBe('true');
    expect(options[0].querySelector('.language-switcher__flag')?.getAttribute('aria-label')).toBe('Thailand');
    expect(options[1].querySelector('.language-switcher__flag')?.getAttribute('aria-label')).toBe('United States');
    expect(options.map((button) => button.querySelector('span:last-child')?.textContent?.trim())).toEqual(['TH', 'EN']);

    options.find((button) => button.textContent?.includes('EN'))?.click();
    fixture.detectChanges();

    expect(TestBed.inject(LocalizationService).language()).toBe('en');
    expect(compiled.querySelector('.site-nav')?.textContent).toContain('New Drops');
    expect(compiled.querySelector('.language-switcher__menu')).toBeNull();
  });

  it('links the cart icon to the cart route and shows live cart count', () => {
    const cart = TestBed.inject(CartService);
    cart.clear();
    cart.addItem({
      productId: '1',
      slug: 'dark-knight',
      name: 'The Dark Knight: Premium Format',
      studioName: 'Prime Studios',
      scale: '1/4 Scale',
      status: 'PreOrder',
      priceAmount: 650,
      currency: 'USD',
      imageUrl: 'https://placehold.co/800x1000/1a1a1a/ffffff?text=Dark+Knight+1/4',
      quantity: 2,
    });

    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    const cartLink = compiled.querySelector('.cart-action') as HTMLAnchorElement;

    expect(cartLink.getAttribute('href')).toBe('/cart');
    expect(cartLink.querySelector('small')?.textContent?.trim()).toBe('2');
  });

  it('wraps routed pages in the template fade container and replays it on route activation', (done) => {
    const scrollTo = spyOn(window, 'scrollTo');
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const app = fixture.componentInstance as unknown as {
      onRouteActivate: () => void;
    };
    const compiled = fixture.nativeElement as HTMLElement;
    const container = compiled.querySelector('#app-container');

    expect(container).not.toBeNull();
    expect(container?.classList.contains('fade-in')).toBeTrue();
    const animationDuration = getComputedStyle(container as Element).animationDuration;
    expect(animationDuration).toBe('0.7s');

    app.onRouteActivate();
    fixture.detectChanges();
    expect(container?.classList.contains('fade-in')).toBeFalse();
    expect(scrollTo).toHaveBeenCalledWith(0, 0);

    setTimeout(() => {
      fixture.detectChanges();
      expect(container?.classList.contains('fade-in')).toBeTrue();
      done();
    });
  });
});
