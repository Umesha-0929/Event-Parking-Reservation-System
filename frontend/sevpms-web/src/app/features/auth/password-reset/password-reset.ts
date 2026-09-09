import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BackendCoverageApiService } from '../../../core/services/backend-coverage-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';

@Component({
  selector: 'app-password-reset',
  imports: [FormsModule, RouterLink],
  templateUrl: './password-reset.html',
  styleUrl: './password-reset.scss',
})
export class PasswordResetComponent {
  private readonly api = inject(BackendCoverageApiService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly token = this.route.snapshot.queryParamMap.get('token')?.trim() ?? '';
  readonly mode = signal<'request' | 'confirm'>(this.token ? 'confirm' : 'request');
  readonly busy = signal(false);
  readonly message = signal('');
  readonly error = signal('');

  email = '';
  newPassword = '';
  confirmPassword = '';

  request(): void {
    const email = this.email.trim();
    if (!email || !email.includes('@')) { this.error.set('Enter a valid email address.'); return; }
    this.error.set(''); this.message.set(''); this.busy.set(true);
    this.api.requestPasswordReset(email).subscribe({
      next: () => {
        this.busy.set(false);
        this.message.set('If the account exists, a secure password-reset link has been sent to that email address.');
      },
      error: (e) => { this.error.set(httpErrorMessage(e, 'Password reset could not be requested.')); this.busy.set(false); },
    });
  }

  confirm(): void {
    if (!this.token) { this.error.set('This reset link is missing its security token. Request a new link.'); return; }
    if (this.newPassword.length < 8) { this.error.set('New password must be at least 8 characters.'); return; }
    if (this.newPassword !== this.confirmPassword) { this.error.set('The two passwords do not match.'); return; }

    this.error.set(''); this.message.set(''); this.busy.set(true);
    this.api.confirmPasswordReset(this.token, this.newPassword).subscribe({
      next: () => {
        this.busy.set(false);
        this.message.set('Password changed successfully. You can sign in with the new password.');
        setTimeout(() => void this.router.navigate(['/login']), 900);
      },
      error: (e) => { this.error.set(httpErrorMessage(e, 'The password could not be reset. The link may have expired.')); this.busy.set(false); },
    });
  }
}
