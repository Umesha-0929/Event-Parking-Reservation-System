import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { SessionService } from '../services/session.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const session = inject(SessionService);
  const auth = inject(AuthService);
  const token = session.accessToken();
  const authRequest = token ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : request;

  return next(authRequest).pipe(
    catchError((error: HttpErrorResponse) => {
      const refreshToken = session.refreshToken();
      const isAuthCall = request.url.includes('/auth/login')
        || request.url.includes('/auth/register')
        || request.url.includes('/auth/refresh')
        || request.url.includes('/auth/logout');

      if (error.status !== 401 || !refreshToken || isAuthCall) {
        return throwError(() => error);
      }

      // Only a failed REFRESH invalidates the local session. A later API error must
      // not silently log the user out.
      return auth.refresh(refreshToken).pipe(
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
