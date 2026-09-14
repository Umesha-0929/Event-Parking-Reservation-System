import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, finalize, map, shareReplay, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthUser, LoginRequest, RegisterRequest, RegistrationPending, UserRole } from '../models/api.models';

const STORAGE_KEY = 'nvent_session';
@Injectable({ providedIn:'root' })
export class AuthService {
  private readonly http = inject(HttpClient); private readonly router = inject(Router);
  private readonly _session = signal<AuthUser|null>(this.restore());
  readonly session = this._session.asReadonly();
  readonly isAuthenticated = computed(() => !!this._session()?.accessToken);
  readonly role = computed<UserRole|null>(() => this._session()?.role ?? null);
  readonly displayName = computed(() => this._session() ? `${this._session()!.firstName} ${this._session()!.lastName}`.trim() : 'Guest');
  private refreshInFlight$:Observable<AuthUser>|null = null;

  login(body:LoginRequest) { return this.http.post<AuthUser>(`${environment.apiBaseUrl}/auth/login`, body, {withCredentials:true}).pipe(tap(v=>this.store(v))); }
  register(body:RegisterRequest) { return this.http.post<RegistrationPending>(`${environment.apiBaseUrl}/auth/register`, body, {withCredentials:true}); }
  verifyEmail(email:string, otp:string) { return this.http.post<{verified:boolean;message:string}>(`${environment.apiBaseUrl}/auth/verify-email-otp`, {email,otp}, {withCredentials:true}); }
  resendOtp(email:string) { return this.http.post<RegistrationPending>(`${environment.apiBaseUrl}/auth/resend-email-otp`, {email}, {withCredentials:true}); }
  requestPasswordReset(email:string) { return this.http.post<void>(`${environment.apiBaseUrl}/auth/password-reset/request`, {email}, {withCredentials:true}); }
  confirmPasswordReset(token:string,newPassword:string) { return this.http.post<void>(`${environment.apiBaseUrl}/auth/password-reset/confirm`, {token,newPassword}, {withCredentials:true}); }
  refresh():Observable<AuthUser> {
    if (this.refreshInFlight$) return this.refreshInFlight$;
    this.refreshInFlight$ = this.http.post<AuthUser>(`${environment.apiBaseUrl}/auth/refresh`, {}, {withCredentials:true}).pipe(
      tap(v=>this.store(v)), finalize(()=>this.refreshInFlight$=null), shareReplay({bufferSize:1,refCount:false})
    );
    return this.refreshInFlight$;
  }
  logout(allSessions=true) { return this.http.post<void>(`${environment.apiBaseUrl}/auth/logout`, {allSessions}, {withCredentials:true}).pipe(finalize(()=>{this.clear();void this.router.navigateByUrl('/auth/sign-in');})); }
  token() { return this._session()?.accessToken ?? null; }
  hasRole(...roles:UserRole[]) { const role=this.role(); return role !== null && roles.includes(role); }
  defaultRoute() { switch(this.role()){case 1:return '/organizer';case 2:return '/venue-owner';case 3:return '/admin';default:return '/app/home';} }
  postLoginRoute(returnUrl:string|null|undefined) {
    if (!returnUrl || !returnUrl.startsWith('/') || returnUrl.startsWith('//')) return this.defaultRoute();
    const path = returnUrl.split(/[?#]/,1)[0];
    const role=this.role();
    const allowed = role===1 ? this.inWorkspace(path,'/organizer')
      : role===2 ? this.inWorkspace(path,'/venue-owner')
      : role===3 ? this.inWorkspace(path,'/admin')
      : this.inWorkspace(path,'/app');
    return allowed ? returnUrl : this.defaultRoute();
  }
  clear() { this._session.set(null); localStorage.removeItem(STORAGE_KEY); }
  private inWorkspace(path:string,root:string) { return path===root || path.startsWith(root+'/'); }
  private store(value:AuthUser) { this._session.set(value); localStorage.setItem(STORAGE_KEY,JSON.stringify(value)); }
  private restore():AuthUser|null { try { const raw=localStorage.getItem(STORAGE_KEY); if(!raw)return null; const v=JSON.parse(raw) as AuthUser; return v?.accessToken ? v : null; } catch { return null; } }
}
