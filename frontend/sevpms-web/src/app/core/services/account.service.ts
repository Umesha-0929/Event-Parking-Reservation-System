import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import { ChangePasswordRequest, UpdateProfileRequest, UserProfile } from '../models/api.models';

@Injectable({providedIn:'root'})
export class AccountService {
  private api=inject(ApiService);
  me(){return this.api.get<UserProfile>('users/me');}
  update(body:UpdateProfileRequest){return this.api.put<UserProfile>('users/me',body);}
  password(currentPassword:string,newPassword:string){
    const body:ChangePasswordRequest={currentPassword,newPassword};
    return this.api.put<void>('users/me/password',body);
  }
}
