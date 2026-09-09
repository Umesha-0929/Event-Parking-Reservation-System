import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, LoginRequest, RegisterRequest, EmailVerificationResponse, RegistrationPendingResponse, ResendEmailOtpRequest, VerifyEmailOtpRequest } from '../models/api.models';
import { SessionService } from './session.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly session = inject(SessionService);
  private readonly api = environment.apiBaseUrl.replace(/\/$/, '');

  login(body: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.api}/auth/login`, body, { withCredentials: true })
      .pipe(tap((result) => this.session.saveAuth(result)));
  }
  register(body: RegisterRequest): Observable<RegistrationPendingResponse> {
    return this.http.post<RegistrationPendingResponse>(`${this.api}/auth/register`, body, { withCredentials: true });
  }
  verifyEmailOtp(body: VerifyEmailOtpRequest): Observable<EmailVerificationResponse> {
    return this.http.post<EmailVerificationResponse>(`${this.api}/auth/verify-email-otp`, body, { withCredentials: true });
  }
  resendEmailOtp(body: ResendEmailOtpRequest): Observable<RegistrationPendingResponse> {
    return this.http.post<RegistrationPendingResponse>(`${this.api}/auth/resend-email-otp`, body, { withCredentials: true });
  }
  refresh(): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.api}/auth/refresh`, {}, { withCredentials: true })
      .pipe(tap((result) => this.session.saveAuth(result)));
  }
  logout(): Observable<unknown> {
    return this.http.post(`${this.api}/auth/logout`, {}, { withCredentials: true })
      .pipe(tap(() => this.session.clear()));
  }
  requestPasswordReset(email: string): Observable<unknown> {
    return this.http.post(`${this.api}/auth/password-reset/request`, { email });
  }
}
