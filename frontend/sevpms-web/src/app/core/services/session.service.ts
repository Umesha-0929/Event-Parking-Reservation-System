import { Injectable, computed, signal } from '@angular/core';
import { AuthResponse, AuthTokens, AuthUser, BackendRole, BackendRoleValue } from '../models/api.models';

const LEGACY_TOKEN_KEY = 'nvent.accessToken';
const LEGACY_REFRESH_KEY = 'nvent.refreshToken';
const USER_KEY = 'nvent.user';

@Injectable({ providedIn: 'root' })
export class SessionService {
  // Access tokens live only in memory. Refresh tokens are Secure/HttpOnly cookies
  // and are deliberately never exposed to JavaScript.
  private readonly accessTokenSignal = signal<string | null>(null);
  private readonly userSignal = signal<AuthUser | null>(this.readUser());

  readonly user = this.userSignal.asReadonly();
  readonly isAuthenticated = computed(() => !!this.accessTokenSignal());
  readonly hasSession = computed(() => !!this.accessTokenSignal() || !!this.userSignal());
  readonly role = computed<BackendRole | null>(() => this.userSignal()?.role ?? this.readRoleFromToken(this.accessTokenSignal()));

  constructor() {
    // Remove credentials written by older builds.
    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem(LEGACY_TOKEN_KEY);
      localStorage.removeItem(LEGACY_REFRESH_KEY);
      localStorage.removeItem(USER_KEY);
    }
  }

  accessToken(): string | null { return this.accessTokenSignal(); }

  saveAuth(response: AuthResponse): void {
    const accessToken = response.accessToken ?? response.token ?? null;
    this.accessTokenSignal.set(accessToken);

    const role = this.normalizeRole(response.user?.role ?? response.role)
      ?? this.readRoleFromToken(accessToken)
      ?? this.userSignal()?.role
      ?? 'Customer';

    const user: AuthUser = response.user ?? {
      id: response.userId ?? this.userSignal()?.id,
      email: response.email ?? this.readEmailFromToken(accessToken) ?? this.userSignal()?.email ?? '',
      displayName: response.displayName
        ?? ([response.firstName, response.lastName].filter(Boolean).join(' ') || this.userSignal()?.displayName),
      role,
    };

    if (typeof sessionStorage !== 'undefined') sessionStorage.setItem(USER_KEY, JSON.stringify(user));
    this.userSignal.set(user);
  }

  updateUser(patch: Partial<AuthUser>): void {
    const current = this.userSignal();
    if (!current) return;
    const user = { ...current, ...patch };
    if (typeof sessionStorage !== 'undefined') sessionStorage.setItem(USER_KEY, JSON.stringify(user));
    this.userSignal.set(user);
  }

  updateTokens(tokens: Partial<AuthTokens>): void {
    if (tokens.accessToken !== undefined) this.accessTokenSignal.set(tokens.accessToken || null);
  }

  clear(): void {
    if (typeof sessionStorage !== 'undefined') sessionStorage.removeItem(USER_KEY);
    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem(LEGACY_TOKEN_KEY);
      localStorage.removeItem(LEGACY_REFRESH_KEY);
      localStorage.removeItem(USER_KEY);
    }
    this.accessTokenSignal.set(null);
    this.userSignal.set(null);
  }

  normalizeRole(value: BackendRoleValue | undefined | null): BackendRole | null {
    if (typeof value === 'number') return (['Customer', 'EventOrganizer', 'VenueOwner', 'Admin'][value] ?? null) as BackendRole | null;
    if (value && ['Customer', 'EventOrganizer', 'VenueOwner', 'Admin'].includes(value)) return value as BackendRole;
    return null;
  }

  routeForRole(role = this.role()): string {
    switch (role) {
      case 'Admin': return '/admin/dashboard';
      case 'EventOrganizer': return '/organizer/dashboard';
      case 'VenueOwner': return '/venue-owner/dashboard';
      default: return '/customer/dashboard';
    }
  }

  notificationsRoute(role = this.role()): string {
    switch (role) {
      case 'Admin': return '/admin/notifications';
      case 'EventOrganizer': return '/organizer/notifications';
      case 'VenueOwner': return '/venue-owner/notifications';
      default: return '/customer/notifications';
    }
  }

  private readUser(): AuthUser | null {
    if (typeof sessionStorage === 'undefined') return null;
    try { return JSON.parse(sessionStorage.getItem(USER_KEY) ?? 'null') as AuthUser | null; }
    catch { return null; }
  }

  private decode(token: string | null): Record<string, unknown> | null {
    if (!token) return null;
    try {
      const payload = token.split('.')[1];
      return JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/'))) as Record<string, unknown>;
    } catch { return null; }
  }

  private readRoleFromToken(token: string | null): BackendRole | null {
    const payload = this.decode(token);
    const value = payload?.['role'] ?? payload?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    if (typeof value !== 'string') return null;
    return ['Customer', 'EventOrganizer', 'VenueOwner', 'Admin'].includes(value) ? value as BackendRole : null;
  }

  private readEmailFromToken(token: string | null): string | null {
    const payload = this.decode(token);
    const value = payload?.['email'] ?? payload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'];
    return typeof value === 'string' ? value : null;
  }
}
