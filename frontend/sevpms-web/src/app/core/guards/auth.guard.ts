import { isPlatformBrowser } from '@angular/common';
import { PLATFORM_ID, inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { SessionService } from '../services/session.service';

export const authGuard: CanActivateFn = (_route, state) => {
  const platformId = inject(PLATFORM_ID);
  if (!isPlatformBrowser(platformId)) return true;

  const session = inject(SessionService);
  const router = inject(Router);
  return session.hasSession() ? true : router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};
