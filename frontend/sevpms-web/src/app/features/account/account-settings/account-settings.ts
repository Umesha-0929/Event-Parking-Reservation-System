import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SessionService } from '../../../core/services/session.service';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';

@Component({selector:'app-account-settings',imports:[FormsModule],templateUrl:'./account-settings.html',styleUrl:'./account-settings.scss'})
export class AccountSettingsComponent implements OnInit{
 private readonly domain=inject(DomainApiService); readonly session=inject(SessionService);
 readonly loading=signal(true); readonly saving=signal(false); readonly error=signal(''); readonly success=signal(''); readonly passwordVisible=signal(false);
 firstName='';lastName='';phoneNumber='';email='';currentPassword='';newPassword='';confirmPassword='';
 ngOnInit():void{this.load();}
 private splitName(name?:string):[string,string]{const parts=(name??'').trim().split(/\s+/).filter(Boolean);return [parts.shift()??'',parts.join(' ')];}
 load():void{this.loading.set(true);this.error.set('');this.domain.me().subscribe({next:u=>{const [given,family]=this.splitName(u.name);this.firstName=u.firstName??given;this.lastName=u.lastName??family;this.phoneNumber=u.phoneNumber??'';this.email=u.email??this.session.user()?.email??'';this.loading.set(false);},error:e=>{this.loading.set(false);this.error.set(httpErrorMessage(e,'Your account could not be loaded.'));}});}
 saveProfile():void{if(!this.firstName.trim()||!this.lastName.trim()||this.saving())return;this.saving.set(true);this.error.set('');this.success.set('');this.domain.updateMe({firstName:this.firstName.trim(),lastName:this.lastName.trim(),phoneNumber:this.phoneNumber.trim()||null}).subscribe({next:u=>{this.saving.set(false);this.success.set('Profile updated.');const [given,family]=this.splitName(u.name);const first=u.firstName??given??this.firstName;const last=u.lastName??family??this.lastName;this.firstName=first||this.firstName;this.lastName=last||this.lastName;this.session.updateUser({firstName:this.firstName,lastName:this.lastName,displayName:u.name||[this.firstName,this.lastName].filter(Boolean).join(' '),email:u.email??this.email});},error:e=>{this.saving.set(false);this.error.set(httpErrorMessage(e,'Profile changes could not be saved.'));}});}
 changePassword():void{if(!this.currentPassword||this.newPassword.length<8||this.newPassword!==this.confirmPassword||this.saving())return;this.saving.set(true);this.error.set('');this.success.set('');this.domain.changePassword({currentPassword:this.currentPassword,newPassword:this.newPassword}).subscribe({next:()=>{this.saving.set(false);this.success.set('Password changed successfully.');this.currentPassword='';this.newPassword='';this.confirmPassword='';},error:e=>{this.saving.set(false);this.error.set(httpErrorMessage(e,'Your password could not be changed.'));}});}
}
