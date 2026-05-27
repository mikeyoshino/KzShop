import { TestBed } from '@angular/core/testing';
import { LocalizationService } from './localization.service';

describe('LocalizationService', () => {
  let service: LocalizationService;

  beforeEach(() => {
    localStorage.clear();
    TestBed.resetTestingModule();
    service = TestBed.inject(LocalizationService);
  });

  it('defaults to Thai', () => {
    expect(service.language()).toBe('th');
    expect(service.t('nav.newDrops')).toBe('สินค้าใหม่');
  });

  it('switches to English and persists the selected language', () => {
    service.setLanguage('en');

    expect(service.language()).toBe('en');
    expect(service.t('nav.newDrops')).toBe('New Drops');
    expect(localStorage.getItem('toyshops-language')).toBe('en');
  });
});
