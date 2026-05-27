import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './core/services/auth.service';
import { ToastService } from './core/services/toast.service';

@Component({
  selector: 'app-root',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  template: `
    <div class="toast-stack" aria-live="polite" aria-atomic="true">
      @for (toast of toasts(); track toast.id) {
        <article class="toast toast--{{ toast.tone }}">
          <span class="toast__icon" aria-hidden="true">
            @if (toast.tone === 'success') {
              <i class="fa-solid fa-check"></i>
            } @else if (toast.tone === 'error') {
              <i class="fa-solid fa-triangle-exclamation"></i>
            } @else {
              <i class="fa-solid fa-circle-info"></i>
            }
          </span>
          <p>{{ toast.message }}</p>
          <button type="button" aria-label="Dismiss notification" (click)="dismissToast(toast.id)">
            <i class="fa-solid fa-xmark" aria-hidden="true"></i>
          </button>
        </article>
      }
    </div>

    @if (isAuthRoute()) {
      <main class="auth-app-shell">
        <router-outlet />
      </main>
    } @else {
      <div class="admin-shell admin-dashboard-shell">
        <aside class="admin-sidebar">
          <a class="admin-brand" routerLink="/dashboard" aria-label="Archive admin home">
            <span class="admin-brand__mark">Archive</span>
            <span class="admin-brand__badge">Admin</span>
          </a>

          <section class="admin-operative" aria-label="Current operative">
            <div class="admin-operative__icon">
              <i class="fa-solid fa-user-shield" aria-hidden="true"></i>
            </div>
            <div>
              <p>Operative</p>
              <strong>A. Smith</strong>
            </div>
          </section>

          <nav class="admin-nav" aria-label="Admin sections">
            <p>Modules</p>
            <a routerLink="/dashboard" routerLinkActive="is-active">
              <span><i class="fa-solid fa-chart-line" aria-hidden="true"></i> Command</span>
            </a>
            <a routerLink="/products" routerLinkActive="is-active">
              <span><i class="fa-solid fa-boxes-stacked" aria-hidden="true"></i> Inventory</span>
              <small class="admin-nav__alert"></small>
            </a>
            <a routerLink="/products/new" routerLinkActive="is-active">
              <span><i class="fa-solid fa-plus" aria-hidden="true"></i> New Artifact</span>
            </a>
            <a routerLink="/studios" routerLinkActive="is-active">
              <span><i class="fa-solid fa-building" aria-hidden="true"></i> Studios</span>
            </a>
            <a routerLink="/categories" routerLinkActive="is-active">
              <span><i class="fa-solid fa-layer-group" aria-hidden="true"></i> Categories</span>
            </a>
            <a class="is-disabled" aria-disabled="true">
              <span><i class="fa-solid fa-users" aria-hidden="true"></i> Clients</span>
            </a>
          </nav>

          <a class="admin-exit" routerLink="/login">
            <i class="fa-solid fa-arrow-right-from-bracket" aria-hidden="true"></i>
            <span>Exit to Store</span>
          </a>
        </aside>

        <div class="admin-main">
          <header class="admin-header">
            <div class="admin-mobile-brand">
              <button type="button" aria-label="Open menu"><i class="fa-solid fa-bars" aria-hidden="true"></i></button>
              <span>Archive Admin</span>
            </div>

            <div class="admin-breadcrumb">
              <span>Archive</span>
              <span>/</span>
              <strong>{{ currentSection() }}</strong>
            </div>

            <div class="admin-header__meta">
              <label class="admin-search">
                <i class="fa-solid fa-magnifying-glass" aria-hidden="true"></i>
                <input type="text" placeholder="SEARCH REGISTRY..." />
              </label>
              <button class="admin-bell" type="button" aria-label="Notifications">
                <i class="fa-regular fa-bell" aria-hidden="true"></i>
                <span></span>
              </button>
              @if (isAuthenticated()) {
                <button type="button" (click)="signOut()">Sign out</button>
              }
            </div>
          </header>

          <main class="admin-content">
            <router-outlet />
          </main>
        </div>
      </div>
    }
  `,
})
export class App {
  private readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  protected readonly isAuthenticated = computed(() => this.auth.isAuthenticated());
  protected readonly toasts = this.toast.toasts;

  protected isAuthRoute(): boolean {
    return this.router.url.startsWith('/login') || this.router.url.startsWith('/register');
  }

  protected currentSection(): string {
    if (this.router.url.startsWith('/products')) {
      return 'inventory';
    }
    if (this.router.url.startsWith('/studios')) {
      return 'studios';
    }
    if (this.router.url.startsWith('/categories')) {
      return 'categories';
    }

    return 'dashboard';
  }

  protected async signOut(): Promise<void> {
    await this.auth.signOut();
    await this.router.navigate(['/login']);
  }

  protected dismissToast(id: number): void {
    this.toast.dismiss(id);
  }
}
