import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { StudioSummary, formatBusinessError } from '../../core/models/catalog.models';
import { CatalogApiService } from '../../core/services/catalog-api.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-studios-page',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './studios.page.html',
  styleUrl: './studios.page.css',
})
export class StudiosPageComponent {
  private readonly api = inject(CatalogApiService);
  private readonly toast = inject(ToastService);
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
          this.toast.success(this.message);
          this.name = '';
          this.slug = '';
          this.refreshStudios();
        },
        error: (error) => {
          this.message = formatBusinessError(error, 'Create studio failed.');
          this.toast.error(this.message);
        },
      });
  }
}
