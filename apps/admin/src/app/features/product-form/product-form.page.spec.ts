import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { of } from 'rxjs';
import { CatalogApiService } from '../../core/services/catalog-api.service';
import { ProductFormPageComponent } from './product-form.page';

describe('ProductFormPageComponent', () => {
  let component: ProductFormPageComponent;
  let fixture: ComponentFixture<ProductFormPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductFormPageComponent],
      providers: [
        {
          provide: CatalogApiService,
          useValue: {
            searchStudios: () => of({ items: [] }),
            createStudio: () => of({ id: '1', name: 's', slug: 's', isActive: true }),
            createProduct: () => of({ id: '1' }),
            updateProduct: () => of({ id: '1' }),
            archiveProduct: () => of({ id: '1', status: 'Archived' }),
            adjustStock: () => of({}),
            uploadProductImage: () => of({ id: '1', publicUrl: '', altText: '', sortOrder: 0 }),
            reorderProductImages: () => of({ imageIds: [] }),
            deleteProductImage: () => of({}),
            getAdminProductById: () => of(null),
            toSpecificationInputs: () => [],
          },
        },
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
});
