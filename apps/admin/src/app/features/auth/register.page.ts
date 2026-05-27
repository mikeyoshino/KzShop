import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-register-page',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './register.page.html',
  styleUrl: './register.page.css',
})
export class RegisterPageComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  protected email = '';
  protected password = '';
  protected error = '';

  protected async submit(): Promise<void> {
    this.error = '';
    const result = await this.auth.signUp(this.email.trim(), this.password);
    if (result.error) {
      this.error = result.error;
      this.toast.error(result.error);
      return;
    }

    this.toast.success('Signed in.');
    await this.router.navigate(['/dashboard']);
  }
}
