import { TestBed } from '@angular/core/testing';
import { ToastService } from './toast.service';

describe('ToastService', () => {
  let service: ToastService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ToastService);
  });

  it('adds success and error toasts to the app-wide stack', () => {
    service.success('Category created.');
    service.error('Create category failed.');

    expect(service.toasts()).toEqual([
      jasmine.objectContaining({ tone: 'success', message: 'Category created.' }),
      jasmine.objectContaining({ tone: 'error', message: 'Create category failed.' }),
    ]);
  });

  it('dismisses a toast by id', () => {
    service.success('Saved.');
    const id = service.toasts()[0].id;

    service.dismiss(id);

    expect(service.toasts()).toEqual([]);
  });
});
