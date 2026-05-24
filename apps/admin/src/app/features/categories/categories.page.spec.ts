import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { CatalogApiService } from '../../core/services/catalog-api.service';
import { CategoriesPageComponent } from './categories.page';

describe('CategoriesPageComponent', () => {
  let component: CategoriesPageComponent;
  let fixture: ComponentFixture<CategoriesPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CategoriesPageComponent],
      providers: [
        {
          provide: CatalogApiService,
          useValue: {
            getCategories: () => of({ items: [] }),
            createCategory: () => of({ id: '1', name: 'c', slug: 'c', description: '', isActive: true }),
            updateCategory: () => of({ id: '1', name: 'c', slug: 'c', description: '', isActive: true }),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(CategoriesPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
