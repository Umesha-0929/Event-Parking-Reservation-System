import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable, catchError, finalize, shareReplay, switchMap, throwError } from 'rxjs';
import { AuthResponse } from '../models/api.models';
import { AuthService } from '../services/auth.service';
import { SessionService } from '../services/session.service';

let refreshInFlight$: Observable<AuthResponse> | null = null;

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const session = inject(SessionService);
  const auth = inject(AuthService);
  const token = session.accessToken();
  const authRequest = token ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : request;

  return next(authRequest).pipe(
    catchError((error: HttpErrorResponse) => {
      const isAuthCall = request.url.includes('/auth/login')
        || request.url.includes('/auth/register')
        || request.url.includes('/auth/refresh')
        || request.url.includes('/auth/logout');

      if (error.status !== 401 || isAuthCall) return throwError(() => error);

      if (!refreshInFlight$) {
        refreshInFlight$ = auth.refresh().pipe(
          shareReplay({ bufferSize: 1, refCount: false }),
          finalize(() => { refreshInFlight$ = null; }),
        );
      }

      return refreshInFlight$.pipe(
        catchError((refreshError) => {
          session.clear();
          return throwError(() => refreshError);
        }),
        switchMap(() => {
          const refreshed = session.accessToken();
          const retryRequest = refreshed
            ? request.clone({ setHeaders: { Authorization: `Bearer ${refreshed}` } })
            : request;
          return next(retryRequest);
        }),
      );
    }),
  );
};
