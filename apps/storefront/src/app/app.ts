import { Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { CartService } from './core/services/cart.service';
import { LanguageCode, LocalizationService } from './core/services/localization.service';

@Component({
  selector: 'app-root',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected readonly cart = inject(CartService);
  protected readonly i18n = inject(LocalizationService);
  protected readonly routeFadeActive = signal(true);
  protected readonly languageMenuOpen = signal(false);

  protected toggleLanguageMenu(): void {
    this.languageMenuOpen.update((isOpen) => !isOpen);
  }

  protected setLanguage(language: LanguageCode): void {
    this.i18n.setLanguage(language);
    this.languageMenuOpen.set(false);
  }

  onRouteActivate(): void {
    window.scrollTo(0, 0);
    this.routeFadeActive.set(false);

    setTimeout(() => {
      this.routeFadeActive.set(true);
    });
  }
}
