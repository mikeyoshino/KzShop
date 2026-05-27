import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import {
  CategorySummary,
  ProductStatus,
  StudioSummary,
  formatBusinessError,
} from '../../core/models/catalog.models';
import { CatalogApiService } from '../../core/services/catalog-api.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-product-form-page',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './product-form.page.html',
  styleUrl: './product-form.page.css',
})
export class ProductFormPageComponent {
  private readonly api = inject(CatalogApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  protected readonly statuses = Object.values(ProductStatus);
  protected productId = '';
  protected isCreateMode = true;
  protected message = '';

  protected name = '';
  protected slug = '';
  protected shortDescription = '';
  protected description = '';
  protected categoryId = '';
  protected studioId = '';
  protected sku = '';
  protected priceAmount = 0;
  protected currency = 'USD';
  protected status: ProductStatus = ProductStatus.PreOrder;
  protected isNew = false;
  protected isFeatured = false;
  protected scale = '';
  protected materials = '';
  protected dimensionsText = '';
  protected editionSize = '';
  protected estimatedReleaseText = '';
  protected specifications: Array<{ label: string; value: string }> = [{ label: '', value: '' }];

  protected studioSearch = '';
  protected studios: StudioSummary[] = [];
  protected categories: CategorySummary[] = [];

  protected quantityDelta = 0;
  protected imageAltText = '';
  protected images: Array<{ id: string; publicUrl: string; altText: string; sortOrder: number }> = [];
  private selectedFile: File | null = null;

  constructor() {
    const routeId = this.route.snapshot.paramMap.get('id') ?? 'new';
    this.productId = routeId;
    this.isCreateMode = routeId === 'new';
    this.fetchCategories();
    this.fetchStudios('');
    if (!this.isCreateMode) {
      this.loadProduct(routeId);
    }
  }

  protected fetchCategories(): void {
    this.api.getCategories('').subscribe((response) => {
      this.categories = response.items;
    });
  }

  protected fetchStudios(search: string): void {
    this.api.searchStudios(search).subscribe((response) => {
      this.studios = response.items;
    });
  }

  protected createStudioQuick(): void {
    const name = this.studioSearch.trim();
    if (!name) {
      return;
    }

    const slug = name.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '');
    this.api.createStudio({ name, slug, isActive: true }).subscribe({
      next: (studio) => {
        this.studioId = studio.id;
        this.setSuccess('Studio created.');
        this.fetchStudios(this.studioSearch);
      },
      error: (error) => this.setError(formatBusinessError(error, 'Create studio failed.')),
    });
  }

  protected addSpecRow(): void {
    this.specifications = [...this.specifications, { label: '', value: '' }];
  }

  protected saveProduct(): void {
    const input = {
      name: this.name.trim(),
      slug: this.slug.trim(),
      shortDescription: this.shortDescription.trim(),
      description: this.description.trim(),
      categoryId: this.categoryId.trim(),
      studioId: this.studioId.trim(),
      sku: this.sku.trim(),
      priceAmount: Number(this.priceAmount),
      currency: this.currency.trim().toUpperCase(),
      status: this.status,
      isNew: this.isNew,
      isFeatured: this.isFeatured,
      scale: this.scale.trim(),
      materials: this.materials.trim(),
      dimensionsText: this.dimensionsText.trim(),
      editionSize: this.editionSize.trim(),
      estimatedReleaseText: this.estimatedReleaseText.trim(),
      specifications: this.api.toSpecificationInputs(this.specifications),
    };

    if (this.isCreateMode) {
      this.api.createProduct(input).subscribe({
        next: (created) => {
          this.setSuccess('Product created.');
          this.router.navigate(['/products', created.id]);
        },
        error: (error) => this.setError(formatBusinessError(error, 'Create product failed.')),
      });
      return;
    }

    this.api.updateProduct(this.productId, input).subscribe({
      next: () => this.setSuccess('Product updated.'),
      error: (error) => this.setError(formatBusinessError(error, 'Update product failed.')),
    });
  }

  protected archiveProduct(): void {
    this.api.archiveProduct(this.productId).subscribe({
      next: () => this.setSuccess('Product archived.'),
      error: (error) => this.setError(formatBusinessError(error, 'Archive failed.')),
    });
  }

  protected adjustStock(): void {
    this.api.adjustStock(this.productId, Number(this.quantityDelta)).subscribe({
      next: () => this.setSuccess('Inventory updated.'),
      error: (error) => this.setError(formatBusinessError(error, 'Inventory update failed.')),
    });
  }

  protected onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile = input.files?.[0] ?? null;
  }

  protected uploadImage(): void {
    if (!this.selectedFile) {
      return;
    }

    this.api.uploadProductImage(this.productId, this.selectedFile, this.imageAltText.trim()).subscribe({
      next: () => {
        this.imageAltText = '';
        this.selectedFile = null;
        this.setSuccess('Image uploaded.');
        this.loadProduct(this.productId);
      },
      error: (error) => this.setError(formatBusinessError(error, 'Upload image failed.')),
    });
  }

  protected moveImageUp(imageId: string): void {
    const index = this.images.findIndex((x) => x.id === imageId);
    if (index <= 0) {
      return;
    }

    const reordered = [...this.images];
    const temp = reordered[index - 1];
    reordered[index - 1] = reordered[index];
    reordered[index] = temp;
    const ids = reordered.map((x) => x.id);
    this.api.reorderProductImages(this.productId, ids).subscribe({
      next: () => {
        this.setSuccess('Image order updated.');
        this.loadProduct(this.productId);
      },
      error: (error) => this.setError(formatBusinessError(error, 'Reorder failed.')),
    });
  }

  protected deleteImage(imageId: string): void {
    this.api.deleteProductImage(this.productId, imageId).subscribe({
      next: () => {
        this.setSuccess('Image deleted.');
        this.loadProduct(this.productId);
      },
      error: (error) => this.setError(formatBusinessError(error, 'Delete image failed.')),
    });
  }

  private loadProduct(id: string): void {
    this.api.getAdminProductById(id).subscribe({
      next: (product) => {
        if (!product) {
          return;
        }

        this.name = product.name;
        this.slug = product.slug;
        this.shortDescription = product.shortDescription;
        this.description = product.description;
        this.categoryId = product.categoryId;
        this.studioId = product.studioId;
        this.sku = product.sku;
        this.priceAmount = product.priceAmount;
        this.currency = product.currency;
        this.status = product.status;
        this.isNew = product.isNew;
        this.isFeatured = product.isFeatured;
        this.scale = product.scale;
        this.materials = product.materials;
        this.dimensionsText = product.dimensionsText;
        this.editionSize = product.editionSize;
        this.estimatedReleaseText = product.estimatedReleaseText;
        this.specifications = product.specifications.map((x) => ({ label: x.label, value: x.value }));
        this.images = product.images;
      },
      error: () => this.setError('Load product failed.'),
    });
  }

  private setSuccess(message: string): void {
    this.message = message;
    this.toast.success(message);
  }

  private setError(message: string): void {
    this.message = message;
    this.toast.error(message);
  }
}
