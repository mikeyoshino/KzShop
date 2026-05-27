import { CurrencyPipe } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AdminDashboardMetricsResponse } from '../../core/models/catalog.models';
import { CatalogApiService } from '../../core/services/catalog-api.service';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [CurrencyPipe, RouterLink],
  template: `
    <section class="command-page">
      <div class="command-heading">
        <div>
          <h1>Command Center</h1>
          <p>System Overview & Live Metrics.</p>
        </div>
        <p class="command-sync">LAST SYNC: {{ lastSync }}</p>
      </div>

      <section class="command-stats">
        <article class="command-stat">
          <div>
            <span>Period Capital</span>
            <i class="fa-solid fa-vault" aria-hidden="true"></i>
          </div>
          <div>
            <strong>{{ periodCapital() | currency: 'USD' : 'symbol' : '1.0-0' }}</strong>
            <p class="is-positive">
              <i class="fa-solid fa-arrow-up" aria-hidden="true"></i>
              {{ recognizedRevenue() | currency: 'USD' : 'symbol' : '1.0-0' }} recognized
            </p>
          </div>
        </article>

        <article class="command-stat">
          <div>
            <span>Active Requisitions</span>
            <i class="fa-solid fa-file-invoice" aria-hidden="true"></i>
          </div>
          <div>
            <strong>{{ orderCount() }}</strong>
            <p>{{ recentOrders().length }} recent orders</p>
          </div>
        </article>

        <article class="command-stat">
          <div>
            <span>Waitlist Queue</span>
            <i class="fa-solid fa-users" aria-hidden="true"></i>
          </div>
          <div>
            <strong>{{ waitlistCount() }}</strong>
            <p>Across recent requisitions</p>
          </div>
        </article>

        <article class="command-stat command-stat--alert">
          <div>
            <span>Inventory Alerts</span>
            <i class="fa-solid fa-triangle-exclamation" aria-hidden="true"></i>
          </div>
          <div>
            <strong>{{ lowStockItems().length }}</strong>
            <p>Items below threshold</p>
          </div>
        </article>
      </section>

      <div class="command-grid">
        <section class="command-chart">
          <div class="command-panel__header">
            <h2>Acquisition Volume</h2>
            <select aria-label="Chart range">
              <option>Last 7 Days</option>
              <option>Last 30 Days</option>
            </select>
          </div>
          <div class="command-chart__plot bg-grid-pattern">
            @for (height of chartBars; track height) {
              <div class="command-chart__bar" [style.height.%]="height">
                <span>{{ height * 12 }}</span>
              </div>
            }
          </div>
          <div class="command-chart__days">
            <span>MON</span><span>TUE</span><span>WED</span><span>THU</span><span>FRI</span><span>SAT</span><span>SUN</span>
          </div>
        </section>

        <aside class="command-side">
          <section class="command-panel quick-directives">
            <div class="command-panel__header">
              <h2>Quick Directives</h2>
            </div>
            <div class="command-actions">
              <a routerLink="/products/new">
                <span><i class="fa-solid fa-plus" aria-hidden="true"></i> New Artifact</span>
                <i class="fa-solid fa-chevron-right" aria-hidden="true"></i>
              </a>
              <button type="button">
                <span><i class="fa-solid fa-bullhorn" aria-hidden="true"></i> Broadcast Drop</span>
                <i class="fa-solid fa-chevron-right" aria-hidden="true"></i>
              </button>
            </div>
          </section>

          <section class="command-panel critical-inventory">
            <div class="command-panel__header">
              <h2>Critical Inventory</h2>
            </div>
            <div class="critical-inventory__list">
              @for (item of lowStockItems(); track item.productId) {
                <div class="critical-item">
                  <img [src]="item.primaryImageUrl || fallbackInventoryImage" [alt]="item.productName" />
                  <div>
                    <strong>{{ item.productName }}</strong>
                    <p>ID: {{ item.productId.padStart(4, '0') }} | {{ item.studioName }}</p>
                  </div>
                  <span>{{ item.availableQuantity }}<small>Left</small></span>
                </div>
              } @empty {
                <p class="critical-empty">No critical inventory alerts.</p>
              }
            </div>
          </section>
        </aside>
      </div>

      <section class="command-panel requisitions-table">
        <div class="command-panel__header">
          <h2>Recent Requisitions</h2>
          <button type="button">View Log <i class="fa-solid fa-arrow-right" aria-hidden="true"></i></button>
        </div>
        <div class="command-table-scroll">
          <table>
            <thead>
              <tr>
                <th>Req ID</th>
                <th>Date</th>
                <th>Operative/Client</th>
                <th class="is-right">Total</th>
                <th class="is-right">Status</th>
              </tr>
            </thead>
            <tbody>
              @for (order of recentOrders(); track order.orderId) {
                <tr>
                  <td>{{ order.orderNumber }}</td>
                  <td>{{ formatOrderDate(order.createdAtUtc) }}</td>
                  <td>{{ order.customerName }}</td>
                  <td class="is-right">{{ order.totalAmount | currency: order.currency : 'symbol' : '1.0-0' }}</td>
                  <td class="is-right"><span class="status-label">{{ order.status }}</span></td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      </section>
    </section>
  `,
})
export class DashboardPageComponent {
  private readonly catalogApi = inject(CatalogApiService);
  private readonly fallbackMetrics: AdminDashboardMetricsResponse = {
    periodRevenue: 0,
    recognizedRevenue: 0,
    orderCount: 0,
    criticalInventoryItems: [],
    recentOrders: [],
  };

  protected readonly chartBars = [40, 25, 60, 45, 80, 55, 90];
  protected readonly fallbackInventoryImage = 'https://placehold.co/80x80/151a27/e2e8f0?text=N/A';
  protected readonly lastSync = new Date().toLocaleTimeString();
  protected readonly dashboardMetrics = toSignal(
    this.catalogApi.getAdminDashboardMetrics().pipe(catchError(() => of(this.fallbackMetrics))),
    { initialValue: this.fallbackMetrics },
  );

  protected readonly periodCapital = computed(() => this.dashboardMetrics().periodRevenue);
  protected readonly recognizedRevenue = computed(() => this.dashboardMetrics().recognizedRevenue);
  protected readonly orderCount = computed(() => this.dashboardMetrics().orderCount);
  protected readonly recentOrders = computed(() => this.dashboardMetrics().recentOrders);
  protected readonly lowStockItems = computed(() => this.dashboardMetrics().criticalInventoryItems);
  protected readonly waitlistCount = computed(() =>
    this.recentOrders().filter((order) => order.status.toLowerCase().includes('wait')).length,
  );

  protected formatOrderDate(createdAtUtc: string): string {
    const date = new Date(createdAtUtc);
    return Number.isNaN(date.getTime())
      ? createdAtUtc
      : date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
  }
}
