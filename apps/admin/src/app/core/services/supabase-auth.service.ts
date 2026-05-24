import { Injectable, PLATFORM_ID, inject, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { AuthChangeEvent, Session, SupabaseClient, createClient } from '@supabase/supabase-js';

@Injectable({ providedIn: 'root' })
export class SupabaseAuthService {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly isBrowser = isPlatformBrowser(this.platformId);
  private readonly client: SupabaseClient | null;

  readonly accessToken = signal<string>('');
  readonly userEmail = signal<string>('');
  readonly isAuthenticated = signal(false);

  constructor() {
    this.client = this.buildClient();
    if (!this.client) {
      return;
    }

    this.client.auth.getSession().then(({ data }) => this.applySession(data.session));
    this.client.auth.onAuthStateChange((_event: AuthChangeEvent, session: Session | null) => {
      this.applySession(session);
    });
  }

  async signIn(email: string, password: string): Promise<{ error?: string }> {
    if (!this.client) {
      return { error: 'Supabase client is not configured.' };
    }

    const result = await this.client.auth.signInWithPassword({ email, password });
    if (result.error) {
      return { error: result.error.message };
    }

    this.applySession(result.data.session);
    return {};
  }

  async signUp(email: string, password: string): Promise<{ error?: string }> {
    if (!this.client) {
      return { error: 'Supabase client is not configured.' };
    }

    const result = await this.client.auth.signUp({ email, password });
    if (result.error) {
      return { error: result.error.message };
    }

    this.applySession(result.data.session);
    return {};
  }

  async signOut(): Promise<void> {
    if (!this.client) {
      this.clearSession();
      return;
    }

    await this.client.auth.signOut();
    this.clearSession();
  }

  private buildClient(): SupabaseClient | null {
    if (!this.isBrowser) {
      return null;
    }

    const url = window.__supabaseUrl ?? '';
    const anonKey = window.__supabaseAnonKey ?? '';
    if (!url || !anonKey) {
      return null;
    }

    return createClient(url, anonKey);
  }

  private applySession(session: Session | null): void {
    if (!session?.access_token) {
      this.clearSession();
      return;
    }

    this.accessToken.set(session.access_token);
    this.userEmail.set(session.user?.email ?? '');
    this.isAuthenticated.set(true);
  }

  private clearSession(): void {
    this.accessToken.set('');
    this.userEmail.set('');
    this.isAuthenticated.set(false);
  }
}
