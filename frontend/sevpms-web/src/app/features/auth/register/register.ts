import { Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { httpErrorMessage } from '../../../core/utils/http-error';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: '../login/login.scss',
})
export class RegisterComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly submitting = signal(false);
  readonly error = signal('');
  readonly showPassword = signal(false);

  readonly form = new FormGroup({
    firstName: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    lastName: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    email: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.email] }),
    password: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.minLength(8)] }),
    phoneNumber: new FormControl<string | null>(null),
    role: new FormControl<0 | 1 | 2>(0, { nonNullable: true }),
  });

  togglePassword(): void {
    this.showPassword.update((value) => !value);
  }

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.submitting()) return;

    this.submitting.set(true);
    this.error.set('');

    this.auth.register(this.form.getRawValue()).subscribe({
      next: (result) => {
        this.submitting.set(false);
        void this.router.navigate(['/verify-email'], {
          queryParams: {
            email: result.email || this.form.controls.email.value.trim(),
            expires: result.otpExpiresAtUtc || undefined,
          },
        });
      },
      error: (error) => {
        this.error.set(httpErrorMessage(error, 'Registration failed.'));
        this.submitting.set(false);
      },
    });
  }
}
