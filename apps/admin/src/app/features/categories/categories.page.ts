import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { CategorySummary, formatBusinessError } from '../../core/models/catalog.models';
import { CatalogApiService } from '../../core/services/catalog-api.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-categories-page',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './categories.page.html',
  styleUrl: './categories.page.css',
})
export class CategoriesPageComponent {
  private readonly api = inject(CatalogApiService);
  private readonly toast = inject(ToastService);
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
  protected isCreating = false;
  protected isUpdating = false;

  constructor() {
    this.refreshCategories();
  }

  protected createCategory(): void {
    if (this.isCreating) {
      return;
    }

    this.isCreating = true;
    this.api
      .createCategory({
        name: this.name.trim(),
        slug: this.slug.trim(),
        description: this.description.trim(),
        isActive: this.isActive,
      })
      .pipe(finalize(() => (this.isCreating = false)))
      .subscribe({
        next: (created) => {
          this.name = '';
          this.slug = '';
          this.description = '';
          this.message = 'Category created.';
          this.toast.success(this.message);
          this.refreshCategories();
        },
        error: (error) => {
          this.message = formatBusinessError(error, 'Create category failed.');
          this.toast.error(this.message);
        },
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
    if (!this.editing || this.isUpdating) {
      return;
    }

    this.isUpdating = true;
    this.api
      .updateCategory(this.editing.id, {
        name: this.editName.trim(),
        slug: this.editSlug.trim(),
        description: this.editDescription.trim(),
        isActive: this.editIsActive,
      })
      .pipe(finalize(() => (this.isUpdating = false)))
      .subscribe({
        next: (updated) => {
          this.categories = this.categories.map((x) => (x.id === updated.id ? updated : x));
          this.editing = null;
          this.message = 'Category updated.';
          this.toast.success(this.message);
        },
        error: (error) => {
          this.message = formatBusinessError(error, 'Update category failed.');
          this.toast.error(this.message);
        },
      });
  }

  protected refreshCategories(): void {
    this.api.getCategories(this.search).subscribe((response) => {
      this.categories = response.items;
    });
  }
}
