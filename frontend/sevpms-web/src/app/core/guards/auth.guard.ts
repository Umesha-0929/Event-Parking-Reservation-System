import { CanActivateFn, Router } from '@angular/router'; import { inject } from '@angular/core'; import { AuthService } from '../services/auth.service';
export const authGuard:CanActivateFn=(_,state)=>{const auth=inject(AuthService);return auth.isAuthenticated()?true:inject(Router).createUrlTree(['/auth/sign-in'],{queryParams:{returnUrl:state.url}});};
