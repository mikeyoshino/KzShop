import { HttpErrorResponse } from '@angular/common/http';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AdminDashboardMetricsResponse, BusinessErrorCode, isBusinessError } from '../models/catalog.models';
import { CatalogApiService } from './catalog-api.service';

describe('CatalogApiService', () => {
  let service: CatalogApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), CatalogApiService],
    });

    service = TestBed.inject(CatalogApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('maps admin business failures to typed business errors', () => {
    let receivedError: unknown;

    service
      .createStudio({ name: 'Prime Studios', slug: 'prime-studios', isActive: true })
      .subscribe({
        next: fail,
        error: (error) => {
          receivedError = error;
        },
      });

    const request = httpMock.expectOne('/api/admin/studios');
    request.flush(
      { code: BusinessErrorCode.DuplicateName, message: 'Studio name already exists.' },
      { status: 409, statusText: 'Conflict' },
    );

    expect(isBusinessError(receivedError)).toBeTrue();
    expect(receivedError).toEqual({
      code: BusinessErrorCode.DuplicateName,
      message: 'Studio name already exists.',
      status: 409,
    });
  });

  it('preserves non-business http failures as HttpErrorResponse', () => {
    let receivedError: unknown;

    service
      .createStudio({ name: 'Prime Studios', slug: 'prime-studios', isActive: true })
      .subscribe({
        next: fail,
        error: (error) => {
          receivedError = error;
        },
      });

    const request = httpMock.expectOne('/api/admin/studios');
    request.flush({ detail: 'boom' }, { status: 500, statusText: 'Server Error' });

    expect(receivedError instanceof HttpErrorResponse).toBeTrue();
  });

  it('gets admin dashboard metrics from the admin dashboard endpoint', () => {
    const expected: AdminDashboardMetricsResponse = {
      periodRevenue: 3200.5,
      recognizedRevenue: 5000.75,
      orderCount: 5,
      criticalInventoryItems: [
        {
          productId: '45a75568-b167-44a0-8f02-730fda4ec53f',
          productName: 'Batman Beyond: Modern Suit',
          studioName: 'Prime Studios',
          primaryImageUrl: 'https://cdn.example.com/products/batman-beyond.jpg',
          availableQuantity: 2,
        },
      ],
      recentOrders: [
        {
          orderId: 'fb6474b4-1ef7-49c0-ac99-c4f19855da92',
          orderNumber: 'ORD-2026-001',
          createdAtUtc: '2026-05-24T12:30:00Z',
          customerName: 'Bruce Wayne',
          totalAmount: 1999.99,
          currency: 'USD',
          status: 'Paid',
        },
      ],
    };

    let actual: AdminDashboardMetricsResponse | undefined;

    service.getAdminDashboardMetrics().subscribe((response) => {
      actual = response;
    });

    const request = httpMock.expectOne('/api/admin/dashboard/metrics');
    expect(request.request.method).toBe('GET');
    request.flush(expected);

    expect(actual).toEqual(expected);
  });
});
