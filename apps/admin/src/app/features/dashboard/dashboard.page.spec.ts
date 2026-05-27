import { CurrencyPipe } from '@angular/common';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { CatalogApiService } from '../../core/services/catalog-api.service';
import { DashboardPageComponent } from './dashboard.page';

describe('DashboardPageComponent', () => {
  let fixture: ComponentFixture<DashboardPageComponent>;
  let api: { getAdminDashboardMetrics: jasmine.Spy };

  beforeEach(async () => {
    api = {
      getAdminDashboardMetrics: jasmine.createSpy('getAdminDashboardMetrics').and.returnValue(of({
        periodRevenue: 91234,
        recognizedRevenue: 87654,
        orderCount: 27,
        criticalInventoryItems: [
          {
            productId: '4',
            productName: 'Joker: Arkham Asylum',
            studioName: 'Hot Toys',
            primaryImageUrl: 'https://placehold.co/600x800/2d1b2e/d8b4e2?text=Arkham+Joker',
            availableQuantity: 2,
          },
          {
            productId: '27',
            productName: 'Batman Beyond',
            studioName: 'Prime 1 Studio',
            primaryImageUrl: '',
            availableQuantity: 1,
          },
        ],
        recentOrders: [
          {
            orderId: 'order-1',
            orderNumber: 'REQ-9001',
            createdAtUtc: '2026-10-24T10:30:00Z',
            customerName: 'Bruce W.',
            totalAmount: 650,
            currency: 'USD',
            status: 'Pre-order',
          },
          {
            orderId: 'order-2',
            orderNumber: 'REQ-9002',
            createdAtUtc: '2026-10-25T10:30:00Z',
            customerName: 'Selina K.',
            totalAmount: 920,
            currency: 'USD',
            status: 'Awaiting Payment',
          },
        ],
      })),
    };

    await TestBed.configureTestingModule({
      imports: [DashboardPageComponent, CurrencyPipe],
      providers: [
        provideRouter([]),
        { provide: CatalogApiService, useValue: api },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardPageComponent);
    fixture.detectChanges();
  });

  it('renders the command center header from the admin dashboard template', () => {
    const element = fixture.nativeElement as HTMLElement;

    expect(element.querySelector('.command-page')).not.toBeNull();
    expect(element.querySelector('.command-heading h1')?.textContent).toContain('Command Center');
    expect(element.querySelector('.command-heading p')?.textContent).toContain('System Overview & Live Metrics');
    expect(element.querySelector('.command-sync')?.textContent).toContain('LAST SYNC');
  });

  it('loads dashboard product metrics from the admin product api', () => {
    expect(api.getAdminDashboardMetrics).toHaveBeenCalled();
  });

  it('renders template stat cards, chart, quick directives, and requisitions table', () => {
    const element = fixture.nativeElement as HTMLElement;

    expect(element.querySelectorAll('.command-stat').length).toBe(4);
    expect(element.querySelector('.command-stat')?.textContent).toContain('Period Capital');
    expect(element.querySelector('.command-chart')).not.toBeNull();
    expect(element.querySelectorAll('.command-chart__bar').length).toBe(7);
    expect(element.querySelector('.quick-directives')?.textContent).toContain('New Artifact');
    expect(element.querySelector('.critical-inventory')?.textContent).toContain('Critical Inventory');
    expect(element.querySelector('.requisitions-table')?.textContent).toContain('REQ-9001');
    expect(element.querySelector('.requisitions-table')?.textContent).toContain('REQ-9002');
    expect(element.querySelector('.critical-inventory')?.textContent).toContain('Joker: Arkham Asylum');
    expect(element.querySelector('.critical-inventory')?.textContent).toContain('Batman Beyond');
    expect(element.querySelector('.command-stat')?.textContent).toContain('$91,234');
    expect(element.querySelector('.command-stat')?.textContent).toContain('$87,654 recognized');
    expect(element.querySelector('.command-page')?.textContent).toContain('27');
  });
});
