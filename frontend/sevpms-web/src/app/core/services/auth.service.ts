import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, LoginRequest, RegisterRequest } from '../models/api.models';
import { SessionService } from './session.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly session = inject(SessionService);
  private readonly api = environment.apiBaseUrl.replace(/\/$/, '');

  login(body: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.api}/auth/login`, body).pipe(tap((result) => this.session.saveAuth(result)));
  }
  register(body: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.api}/auth/register`, body).pipe(tap((result) => this.session.saveAuth(result)));
  }
  refresh(refreshToken: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.api}/auth/refresh`, { refreshToken }).pipe(tap((result) => this.session.saveAuth(result)));
  }
  logout(): Observable<unknown> {
    const refreshToken = this.session.refreshToken();
    return this.http.post(`${this.api}/auth/logout`, { refreshToken }).pipe(tap(() => this.session.clear()));
  }
  requestPasswordReset(email: string): Observable<unknown> {
    return this.http.post(`${this.api}/auth/password-reset/request`, { email });
  }
}
