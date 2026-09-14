import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authInterceptor:HttpInterceptorFn = (req,next) => {
  const auth=inject(AuthService); const router=inject(Router); const token=auth.token();
  const request=token ? req.clone({setHeaders:{Authorization:`Bearer ${token}`},withCredentials:true}) : req.clone({withCredentials:true});
  return next(request).pipe(catchError((err:HttpErrorResponse)=>{
    const authEndpoint=/\/auth\/(login|refresh|register|verify|resend|password-reset)/.test(req.url);
    if(err.status!==401 || authEndpoint || !auth.isAuthenticated()) return throwError(()=>err);
    return auth.refresh().pipe(switchMap(session=>next(req.clone({setHeaders:{Authorization:`Bearer ${session.accessToken}`},withCredentials:true}))),catchError(refreshErr=>{const returnUrl=router.url;auth.clear();if(!returnUrl.startsWith('/auth/'))void router.navigate(['/auth/sign-in'],{queryParams:{returnUrl}});return throwError(()=>refreshErr);}));
  }));
};
