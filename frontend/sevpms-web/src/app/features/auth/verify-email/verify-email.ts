import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { SeoService } from '../../../core/services/seo.service';
import { httpErrorMessage } from '../../../core/utils/http-error';

@Component({
  selector: 'app-verify-email',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './verify-email.html',
  styleUrl: './verify-email.scss',
})
export class VerifyEmailComponent implements OnInit, OnDestroy {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly seo = inject(SeoService);

  readonly verifying = signal(false);
  readonly resending = signal(false);
  readonly error = signal('');
  readonly notice = signal('');
  readonly resendSeconds = signal(0);
  readonly expirySeconds = signal(0);

  readonly email = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.email],
  });

  readonly otp = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.pattern(/^\d{6}$/)],
  });

  private resendTimer: ReturnType<typeof setInterval> | null = null;
  private expiryTimer: ReturnType<typeof setInterval> | null = null;
  private expiresAtMs = 0;

  ngOnInit(): void {
    this.seo.setPage('Verify Email | Nvent', 'Verify your Nvent email address.');

    const email = this.route.snapshot.queryParamMap.get('email') ?? '';
    const expires = this.route.snapshot.queryParamMap.get('expires') ?? '';

    this.email.setValue(email);

    if (expires) {
      const parsed = Date.parse(expires);
      if (Number.isFinite(parsed)) {
        this.expiresAtMs = parsed;
        this.startExpiryCountdown();
      }
    }

    if (email) this.startResendCooldown(60);
  }

  ngOnDestroy(): void {
    if (this.resendTimer) clearInterval(this.resendTimer);
    if (this.expiryTimer) clearInterval(this.expiryTimer);
  }

  cleanOtp(): void {
    const value = this.otp.value.replace(/\D/g, '').slice(0, 6);
    if (value !== this.otp.value) this.otp.setValue(value, { emitEvent: false });
  }

  verify(): void {
    this.email.markAsTouched();
    this.otp.markAsTouched();
    this.cleanOtp();

    if (this.email.invalid || this.otp.invalid || this.verifying()) return;

    this.verifying.set(true);
    this.error.set('');
    this.notice.set('');

    this.auth.verifyEmailOtp({
      email: this.email.value.trim(),
      otp: this.otp.value.trim(),
    }).subscribe({
      next: (result) => {
        this.verifying.set(false);
        this.notice.set(result.message || 'Email verified successfully.');
        void this.router.navigate(['/login'], {
          queryParams: {
            verified: 1,
            email: this.email.value.trim(),
          },
        });
      },
      error: (error) => {
        this.error.set(httpErrorMessage(error, 'The verification code could not be verified.'));
        this.verifying.set(false);
      },
    });
  }

  resend(): void {
    this.email.markAsTouched();
    if (this.email.invalid || this.resending() || this.resendSeconds() > 0) return;

    this.resending.set(true);
    this.error.set('');
    this.notice.set('');

    this.auth.resendEmailOtp({ email: this.email.value.trim() }).subscribe({
      next: (result) => {
        this.resending.set(false);
        this.notice.set(result.message || 'A new verification code has been sent.');
        const parsed = Date.parse(result.otpExpiresAtUtc);
        if (Number.isFinite(parsed)) {
          this.expiresAtMs = parsed;
          this.startExpiryCountdown();
        }
        this.startResendCooldown(60);
      },
      error: (error) => {
        this.error.set(httpErrorMessage(error, 'A new verification code could not be sent.'));
        this.resending.set(false);
      },
    });
  }

  expiryLabel(): string {
    const value = this.expirySeconds();
    if (value <= 0) return 'Code expired';
    const minutes = Math.floor(value / 60);
    const seconds = value % 60;
    return `${minutes}:${seconds.toString().padStart(2, '0')}`;
  }

  private startResendCooldown(seconds: number): void {
    if (this.resendTimer) clearInterval(this.resendTimer);
    this.resendSeconds.set(seconds);

    this.resendTimer = setInterval(() => {
      const next = Math.max(0, this.resendSeconds() - 1);
      this.resendSeconds.set(next);
      if (next === 0 && this.resendTimer) {
        clearInterval(this.resendTimer);
        this.resendTimer = null;
      }
    }, 1000);
  }

  private startExpiryCountdown(): void {
    if (this.expiryTimer) clearInterval(this.expiryTimer);

    const update = () => {
      if (!this.expiresAtMs) {
        this.expirySeconds.set(0);
        return;
      }

      const seconds = Math.max(0, Math.ceil((this.expiresAtMs - Date.now()) / 1000));
      this.expirySeconds.set(seconds);

      if (seconds === 0 && this.expiryTimer) {
        clearInterval(this.expiryTimer);
        this.expiryTimer = null;
      }
    };

    update();
    this.expiryTimer = setInterval(update, 1000);
  }
}
