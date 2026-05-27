import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { AuthService } from './core/services/auth.service';
import { ToastService } from './core/services/toast.service';
import { LoginPageComponent } from './features/auth/login.page';
import { App } from './app';

@Component({
  standalone: true,
  template: '<p>Dashboard</p>',
})
class DashboardStubComponent {}

describe('App', () => {
  let toast: jasmine.SpyObj<ToastService>;

  beforeEach(async () => {
    toast = jasmine.createSpyObj<ToastService>('ToastService', ['dismiss'], {
      toasts: signal([{ id: 1, tone: 'success' as const, message: 'Saved.' }]).asReadonly(),
    });

    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideRouter([
          { path: 'login', component: LoginPageComponent },
          { path: 'dashboard', component: DashboardStubComponent },
        ]),
        {
          provide: AuthService,
          useValue: {
            isAuthenticated: () => false,
            signOut: async () => undefined,
            signIn: async () => ({}),
          },
        },
        { provide: ToastService, useValue: toast },
      ],
    }).compileComponents();
  });

  it('renders auth pages without the admin dashboard shell', async () => {
    const router = TestBed.inject(Router);
    await router.navigateByUrl('/login');
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const element = fixture.nativeElement as HTMLElement;
    expect(element.querySelector('.auth-app-shell')).not.toBeNull();
    expect(element.querySelector('.admin-shell')).toBeNull();
    expect(element.querySelector('.admin-sidebar')).toBeNull();
  });

  it('renders protected workspace pages inside the admin shell', async () => {
    const router = TestBed.inject(Router);
    await router.navigateByUrl('/dashboard');
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const element = fixture.nativeElement as HTMLElement;
    expect(element.querySelector('.admin-shell')).not.toBeNull();
    expect(element.querySelector('.admin-sidebar')).not.toBeNull();
    expect(element.querySelector('.admin-dashboard-shell')).not.toBeNull();
    expect(element.querySelector('.admin-brand__badge')?.textContent).toContain('Admin');
    expect(element.querySelector('.admin-operative')?.textContent).toContain('Operative');
    expect(element.querySelector('.admin-search input')?.getAttribute('placeholder')).toBe('SEARCH REGISTRY...');
  });

  it('renders the reusable top-right toast stack', async () => {
    const router = TestBed.inject(Router);
    await router.navigateByUrl('/dashboard');
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const element = fixture.nativeElement as HTMLElement;
    expect(element.querySelector('.toast-stack')).not.toBeNull();
    expect(element.querySelector('.toast.toast--success')?.textContent).toContain('Saved.');
  });
});
