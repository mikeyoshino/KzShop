import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { SupabaseAuthService } from '../../core/services/supabase-auth.service';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './login.page.html',
  styleUrl: './login.page.css',
})
export class LoginPageComponent {
  private readonly auth = inject(SupabaseAuthService);
  private readonly router = inject(Router);

  protected email = '';
  protected password = '';
  protected error = '';

  protected async submit(): Promise<void> {
    this.error = '';
    const result = await this.auth.signIn(this.email.trim(), this.password);
    if (result.error) {
      this.error = result.error;
      return;
    }

    await this.router.navigate(['/dashboard']);
  }
}
