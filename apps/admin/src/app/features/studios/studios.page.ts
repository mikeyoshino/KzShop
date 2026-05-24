import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { StudioSummary } from '../../core/models/catalog.models';
import { CatalogApiService } from '../../core/services/catalog-api.service';

@Component({
  selector: 'app-studios-page',
  standalone: true,
  imports: [FormsModule],
  template: `
    <section class="page-stack">
      <section class="panel products-panel">
        <div>
          <p class="section-label">Studios</p>
          <h2>Create studio</h2>
        </div>

        <div class="form-grid">
          <input [(ngModel)]="name" placeholder="Studio name" />
          <input [(ngModel)]="slug" placeholder="studio-slug" />
          <label class="checkbox-line">
            <input type="checkbox" [(ngModel)]="isActive" />
            <span>Active</span>
          </label>
          <button class="button-primary" type="button" (click)="createStudio()">Create</button>
        </div>
        @if (message) {
          <p>{{ message }}</p>
        }
      </section>

      <section class="panel products-panel">
        <div class="toolbar">
          <input [(ngModel)]="search" (ngModelChange)="refreshStudios()" placeholder="Search studio" />
        </div>

        <div class="list-panel">
          @for (studio of studios; track studio.id) {
            <div class="list-row">
              <div>
                <strong>{{ studio.name }}</strong>
                <p>{{ studio.slug }}</p>
              </div>
              <span class="badge">{{ studio.isActive ? 'Active' : 'Inactive' }}</span>
              <small></small>
            </div>
          } @empty {
            <div class="list-empty">No studios found.</div>
          }
        </div>
      </section>
    </section>
  `,
})
export class StudiosPageComponent {
  private readonly api = inject(CatalogApiService);
  protected name = '';
  protected slug = '';
  protected search = '';
  protected isActive = true;
  protected message = '';
  protected studios: StudioSummary[] = [];

  constructor() {
    this.refreshStudios();
  }

  protected refreshStudios(): void {
    this.api.searchStudios(this.search).subscribe((x) => {
      this.studios = x.items;
    });
  }

  protected createStudio(): void {
    this.message = '';
    this.api
      .createStudio({ name: this.name.trim(), slug: this.slug.trim(), isActive: this.isActive })
      .subscribe({
        next: () => {
          this.message = 'Studio created.';
          this.name = '';
          this.slug = '';
          this.refreshStudios();
        },
        error: () => (this.message = 'Create studio failed.'),
      });
  }
}
