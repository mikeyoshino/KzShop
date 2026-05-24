import { HttpClient } from '@angular/common/http';
import { Injectable, REQUEST, inject } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import {
  PagedProductsResponse,
  ProductDetail,
} from '../models/catalog.models';

@Injectable({ providedIn: 'root' })
export class CatalogApiService {
  private readonly http = inject(HttpClient);
  private readonly request = inject(REQUEST, { optional: true });
  private readonly baseUrl = this.resolveBaseUrl();

  getProducts(): Observable<PagedProductsResponse> {
    return this.http.get<PagedProductsResponse>(`${this.baseUrl}/products`).pipe(
      catchError(() => of({ items: [] })),
    );
  }

  getProductBySlug(slug: string): Observable<ProductDetail | null> {
    return this.http
      .get<ProductDetail>(`${this.baseUrl}/products/${encodeURIComponent(slug)}`)
      .pipe(catchError(() => of(null)));
  }

  private resolveBaseUrl(): string {
    const requestUrl = this.request?.url;
    return requestUrl ? new URL('/api/storefront', requestUrl).toString() : '/api/storefront';
  }
}
