import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, REQUEST, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';

interface AuthUserResponse {
  id: string;
  email: string;
  roles: string[];
}

interface AuthResponse {
  accessToken: string;
  expiresAtUtc: string;
  user: AuthUserResponse;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly request = inject(REQUEST, { optional: true });
  private readonly authBaseUrl = this.resolveBaseUrl('/api/auth');
  private refreshInFlight: Promise<{ error?: string }> | null = null;

  readonly accessToken = signal<string>('');
  readonly userEmail = signal<string>('');
  readonly isAuthenticated = signal(false);

  constructor() {
    void this.refreshSession();
  }

  async signIn(email: string, password: string): Promise<{ error?: string }> {
    try {
      const response = await firstValueFrom(
        this.http.post<AuthResponse>(
          `${this.authBaseUrl}/login`,
          { email, password },
          { withCredentials: true },
        ),
      );
      this.applyAuthResponse(response);
      return {};
    } catch (error) {
      this.clearSession();
      return { error: this.toErrorMessage(error) };
    }
  }

  async signUp(_email: string, _password: string): Promise<{ error?: string }> {
    return { error: 'Registration is not enabled for this admin workspace.' };
  }

  async refreshSession(): Promise<{ error?: string }> {
    if (this.refreshInFlight) {
      return this.refreshInFlight;
    }

    this.refreshInFlight = this.doRefreshSession();
    try {
      return await this.refreshInFlight;
    } finally {
      this.refreshInFlight = null;
    }
  }

  async signOut(): Promise<void> {
    try {
      await firstValueFrom(
        this.http.post<void>(`${this.authBaseUrl}/logout`, {}, { withCredentials: true }),
      );
    } finally {
      this.clearSession();
    }
  }

  clearSession(): void {
    this.accessToken.set('');
    this.userEmail.set('');
    this.isAuthenticated.set(false);
  }

  private async doRefreshSession(): Promise<{ error?: string }> {
    try {
      const response = await firstValueFrom(
        this.http.post<AuthResponse>(`${this.authBaseUrl}/refresh`, {}, { withCredentials: true }),
      );
      this.applyAuthResponse(response);
      return {};
    } catch (error) {
      this.clearSession();
      return { error: this.toErrorMessage(error) };
    }
  }

  private applyAuthResponse(response: AuthResponse): void {
    this.accessToken.set(response.accessToken);
    this.userEmail.set(response.user.email);
    this.isAuthenticated.set(true);
  }

  private resolveBaseUrl(path: string): string {
    const requestUrl = this.request?.url;
    return requestUrl ? new URL(path, requestUrl).toString() : path;
  }

  private toErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse && error.status === 401) {
      return 'Invalid email or password.';
    }

    if (error instanceof HttpErrorResponse && typeof error.error?.message === 'string') {
      return error.error.message;
    }

    return 'Authentication request failed.';
  }
}
