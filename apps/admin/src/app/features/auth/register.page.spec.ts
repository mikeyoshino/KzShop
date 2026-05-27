import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { RegisterPageComponent } from './register.page';

describe('RegisterPageComponent', () => {
  let component: RegisterPageComponent;
  let fixture: ComponentFixture<RegisterPageComponent>;
  let auth: jasmine.SpyObj<AuthService>;
  let router: Router;

  beforeEach(async () => {
    auth = jasmine.createSpyObj<AuthService>('AuthService', ['signUp']);
    auth.signUp.and.resolveTo({});

    await TestBed.configureTestingModule({
      imports: [RegisterPageComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: auth },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(RegisterPageComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('shows the authentication error returned by the auth service', async () => {
    auth.signUp.and.resolveTo({ error: 'User already registered' });

    await (component as any).submit();

    expect((component as any).error).toBe('User already registered');
  });

  it('navigates to the dashboard after a successful registration', async () => {
    const navigateSpy = spyOn(router, 'navigate').and.resolveTo(true);

    await (component as any).submit();

    expect(navigateSpy).toHaveBeenCalledWith(['/dashboard']);
  });
});
