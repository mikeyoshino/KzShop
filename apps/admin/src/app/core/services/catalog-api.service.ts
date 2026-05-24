import { HttpClient } from '@angular/common/http';
import { Injectable, REQUEST, inject } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import {
  AdminProductDetail,
  CategorySummary,
  CreateProductSpecificationInput,
  PagedProductsResponse,
  ProductDetail,
  ProductStatus,
  SearchStudiosResponse,
  StudioSummary,
  UpsertProductInput,
} from '../models/catalog.models';

@Injectable({ providedIn: 'root' })
export class CatalogApiService {
  private readonly http = inject(HttpClient);
  private readonly request = inject(REQUEST, { optional: true });
  private readonly storefrontBaseUrl = this.resolveBaseUrl('/api/storefront');
  private readonly adminBaseUrl = this.resolveBaseUrl('/api/admin');

  getProducts(): Observable<PagedProductsResponse> {
    return this.http.get<PagedProductsResponse>(`${this.storefrontBaseUrl}/products`).pipe(
      catchError(() => of({ items: [] })),
    );
  }

  getProductBySlug(slug: string): Observable<ProductDetail | null> {
    return this.http
      .get<ProductDetail>(`${this.storefrontBaseUrl}/products/${encodeURIComponent(slug)}`)
      .pipe(catchError(() => of(null)));
  }

  getAdminProducts(): Observable<PagedProductsResponse> {
    return this.http.get<PagedProductsResponse>(`${this.adminBaseUrl}/products`).pipe(
      catchError(() => of({ items: [] })),
    );
  }

  getAdminProductById(id: string): Observable<AdminProductDetail | null> {
    return this.http
      .get<AdminProductDetail>(`${this.adminBaseUrl}/products/${encodeURIComponent(id)}`)
      .pipe(catchError(() => of(null)));
  }

  createStudio(input: { name: string; slug: string; isActive: boolean }): Observable<StudioSummary> {
    return this.http.post<StudioSummary>(`${this.adminBaseUrl}/studios`, input);
  }

  searchStudios(search: string): Observable<SearchStudiosResponse> {
    const query = search.trim();
    const url = query.length > 0
      ? `${this.adminBaseUrl}/studios?search=${encodeURIComponent(query)}`
      : `${this.adminBaseUrl}/studios`;
    return this.http.get<SearchStudiosResponse>(url).pipe(catchError(() => of({ items: [] })));
  }

  createCategory(input: {
    name: string;
    slug: string;
    description: string;
    isActive: boolean;
  }): Observable<CategorySummary> {
    return this.http.post<CategorySummary>(`${this.adminBaseUrl}/categories`, input);
  }

  updateCategory(
    id: string,
    input: { name: string; slug: string; description: string; isActive: boolean },
  ): Observable<CategorySummary> {
    return this.http.put<CategorySummary>(`${this.adminBaseUrl}/categories/${encodeURIComponent(id)}`, input);
  }

  getCategories(search = ''): Observable<{ items: CategorySummary[] }> {
    const query = search.trim();
    const url = query.length > 0
      ? `${this.adminBaseUrl}/categories?search=${encodeURIComponent(query)}`
      : `${this.adminBaseUrl}/categories`;
    return this.http.get<{ items: CategorySummary[] }>(url).pipe(catchError(() => of({ items: [] })));
  }

  createProduct(input: UpsertProductInput): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.adminBaseUrl}/products`, input);
  }

  updateProduct(id: string, input: UpsertProductInput): Observable<{ id: string }> {
    return this.http.put<{ id: string }>(`${this.adminBaseUrl}/products/${encodeURIComponent(id)}`, input);
  }

  archiveProduct(id: string): Observable<{ id: string; status: ProductStatus }> {
    return this.http.post<{ id: string; status: ProductStatus }>(
      `${this.adminBaseUrl}/products/${encodeURIComponent(id)}/archive`,
      {},
    );
  }

  adjustStock(productId: string, quantityDelta: number): Observable<{
    productId: string;
    onHandQuantity: number;
    reservedQuantity: number;
    availableQuantity: number;
  }> {
    return this.http.post<{
      productId: string;
      onHandQuantity: number;
      reservedQuantity: number;
      availableQuantity: number;
    }>(`${this.adminBaseUrl}/inventory/adjust`, { productId, quantityDelta });
  }

  uploadProductImage(productId: string, file: File, altText: string): Observable<{
    id: string;
    publicUrl: string;
    altText: string;
    sortOrder: number;
  }> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('altText', altText);
    return this.http.post<{
      id: string;
      publicUrl: string;
      altText: string;
      sortOrder: number;
    }>(`${this.adminBaseUrl}/products/${encodeURIComponent(productId)}/images`, formData);
  }

  reorderProductImages(productId: string, orderedImageIds: string[]): Observable<{ imageIds: string[] }> {
    return this.http.put<{ imageIds: string[] }>(
      `${this.adminBaseUrl}/products/${encodeURIComponent(productId)}/images/reorder`,
      { orderedImageIds },
    );
  }

  deleteProductImage(productId: string, productImageId: string): Observable<unknown> {
    return this.http.delete(`${this.adminBaseUrl}/products/${encodeURIComponent(productId)}/images/${encodeURIComponent(productImageId)}`);
  }

  toSpecificationInputs(rows: Array<{ label: string; value: string }>): CreateProductSpecificationInput[] {
    return rows
      .map((row, sortOrder) => ({ label: row.label.trim(), value: row.value.trim(), sortOrder }))
      .filter((row) => row.label.length > 0 && row.value.length > 0);
  }

  private resolveBaseUrl(path: string): string {
    const requestUrl = this.request?.url;
    return requestUrl ? new URL(path, requestUrl).toString() : path;
  }
}
