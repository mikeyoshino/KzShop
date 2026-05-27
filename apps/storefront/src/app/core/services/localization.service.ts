import { Injectable, computed, signal } from '@angular/core';
import enTranslations from '../../../assets/i18n/en.json';
import thTranslations from '../../../assets/i18n/th.json';

export type LanguageCode = 'th' | 'en';
type TranslationNode = string | { [key: string]: TranslationNode };
type TranslationTree = { [key: string]: TranslationNode };

export interface LanguageOption {
  code: LanguageCode;
  label: string;
  shortLabel: string;
  flag: string;
}

const STORAGE_KEY = 'toyshops-language';

const translations: Record<LanguageCode, TranslationTree> = {
  th: thTranslations,
  en: enTranslations,
};

@Injectable({ providedIn: 'root' })
export class LocalizationService {
  readonly languages: LanguageOption[] = [
    { code: 'th', label: 'ไทย', shortLabel: 'TH', flag: 'Thailand' },
    { code: 'en', label: 'English', shortLabel: 'EN', flag: 'United States' },
  ];

  private readonly currentLanguage = signal<LanguageCode>(this.readInitialLanguage());

  readonly language = this.currentLanguage.asReadonly();
  readonly activeLanguage = computed(
    () => this.languages.find((language) => language.code === this.currentLanguage()) ?? this.languages[0],
  );

  setLanguage(language: LanguageCode): void {
    this.currentLanguage.set(language);

    try {
      localStorage.setItem(STORAGE_KEY, language);
    } catch {
      // Ignore storage failures in SSR/tests/private mode.
    }
  }

  t(key: string): string {
    return this.lookup(translations[this.currentLanguage()], key) ?? this.lookup(translations.en, key) ?? key;
  }

  private lookup(tree: TranslationTree, key: string): string | null {
    const value = key.split('.').reduce<TranslationNode | undefined>((current, part) => {
      if (!current || typeof current === 'string') {
        return undefined;
      }

      return current[part];
    }, tree);

    return typeof value === 'string' ? value : null;
  }

  private readInitialLanguage(): LanguageCode {
    try {
      const stored = localStorage.getItem(STORAGE_KEY);
      return stored === 'en' || stored === 'th' ? stored : 'th';
    } catch {
      return 'th';
    }
  }
}
