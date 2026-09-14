import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { UserRole } from '../models/api.models';

export const roleGuard=(roles:UserRole[]):CanActivateFn=>()=>{
  const auth=inject(AuthService);
  const router=inject(Router);
  if(!auth.isAuthenticated()) return router.parseUrl('/auth/sign-in');
  const role=auth.role();
  return role!==null&&roles.includes(role) ? true : router.parseUrl('/access-denied');
};
