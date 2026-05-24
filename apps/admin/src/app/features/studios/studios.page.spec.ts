import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { CatalogApiService } from '../../core/services/catalog-api.service';
import { StudiosPageComponent } from './studios.page';

describe('StudiosPageComponent', () => {
  let component: StudiosPageComponent;
  let fixture: ComponentFixture<StudiosPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StudiosPageComponent],
      providers: [
        {
          provide: CatalogApiService,
          useValue: {
            searchStudios: () => of({ items: [] }),
            createStudio: () => of({ id: '1', name: 's', slug: 's', isActive: true }),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(StudiosPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
