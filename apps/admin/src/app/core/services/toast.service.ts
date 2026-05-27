import { Injectable, signal } from '@angular/core';

export type ToastTone = 'success' | 'error' | 'info';

export interface ToastMessage {
  id: number;
  tone: ToastTone;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private nextId = 1;
  private readonly messages = signal<ToastMessage[]>([]);

  readonly toasts = this.messages.asReadonly();

  success(message: string): void {
    this.add('success', message);
  }

  error(message: string): void {
    this.add('error', message);
  }

  info(message: string): void {
    this.add('info', message);
  }

  dismiss(id: number): void {
    this.messages.update((toasts) => toasts.filter((toast) => toast.id !== id));
  }

  private add(tone: ToastTone, message: string): void {
    const toast = { id: this.nextId++, tone, message };
    this.messages.update((toasts) => [...toasts, toast].slice(-4));

    window.setTimeout(() => this.dismiss(toast.id), 5000);
  }
}
