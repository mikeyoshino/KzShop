import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { Subject } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { LoginPageComponent } from './login.page';

describe('LoginPageComponent', () => {
  let component: LoginPageComponent;
  let fixture: ComponentFixture<LoginPageComponent>;
  let auth: jasmine.SpyObj<AuthService>;
  let toast: jasmine.SpyObj<ToastService>;
  let router: Router;

  beforeEach(async () => {
    auth = jasmine.createSpyObj<AuthService>('AuthService', ['signIn']);
    toast = jasmine.createSpyObj<ToastService>('ToastService', ['success', 'error']);
    auth.signIn.and.resolveTo({});

    await TestBed.configureTestingModule({
      imports: [LoginPageComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: auth },
        { provide: ToastService, useValue: toast },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LoginPageComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('renders a full-page split login experience', () => {
    const element = fixture.nativeElement as HTMLElement;

    expect(element.querySelector('.auth-shell--fullscreen')).not.toBeNull();
    expect(element.querySelector('.auth-hero')).not.toBeNull();
    expect(element.querySelector('.auth-card')).not.toBeNull();
    expect(element.querySelector('.auth-panel')).toBeNull();
  });

  it('shows the authentication error returned by the auth service', async () => {
    auth.signIn.and.resolveTo({ error: 'Invalid login credentials' });

    await (component as any).submit();

    expect((component as any).error).toBe('Invalid login credentials');
    expect(toast.error).toHaveBeenCalledWith('Invalid login credentials');
  });

  it('navigates to the dashboard after a successful login', async () => {
    const navigateSpy = spyOn(router, 'navigate').and.resolveTo(true);

    await (component as any).submit();

    expect(navigateSpy).toHaveBeenCalledWith(['/dashboard']);
    expect(toast.success).toHaveBeenCalledWith('Signed in.');
  });

  it('shows a loading screen and disables sign in while submitting', () => {
    const signInSubject = new Subject<{ error?: string }>();
    auth.signIn.and.returnValue(new Promise((resolve) => signInSubject.subscribe(resolve)));

    void (component as any).submit();
    fixture.detectChanges();

    const element = fixture.nativeElement as HTMLElement;
    const button = element.querySelector('.auth-submit') as HTMLButtonElement;

    expect((component as any).isSubmitting).toBeTrue();
    expect(element.querySelector('.auth-loading-screen')).not.toBeNull();
    expect(button.disabled).toBeTrue();
    expect(button.querySelector('.button-spinner')).not.toBeNull();

    signInSubject.next({});
    signInSubject.complete();
  });
});
