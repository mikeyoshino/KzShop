import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Subject, of, throwError } from 'rxjs';
import { BusinessErrorCode } from '../../core/models/catalog.models';
import { CatalogApiService } from '../../core/services/catalog-api.service';
import { ToastService } from '../../core/services/toast.service';
import { CategoriesPageComponent } from './categories.page';

describe('CategoriesPageComponent', () => {
  let component: CategoriesPageComponent;
  let fixture: ComponentFixture<CategoriesPageComponent>;
  let api: jasmine.SpyObj<CatalogApiService>;
  let toast: jasmine.SpyObj<ToastService>;

  beforeEach(async () => {
    api = jasmine.createSpyObj<CatalogApiService>('CatalogApiService', ['getCategories', 'createCategory', 'updateCategory']);
    toast = jasmine.createSpyObj<ToastService>('ToastService', ['success', 'error']);
    api.getCategories.and.returnValue(of({ items: [] }));
    api.createCategory.and.returnValue(of({ id: '1', name: 'c', slug: 'c', description: '', isActive: true }));
    api.updateCategory.and.returnValue(of({ id: '1', name: 'c', slug: 'c', description: '', isActive: true }));

    await TestBed.configureTestingModule({
      imports: [CategoriesPageComponent],
      providers: [
        { provide: CatalogApiService, useValue: api },
        { provide: ToastService, useValue: toast },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(CategoriesPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('renders category management with styled form, toggle, and row actions', () => {
    api.getCategories.and.returnValue(
      of({
        items: [
          {
            id: '1',
            name: 'DC / Batman',
            slug: 'dc-batman',
            description: 'DC collectibles',
            isActive: true,
          },
        ],
      }),
    );
    (component as any).refreshCategories();
    fixture.detectChanges();

    const element = fixture.nativeElement as HTMLElement;

    expect(element.querySelector('.category-management')).not.toBeNull();
    expect(element.querySelector('.category-form-grid')).not.toBeNull();
    expect(element.querySelector('.category-toggle input[type="checkbox"]')).not.toBeNull();
    expect(element.querySelector('.category-table')).not.toBeNull();
    expect(element.querySelector('.category-row__actions .button-ghost')).not.toBeNull();
  });

  it('shows the business code when category creation fails', () => {
    api.createCategory.and.returnValue(
      throwError(() => ({
        code: BusinessErrorCode.DuplicateSlug,
        message: 'Category slug already exists.',
        status: 409,
      })),
    );

    (component as any).name = 'Batman';
    (component as any).slug = 'batman';
    (component as any).createCategory();

    expect((component as any).message).toContain('[0]');
    expect((component as any).message).toContain('Category slug already exists.');
    expect(toast.error).toHaveBeenCalledWith('[0] Category slug already exists.');
  });

  it('shows loading state while creating a category', () => {
    const createSubject = new Subject<any>();
    api.createCategory.and.returnValue(createSubject.asObservable());

    (component as any).name = 'Batman';
    (component as any).slug = 'batman';
    (component as any).createCategory();
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('.category-submit') as HTMLButtonElement;
    expect((component as any).isCreating).toBeTrue();
    expect(button.disabled).toBeTrue();
    expect(button.querySelector('.button-spinner')).not.toBeNull();
    expect(button.textContent).toContain('Creating');

    createSubject.next({ id: '2', name: 'Batman', slug: 'batman', description: '', isActive: true });
    createSubject.complete();
    fixture.detectChanges();

    expect((component as any).isCreating).toBeFalse();
    expect(toast.success).toHaveBeenCalledWith('Category created.');
  });

  it('shows loading state while updating a category', () => {
    const updateSubject = new Subject<any>();
    api.updateCategory.and.returnValue(updateSubject.asObservable());
    (component as any).edit({
      id: '1',
      name: 'DC / Batman',
      slug: 'dc-batman',
      description: '',
      isActive: true,
    });
    fixture.detectChanges();

    (component as any).updateCategory();
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('.category-update-submit') as HTMLButtonElement;
    expect((component as any).isUpdating).toBeTrue();
    expect(button.disabled).toBeTrue();
    expect(button.querySelector('.button-spinner')).not.toBeNull();
    expect(button.textContent).toContain('Saving');

    updateSubject.next({ id: '1', name: 'DC / Batman', slug: 'dc-batman', description: '', isActive: true });
    updateSubject.complete();
    fixture.detectChanges();

    expect((component as any).isUpdating).toBeFalse();
    expect(toast.success).toHaveBeenCalledWith('Category updated.');
  });
});
