import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { SessionService } from '../../../core/services/session.service';
import { SeoService } from '../../../core/services/seo.service';
import { httpErrorMessage } from '../../../core/utils/http-error';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class LoginComponent implements OnInit {
  private readonly seo = inject(SeoService);
  private readonly auth = inject(AuthService);
  private readonly session = inject(SessionService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly submitting = signal(false);
  readonly error = signal('');
  readonly showPassword = signal(false);

  readonly form = new FormGroup({
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    password: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(8)],
    }),
  });

  ngOnInit(): void {
    this.seo.setPage('Login | Nvent', 'Sign in to your Nvent account.');
  }

  togglePassword(): void {
    this.showPassword.update((value) => !value);
  }


  private canReturnTo(url: string): boolean {
    const role = this.session.role();
    if (!url.startsWith('/')) return false;
    if (url.startsWith('/customer/')) return role === 'Customer';
    if (url.startsWith('/organizer/')) return role === 'EventOrganizer';
    if (url.startsWith('/venue-owner/')) return role === 'VenueOwner';
    if (url.startsWith('/admin/')) return role === 'Admin';
    return true;
  }

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.submitting()) return;

    this.submitting.set(true);
    this.error.set('');

    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => {
        this.submitting.set(false);
        const requested = this.route.snapshot.queryParamMap.get('returnUrl');
        const target = requested && this.canReturnTo(requested) ? requested : this.session.routeForRole();
        void this.router.navigateByUrl(target);
      },
      error: (error) => {
        this.error.set(httpErrorMessage(error, 'Sign in failed. Check your credentials and try again.'));
        this.submitting.set(false);
      },
    });
  }
}
