import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    spyOn(localStorage, 'getItem').and.callThrough();
    spyOn(localStorage, 'setItem').and.callThrough();
    spyOn(sessionStorage, 'getItem').and.callThrough();
    spyOn(sessionStorage, 'setItem').and.callThrough();

    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), AuthService],
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('refreshes session on startup using the HttpOnly refresh cookie', async () => {
    const request = httpMock.expectOne('/api/auth/refresh');
    expect(request.request.method).toBe('POST');
    expect(request.request.withCredentials).toBeTrue();

    request.flush({
      accessToken: 'access-token',
      expiresAtUtc: '2026-05-27T00:00:00Z',
      user: { id: 'user-id', email: 'admin@example.com', roles: ['admin'] },
    });
    await Promise.resolve();

    expect(service.accessToken()).toBe('access-token');
    expect(service.userEmail()).toBe('admin@example.com');
    expect(service.isAuthenticated()).toBeTrue();
    expect(localStorage.getItem).not.toHaveBeenCalled();
    expect(localStorage.setItem).not.toHaveBeenCalled();
    expect(sessionStorage.getItem).not.toHaveBeenCalled();
    expect(sessionStorage.setItem).not.toHaveBeenCalled();
  });

  it('signs in through the API using credentials cookies and keeps the access token in memory', async () => {
    httpMock.expectOne('/api/auth/refresh').flush({}, { status: 401, statusText: 'Unauthorized' });

    const resultPromise = service.signIn('admin@example.com', 'Admin123!');
    const request = httpMock.expectOne('/api/auth/login');
    expect(request.request.method).toBe('POST');
    expect(request.request.withCredentials).toBeTrue();
    expect(request.request.body).toEqual({ email: 'admin@example.com', password: 'Admin123!' });

    request.flush({
      accessToken: 'login-access-token',
      expiresAtUtc: '2026-05-27T00:00:00Z',
      user: { id: 'user-id', email: 'admin@example.com', roles: ['admin'] },
    });

    await expectAsync(resultPromise).toBeResolvedTo({});
    expect(service.accessToken()).toBe('login-access-token');
    expect(localStorage.setItem).not.toHaveBeenCalled();
    expect(sessionStorage.setItem).not.toHaveBeenCalled();
  });
});
