import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, REQUEST, inject } from '@angular/core';
import { Observable, of, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import {
  BusinessError,
  BusinessErrorCode,
  AdminDashboardMetricsResponse,
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

  getAdminDashboardMetrics(): Observable<AdminDashboardMetricsResponse> {
    return this.http.get<AdminDashboardMetricsResponse>(`${this.adminBaseUrl}/dashboard/metrics`);
  }

  createStudio(input: { name: string; slug: string; isActive: boolean }): Observable<StudioSummary> {
    return this.http
      .post<StudioSummary>(`${this.adminBaseUrl}/studios`, input)
      .pipe(catchError((error) => this.rethrowAdminError(error)));
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
    return this.http
      .post<CategorySummary>(`${this.adminBaseUrl}/categories`, input)
      .pipe(catchError((error) => this.rethrowAdminError(error)));
  }

  updateCategory(
    id: string,
    input: { name: string; slug: string; description: string; isActive: boolean },
  ): Observable<CategorySummary> {
    return this.http
      .put<CategorySummary>(`${this.adminBaseUrl}/categories/${encodeURIComponent(id)}`, input)
      .pipe(catchError((error) => this.rethrowAdminError(error)));
  }

  getCategories(search = ''): Observable<{ items: CategorySummary[] }> {
    const query = search.trim();
    const url = query.length > 0
      ? `${this.adminBaseUrl}/categories?search=${encodeURIComponent(query)}`
      : `${this.adminBaseUrl}/categories`;
    return this.http.get<{ items: CategorySummary[] }>(url).pipe(catchError(() => of({ items: [] })));
  }

  createProduct(input: UpsertProductInput): Observable<{ id: string }> {
    return this.http
      .post<{ id: string }>(`${this.adminBaseUrl}/products`, input)
      .pipe(catchError((error) => this.rethrowAdminError(error)));
  }

  updateProduct(id: string, input: UpsertProductInput): Observable<{ id: string }> {
    return this.http
      .put<{ id: string }>(`${this.adminBaseUrl}/products/${encodeURIComponent(id)}`, input)
      .pipe(catchError((error) => this.rethrowAdminError(error)));
  }

  archiveProduct(id: string): Observable<{ id: string; status: ProductStatus }> {
    return this.http
      .post<{ id: string; status: ProductStatus }>(
        `${this.adminBaseUrl}/products/${encodeURIComponent(id)}/archive`,
        {},
      )
      .pipe(catchError((error) => this.rethrowAdminError(error)));
  }

  adjustStock(productId: string, quantityDelta: number): Observable<{
    productId: string;
    onHandQuantity: number;
    reservedQuantity: number;
    availableQuantity: number;
  }> {
    return this.http
      .post<{
        productId: string;
        onHandQuantity: number;
        reservedQuantity: number;
        availableQuantity: number;
      }>(`${this.adminBaseUrl}/inventory/adjust`, { productId, quantityDelta })
      .pipe(catchError((error) => this.rethrowAdminError(error)));
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
    return this.http
      .post<{
        id: string;
        publicUrl: string;
        altText: string;
        sortOrder: number;
      }>(`${this.adminBaseUrl}/products/${encodeURIComponent(productId)}/images`, formData)
      .pipe(catchError((error) => this.rethrowAdminError(error)));
  }

  reorderProductImages(productId: string, orderedImageIds: string[]): Observable<{ imageIds: string[] }> {
    return this.http
      .put<{ imageIds: string[] }>(
        `${this.adminBaseUrl}/products/${encodeURIComponent(productId)}/images/reorder`,
        { orderedImageIds },
      )
      .pipe(catchError((error) => this.rethrowAdminError(error)));
  }

  deleteProductImage(productId: string, productImageId: string): Observable<unknown> {
    return this.http
      .delete(
        `${this.adminBaseUrl}/products/${encodeURIComponent(productId)}/images/${encodeURIComponent(productImageId)}`,
      )
      .pipe(catchError((error) => this.rethrowAdminError(error)));
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

  private rethrowAdminError(error: unknown): Observable<never> {
    return throwError(() => this.parseAdminError(error));
  }

  private parseAdminError(error: unknown): unknown {
    if (!(error instanceof HttpErrorResponse) || !error.error || typeof error.error !== 'object') {
      return error;
    }

    const payload = error.error as Partial<BusinessError>;
    if (typeof payload.code !== 'number' || typeof payload.message !== 'string') {
      return error;
    }

    if (!Object.values(BusinessErrorCode).includes(payload.code)) {
      return error;
    }

    return {
      code: payload.code as BusinessErrorCode,
      message: payload.message,
      status: error.status,
    } satisfies BusinessError;
  }
}
