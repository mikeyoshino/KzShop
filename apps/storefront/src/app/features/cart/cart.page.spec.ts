import { CurrencyPipe } from '@angular/common';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { CartService } from '../../core/services/cart.service';
import { CartPageComponent } from './cart.page';

describe('CartPageComponent', () => {
  let fixture: ComponentFixture<CartPageComponent>;
  let cart: CartService;

  beforeEach(async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [CartPageComponent, CurrencyPipe],
      providers: [provideRouter([])],
    }).compileComponents();

    cart = TestBed.inject(CartService);
    cart.clear();
  });

  it('renders the template empty cart state', () => {
    fixture = TestBed.createComponent(CartPageComponent);
    fixture.detectChanges();
    const element = fixture.nativeElement as HTMLElement;

    expect(element.querySelector('.cart-page .page-title')?.textContent).toContain('รายการจอง');
    expect(element.querySelector('.cart-empty')?.textContent).toContain('ตะกร้าว่าง');
    expect(element.querySelector('.cart-empty .fa-cart-shopping')).not.toBeNull();
    expect(element.querySelector('.cart-empty a')?.textContent).toContain('กลับสู่ Archive');
  });

  it('renders cart items, quantity controls, and the sticky order summary', () => {
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

    fixture = TestBed.createComponent(CartPageComponent);
    fixture.detectChanges();
    const element = fixture.nativeElement as HTMLElement;

    expect(element.querySelectorAll('.cart-item').length).toBe(1);
    expect(element.querySelector('.cart-item')?.textContent).toContain('Prime Studios | 1/4 Scale');
    expect(element.querySelector('.cart-item')?.textContent).toContain('สินค้าพรีออร์เดอร์');
    expect(element.querySelector('.cart-quantity')?.textContent).toContain('2');
    expect(element.querySelector('.order-summary')?.textContent).toContain('สรุปคำสั่งซื้อ');
    expect(element.querySelector('.order-summary')?.textContent).toContain('$1,300');
    expect(element.querySelector('.order-summary a')?.textContent).toContain('ชำระเงินปลอดภัย');
  });
});
