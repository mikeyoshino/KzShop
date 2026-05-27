import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { BusinessErrorCode } from '../../core/models/catalog.models';
import { CatalogApiService } from '../../core/services/catalog-api.service';
import { ProductFormPageComponent } from './product-form.page';

describe('ProductFormPageComponent', () => {
  let component: ProductFormPageComponent;
  let fixture: ComponentFixture<ProductFormPageComponent>;
  let api: jasmine.SpyObj<CatalogApiService>;

  beforeEach(async () => {
    api = jasmine.createSpyObj<CatalogApiService>('CatalogApiService', [
      'searchStudios',
      'getCategories',
      'createStudio',
      'createProduct',
      'updateProduct',
      'archiveProduct',
      'adjustStock',
      'uploadProductImage',
      'reorderProductImages',
      'deleteProductImage',
      'getAdminProductById',
      'toSpecificationInputs',
    ]);
    api.searchStudios.and.returnValue(of({ items: [] }));
    api.getCategories.and.returnValue(
      of({ items: [{ id: 'cat-1', name: 'DC Batman', slug: 'dc-batman', description: '', isActive: true }] }),
    );
    api.createStudio.and.returnValue(of({ id: '1', name: 's', slug: 's', isActive: true }));
    api.createProduct.and.returnValue(of({ id: '1' }));
    api.updateProduct.and.returnValue(of({ id: '1' }));
    api.archiveProduct.and.returnValue(of({ id: '1', status: 'Archived' as never }));
    api.adjustStock.and.returnValue(of({ productId: '1', onHandQuantity: 1, reservedQuantity: 0, availableQuantity: 1 }));
    api.uploadProductImage.and.returnValue(of({ id: '1', publicUrl: '', altText: '', sortOrder: 0 }));
    api.reorderProductImages.and.returnValue(of({ imageIds: [] }));
    api.deleteProductImage.and.returnValue(of({}));
    api.getAdminProductById.and.returnValue(of(null));
    api.toSpecificationInputs.and.returnValue([]);

    await TestBed.configureTestingModule({
      imports: [ProductFormPageComponent],
      providers: [
        { provide: CatalogApiService, useValue: api },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => 'new' } } } },
        { provide: Router, useValue: { navigate: () => Promise.resolve(true) } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ProductFormPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('loads categories for the product editor', () => {
    expect(api.getCategories).toHaveBeenCalledWith('');
    expect((component as any).categories).toEqual([
      { id: 'cat-1', name: 'DC Batman', slug: 'dc-batman', description: '', isActive: true },
    ]);
  });

  it('shows the business code when product creation fails', () => {
    api.createProduct.and.returnValue(
      throwError(() => ({
        code: BusinessErrorCode.DuplicateSku,
        message: 'SKU already exists.',
        status: 409,
      })),
    );

    (component as any).saveProduct();

    expect((component as any).message).toContain('[2]');
    expect((component as any).message).toContain('SKU already exists.');
  });
});
