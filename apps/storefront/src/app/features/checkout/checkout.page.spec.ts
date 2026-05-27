import { CurrencyPipe } from '@angular/common';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { CartService } from '../../core/services/cart.service';
import { CheckoutPageComponent } from './checkout.page';

describe('CheckoutPageComponent', () => {
  let fixture: ComponentFixture<CheckoutPageComponent>;
  let cart: CartService;

  beforeEach(async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [CheckoutPageComponent, CurrencyPipe],
      providers: [provideRouter([])],
    }).compileComponents();

    cart = TestBed.inject(CartService);
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
      quantity: 1,
    });
  });

  it('renders a checkout page consistent with the template order summary', () => {
    fixture = TestBed.createComponent(CheckoutPageComponent);
    fixture.detectChanges();
    const element = fixture.nativeElement as HTMLElement;

    expect(element.querySelector('.checkout-page .page-title')?.textContent).toContain('ชำระเงินปลอดภัย');
    expect(element.querySelector('.checkout-form')?.textContent).toContain('ข้อมูลติดต่อ');
    expect(element.querySelector('.checkout-form')?.textContent).toContain('ที่อยู่จัดส่ง');
    expect(element.querySelector('.order-summary')?.textContent).toContain('สรุปคำสั่งซื้อ');
    expect(element.querySelector('.order-summary')?.textContent).toContain('The Dark Knight: Premium Format');
    expect(element.querySelector('.order-summary')?.textContent).toContain('$650');
  });
});
