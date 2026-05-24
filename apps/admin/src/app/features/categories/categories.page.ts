import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CategorySummary } from '../../core/models/catalog.models';
import { CatalogApiService } from '../../core/services/catalog-api.service';

@Component({
  selector: 'app-categories-page',
  standalone: true,
  imports: [FormsModule],
  template: `
    <section class="page-stack">
      <section class="panel products-panel">
        <div>
          <p class="section-label">Categories</p>
          <h2>Create category</h2>
        </div>
        <div class="form-grid">
          <input [(ngModel)]="name" placeholder="Category name" />
          <input [(ngModel)]="slug" placeholder="category-slug" />
          <input [(ngModel)]="description" placeholder="Description" />
          <label class="checkbox-line">
            <input type="checkbox" [(ngModel)]="isActive" />
            <span>Active</span>
          </label>
          <button class="button-primary" type="button" (click)="createCategory()">Create</button>
        </div>
        @if (message) {
          <p>{{ message }}</p>
        }
      </section>

      <section class="panel products-panel">
        <div class="toolbar">
          <input [(ngModel)]="search" (ngModelChange)="refreshCategories()" placeholder="Search category" />
        </div>
        <div>
          <p class="section-label">Managed list</p>
          <h2>Categories</h2>
        </div>
        @for (category of categories; track category.id) {
          <div class="list-row">
            <div>
              <strong>{{ category.name }}</strong>
              <p>{{ category.slug }}</p>
            </div>
            <span class="badge">{{ category.isActive ? 'Active' : 'Inactive' }}</span>
            <button type="button" (click)="edit(category)">Edit</button>
          </div>
        } @empty {
          <div class="list-empty">No categories.</div>
        }
      </section>

      @if (editing) {
        <section class="panel products-panel">
          <div>
            <p class="section-label">Update</p>
            <h2>{{ editing.name }}</h2>
          </div>
          <div class="form-grid">
            <input [(ngModel)]="editName" placeholder="Category name" />
            <input [(ngModel)]="editSlug" placeholder="category-slug" />
            <input [(ngModel)]="editDescription" placeholder="Description" />
            <label class="checkbox-line">
              <input type="checkbox" [(ngModel)]="editIsActive" />
              <span>Active</span>
            </label>
            <button class="button-primary" type="button" (click)="updateCategory()">Save</button>
          </div>
        </section>
      }
    </section>
  `,
})
export class CategoriesPageComponent {
  private readonly api = inject(CatalogApiService);
  protected categories: CategorySummary[] = [];
  protected editing: CategorySummary | null = null;
  protected message = '';

  protected name = '';
  protected slug = '';
  protected description = '';
  protected isActive = true;

  protected editName = '';
  protected editSlug = '';
  protected editDescription = '';
  protected editIsActive = true;
  protected search = '';

  constructor() {
    this.refreshCategories();
  }

  protected createCategory(): void {
    this.api
      .createCategory({
        name: this.name.trim(),
        slug: this.slug.trim(),
        description: this.description.trim(),
        isActive: this.isActive,
      })
      .subscribe({
        next: (created) => {
          this.name = '';
          this.slug = '';
          this.description = '';
          this.message = 'Category created.';
          this.refreshCategories();
        },
        error: () => (this.message = 'Create category failed.'),
      });
  }

  protected edit(category: CategorySummary): void {
    this.editing = category;
    this.editName = category.name;
    this.editSlug = category.slug;
    this.editDescription = category.description;
    this.editIsActive = category.isActive;
  }

  protected updateCategory(): void {
    if (!this.editing) {
      return;
    }

    this.api
      .updateCategory(this.editing.id, {
        name: this.editName.trim(),
        slug: this.editSlug.trim(),
        description: this.editDescription.trim(),
        isActive: this.editIsActive,
      })
      .subscribe({
        next: (updated) => {
          this.categories = this.categories.map((x) => (x.id === updated.id ? updated : x));
          this.editing = null;
          this.message = 'Category updated.';
        },
        error: () => (this.message = 'Update category failed.'),
      });
  }

  protected refreshCategories(): void {
    this.api.getCategories(this.search).subscribe((response) => {
      this.categories = response.items;
    });
  }
}
