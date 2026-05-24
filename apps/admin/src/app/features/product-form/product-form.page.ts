import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductStatus, StudioSummary } from '../../core/models/catalog.models';
import { CatalogApiService } from '../../core/services/catalog-api.service';

@Component({
  selector: 'app-product-form-page',
  standalone: true,
  imports: [FormsModule],
  template: `
    <section class="page-stack">
      <section class="panel products-panel">
        <div>
          <p class="section-label">Product editor</p>
          <h2>{{ isCreateMode ? 'Create product' : 'Edit product' }}</h2>
        </div>

        <div class="form-grid form-grid--two">
          <input [(ngModel)]="name" placeholder="Name" />
          <input [(ngModel)]="slug" placeholder="Slug" />
          <input [(ngModel)]="sku" placeholder="SKU" />
          <input type="number" [(ngModel)]="priceAmount" placeholder="Price" />
          <input [(ngModel)]="currency" placeholder="Currency" />
          <select [(ngModel)]="status">
            @for (s of statuses; track s) {
              <option [value]="s">{{ s }}</option>
            }
          </select>
          <input [(ngModel)]="scale" placeholder="Scale (1/4 Scale)" />
          <input [(ngModel)]="materials" placeholder="Materials" />
          <input [(ngModel)]="dimensionsText" placeholder="Dimensions" />
          <input [(ngModel)]="editionSize" placeholder="Edition size" />
          <input [(ngModel)]="estimatedReleaseText" placeholder="Estimated release" />
          <input [(ngModel)]="categoryId" placeholder="Category Id" />
        </div>

        <textarea [(ngModel)]="shortDescription" rows="2" placeholder="Short description"></textarea>
        <textarea [(ngModel)]="description" rows="4" placeholder="Description"></textarea>

        <div class="toolbar">
          <input
            [(ngModel)]="studioSearch"
            (ngModelChange)="fetchStudios($event)"
            placeholder="Search studio"
          />
          <select [(ngModel)]="studioId">
            <option value="">Select studio</option>
            @for (studio of studios; track studio.id) {
              <option [value]="studio.id">{{ studio.name }}</option>
            }
          </select>
          <button type="button" (click)="createStudioQuick()">Add studio</button>
        </div>

        <div class="checkbox-row">
          <label class="checkbox-line"><input type="checkbox" [(ngModel)]="isNew" /> <span>New</span></label>
          <label class="checkbox-line"><input type="checkbox" [(ngModel)]="isFeatured" /> <span>Featured</span></label>
        </div>

        <div class="toolbar">
          <button class="button-primary" type="button" (click)="saveProduct()">
            {{ isCreateMode ? 'Create product' : 'Save product' }}
          </button>
          @if (!isCreateMode) {
            <button type="button" (click)="archiveProduct()">Archive</button>
          }
        </div>
        @if (message) {
          <p>{{ message }}</p>
        }
      </section>

      <section class="panel products-panel">
        <div>
          <p class="section-label">Specifications</p>
          <h2>Rows</h2>
        </div>
        @for (row of specifications; track $index) {
          <div class="form-grid form-grid--two">
            <input [(ngModel)]="row.label" placeholder="Label" />
            <input [(ngModel)]="row.value" placeholder="Value" />
          </div>
        }
        <button type="button" (click)="addSpecRow()">Add spec row</button>
      </section>

      @if (!isCreateMode && productId) {
        <section class="panel products-panel">
          <div>
            <p class="section-label">Inventory</p>
            <h2>Adjustment</h2>
          </div>
          <div class="toolbar">
            <input type="number" [(ngModel)]="quantityDelta" placeholder="Quantity delta" />
            <button type="button" (click)="adjustStock()">Apply</button>
          </div>
        </section>

        <section class="panel products-panel">
          <div>
            <p class="section-label">Images</p>
            <h2>Upload and order</h2>
          </div>
          <div class="toolbar">
            <input type="file" (change)="onFileSelected($event)" />
            <input [(ngModel)]="imageAltText" placeholder="Alt text" />
            <button type="button" (click)="uploadImage()">Upload</button>
          </div>
          @for (image of images; track image.id) {
            <div class="list-row">
              <div>
                <strong>#{{ image.sortOrder + 1 }} {{ image.altText }}</strong>
                <p>{{ image.publicUrl }}</p>
              </div>
              <button type="button" (click)="moveImageUp(image.id)">Up</button>
              <button type="button" (click)="deleteImage(image.id)">Delete</button>
            </div>
          } @empty {
            <div class="list-empty">No images.</div>
          }
        </section>
      }
    </section>
  `,
})
export class ProductFormPageComponent {
  private readonly api = inject(CatalogApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

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

  protected quantityDelta = 0;
  protected imageAltText = '';
  protected images: Array<{ id: string; publicUrl: string; altText: string; sortOrder: number }> = [];
  private selectedFile: File | null = null;

  constructor() {
    const routeId = this.route.snapshot.paramMap.get('id') ?? 'new';
    this.productId = routeId;
    this.isCreateMode = routeId === 'new';
    this.fetchStudios('');
    if (!this.isCreateMode) {
      this.loadProduct(routeId);
    }
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
        this.fetchStudios(this.studioSearch);
      },
      error: () => (this.message = 'Create studio failed.'),
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
        next: (created) => this.router.navigate(['/products', created.id]),
        error: () => (this.message = 'Create product failed.'),
      });
      return;
    }

    this.api.updateProduct(this.productId, input).subscribe({
      next: () => (this.message = 'Product updated.'),
      error: () => (this.message = 'Update product failed.'),
    });
  }

  protected archiveProduct(): void {
    this.api.archiveProduct(this.productId).subscribe({
      next: () => (this.message = 'Product archived.'),
      error: () => (this.message = 'Archive failed.'),
    });
  }

  protected adjustStock(): void {
    this.api.adjustStock(this.productId, Number(this.quantityDelta)).subscribe({
      next: () => (this.message = 'Inventory updated.'),
      error: () => (this.message = 'Inventory update failed.'),
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
        this.loadProduct(this.productId);
      },
      error: () => (this.message = 'Upload image failed.'),
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
      next: () => this.loadProduct(this.productId),
      error: () => (this.message = 'Reorder failed.'),
    });
  }

  protected deleteImage(imageId: string): void {
    this.api.deleteProductImage(this.productId, imageId).subscribe({
      next: () => this.loadProduct(this.productId),
      error: () => (this.message = 'Delete image failed.'),
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
      error: () => (this.message = 'Load product failed.'),
    });
  }
}
