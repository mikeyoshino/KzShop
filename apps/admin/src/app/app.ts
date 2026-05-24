import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  template: `
    <div class="admin-shell">
      <aside class="admin-sidebar">
        <a class="admin-brand" routerLink="/dashboard">
          <span class="admin-brand__mark">Archive</span>
          <span class="admin-brand__sub">Ops Console</span>
        </a>

        <nav class="admin-nav" aria-label="Admin sections">
          <a routerLink="/dashboard" routerLinkActive="is-active">Dashboard</a>
          <a routerLink="/products" routerLinkActive="is-active">Products</a>
        </nav>

        <section class="admin-sidebar__status" aria-label="Workspace status">
          <p>Phase 1</p>
          <strong>Catalog foundation</strong>
          <span>Studios, products, and inventory controls next.</span>
        </section>
      </aside>

      <div class="admin-main">
        <header class="admin-header">
          <div>
            <p class="eyebrow">ToyShops administration</p>
            <h1>Operations workspace</h1>
          </div>

          <div class="admin-header__meta">
            <span>Supabase ready</span>
            <span>Stripe planned</span>
          </div>
        </header>

        <main class="admin-content">
          <router-outlet />
        </main>
      </div>
    </div>
  `,
})
export class App {}
