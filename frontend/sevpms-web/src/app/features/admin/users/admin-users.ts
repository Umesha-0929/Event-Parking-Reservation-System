import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';

interface AdminUserRow{userId:string;firstName?:string;lastName?:string;email?:string;phoneNumber?:string|null;role?:string|number;status?:string|number;createdAtUtc?:string;lastLoginAtUtc?:string|null;}
@Component({selector:'app-admin-users',imports:[FormsModule],templateUrl:'./admin-users.html',styleUrl:'./admin-users.scss'})
export class AdminUsersComponent implements OnInit{
 private readonly domain=inject(DomainApiService);readonly users=signal<AdminUserRow[]>([]);readonly loading=signal(true);readonly error=signal('');readonly busy=signal('');query='';
 ngOnInit():void{this.load();}load():void{this.loading.set(true);this.error.set('');this.domain.adminUsers().subscribe({next:u=>{this.users.set(u as AdminUserRow[]);this.loading.set(false);},error:e=>{this.loading.set(false);this.error.set(httpErrorMessage(e,'Users could not be loaded.'));}});}
 filtered():AdminUserRow[]{const q=this.query.trim().toLowerCase();return this.users().filter(u=>!q||`${u.firstName} ${u.lastName} ${u.email} ${this.role(u.role)} ${this.status(u.status)}`.toLowerCase().includes(q));}
 role(v:string|number|undefined):string{return typeof v==='number'?(['Customer','Event Organizer','Venue Owner','Admin'][v]??String(v)):(v||'—').replace('EventOrganizer','Event Organizer').replace('VenueOwner','Venue Owner');}
 status(v:string|number|undefined):string{return typeof v==='number'?(['Active','Inactive','Suspended'][v]??String(v)):(v||'Active');}
 setStatus(u:AdminUserRow,status:0|1|2):void{if(!u.userId||this.busy())return;this.busy.set(u.userId);this.error.set('');this.domain.updateAdminUserStatus(u.userId,status).subscribe({next:()=>{this.busy.set('');this.load();},error:e=>{this.busy.set('');this.error.set(httpErrorMessage(e,'User status could not be changed.'));}});}
}
