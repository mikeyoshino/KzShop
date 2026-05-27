import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './login.page.html',
  styleUrl: './login.page.css',
})
export class LoginPageComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  protected email = '';
  protected password = '';
  protected error = '';
  protected isSubmitting = false;

  protected async submit(): Promise<void> {
    if (this.isSubmitting) {
      return;
    }

    this.error = '';
    this.isSubmitting = true;
    try {
      const result = await this.auth.signIn(this.email.trim(), this.password);
      if (result.error) {
        this.error = result.error;
        this.toast.error(result.error);
        return;
      }

      this.toast.success('Signed in.');
      await this.router.navigate(['/dashboard']);
    } finally {
      this.isSubmitting = false;
    }
  }
}
