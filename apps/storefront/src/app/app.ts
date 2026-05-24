import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  template: `
    <div class="site-shell">
      <header class="site-header">
        <div class="shell-inner site-header__inner">
          <a class="brand" routerLink="/">
            <span class="brand__mark">Archive</span>
            <span class="brand__divider"></span>
            <span class="brand__sub">Collectibles</span>
          </a>

          <nav class="site-nav" aria-label="Primary">
            <a routerLink="/" [routerLinkActive]="['is-active']" [routerLinkActiveOptions]="{ exact: true }">
              New Drops
            </a>
            <a routerLink="/collections/dc-batman" routerLinkActive="is-active">Batman / DC</a>
            <a routerLink="/collections/anime" routerLinkActive="is-active">Anime</a>
            <a routerLink="/collections/dc-batman" routerLinkActive="is-active">All Figures</a>
          </nav>

          <div class="site-actions" aria-hidden="true">
            <span>Search</span>
            <span>Cart</span>
          </div>
        </div>
      </header>

      <router-outlet />

      <footer class="site-footer">
        <div class="shell-inner site-footer__grid">
          <section>
            <h2>Archive</h2>
            <p>
              Curated display collectibles for collectors who care about sculpt, scale, and
              presentation quality.
            </p>
          </section>

          <section>
            <h3>Customer Care</h3>
            <a routerLink="/collections/dc-batman">Shipping & Returns</a>
            <a routerLink="/collections/anime">Authenticity Guarantee</a>
            <a routerLink="/products/dark-knight">Pre-order Policy</a>
          </section>

          <section>
            <h3>Newsletter</h3>
            <p>Join for limited drop notifications and allocation updates.</p>
            <div class="footer-input">
              <span>Email address</span>
              <span>&rarr;</span>
            </div>
          </section>
        </div>
      </footer>
    </div>
  `,
})
export class App {}
