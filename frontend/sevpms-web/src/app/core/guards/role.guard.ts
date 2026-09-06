import { isPlatformBrowser } from '@angular/common';
import { PLATFORM_ID, inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { BackendRole } from '../models/api.models';
import { SessionService } from '../services/session.service';

export const roleGuard: CanActivateFn = (route, state) => {
  const platformId = inject(PLATFORM_ID);
  if (!isPlatformBrowser(platformId)) return true;

  const session = inject(SessionService);
  const router = inject(Router);
  if (!session.hasSession()) return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });

  const allowed = (route.data?.['roles'] ?? []) as BackendRole[];
  const role = session.role();
  return !allowed.length || (role && allowed.includes(role))
    ? true
    : router.createUrlTree(['/access-denied']);
};
