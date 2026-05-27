import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { BusinessErrorCode } from '../../core/models/catalog.models';
import { CatalogApiService } from '../../core/services/catalog-api.service';
import { StudiosPageComponent } from './studios.page';

describe('StudiosPageComponent', () => {
  let component: StudiosPageComponent;
  let fixture: ComponentFixture<StudiosPageComponent>;
  let api: jasmine.SpyObj<CatalogApiService>;

  beforeEach(async () => {
    api = jasmine.createSpyObj<CatalogApiService>('CatalogApiService', ['searchStudios', 'createStudio']);
    api.searchStudios.and.returnValue(of({ items: [] }));
    api.createStudio.and.returnValue(of({ id: '1', name: 's', slug: 's', isActive: true }));

    await TestBed.configureTestingModule({
      imports: [StudiosPageComponent],
      providers: [
        { provide: CatalogApiService, useValue: api },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(StudiosPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('shows the business code when studio creation fails', () => {
    api.createStudio.and.returnValue(
      throwError(() => ({
        code: BusinessErrorCode.DuplicateName,
        message: 'Studio name already exists.',
        status: 409,
      })),
    );

    (component as any).name = 'Prime Studios';
    (component as any).slug = 'prime-studios';
    (component as any).createStudio();

    expect((component as any).message).toContain('[1]');
    expect((component as any).message).toContain('Studio name already exists.');
  });
});
