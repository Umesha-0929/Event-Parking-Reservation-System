import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { SessionService } from '../../../core/services/session.service';
import { httpErrorMessage } from '../../../core/utils/http-error';
import { NavIconComponent } from '../../../shared/components/nav-icon/nav-icon';

interface AdminUserRow{userId:string;firstName?:string;lastName?:string;email?:string;phoneNumber?:string|null;role?:string|number;status?:string|number;createdAtUtc?:string;lastLoginAtUtc?:string|null;}
@Component({selector:'app-admin-users',imports:[FormsModule,NavIconComponent],templateUrl:'./admin-users.html',styleUrl:'./admin-users.scss'})
export class AdminUsersComponent implements OnInit{
 private readonly domain=inject(DomainApiService);private readonly session=inject(SessionService);readonly users=signal<AdminUserRow[]>([]);readonly loading=signal(true);readonly error=signal('');readonly success=signal('');readonly busy=signal('');readonly openMenu=signal('');query='';
 ngOnInit():void{this.load();}
 load():void{this.loading.set(true);this.error.set('');this.domain.adminUsers().subscribe({next:u=>{this.users.set(u as AdminUserRow[]);this.loading.set(false);},error:e=>{this.loading.set(false);this.error.set(httpErrorMessage(e,'Users could not be loaded.'));}});}
 filtered():AdminUserRow[]{const q=this.query.trim().toLowerCase();return this.users().filter(u=>!q||`${u.firstName} ${u.lastName} ${u.email} ${this.role(u.role)} ${this.status(u.status)}`.toLowerCase().includes(q));}
 role(v:string|number|undefined):string{return typeof v==='number'?(['Customer','Event Organizer','Venue Owner','Admin'][v]??String(v)):(v||'—').replace('EventOrganizer','Event Organizer').replace('VenueOwner','Venue Owner');}
 status(v:string|number|undefined):string{if(typeof v==='number')return ['Active','Inactive','Suspended'][v]??String(v);const text=String(v??'Active');const n=Number(text);return Number.isNaN(n)?text:(['Active','Inactive','Suspended'][n]??text);}
 statusClass(u:AdminUserRow):string{return `account-status account-status--${this.status(u.status).toLowerCase()}`;}
 initials(u:AdminUserRow):string{return `${u.firstName?.[0]??''}${u.lastName?.[0]??''}`.trim().toUpperCase()||u.email?.[0]?.toUpperCase()||'U';}
 isCurrent(u:AdminUserRow):boolean{const current=this.session.user();return (!!current?.id&&current.id===u.userId)||!!(current?.email&&u.email&&current.email.toLowerCase()===u.email.toLowerCase());}
 lastLogin(u:AdminUserRow):string{if(!u.lastLoginAtUtc)return 'Never';const d=new Date(u.lastLoginAtUtc);return Number.isNaN(d.getTime())?'Never':new Intl.DateTimeFormat(undefined,{month:'short',day:'numeric',year:'numeric',hour:'numeric',minute:'2-digit'}).format(d);}
 toggleMenu(u:AdminUserRow):void{this.openMenu.set(this.openMenu()===u.userId?'':u.userId);}
 setStatus(u:AdminUserRow,status:0|1|2):void{if(!u.userId||this.busy())return;if(this.isCurrent(u)){this.error.set('For safety, use another administrator account to change the currently signed-in admin.');this.openMenu.set('');return;}const target=['Active','Inactive','Suspended'][status];const verb=target==='Active'?'activate':target==='Inactive'?'deactivate':'suspend';if(!confirm(`${verb[0].toUpperCase()+verb.slice(1)} ${u.firstName??''} ${u.lastName??''}?`))return;this.busy.set(u.userId);this.error.set('');this.success.set('');this.domain.updateAdminUserStatus(u.userId,status).subscribe({next:(response)=>{const updated=response as AdminUserRow;this.users.update(items=>items.map(item=>item.userId===u.userId?{...item,...updated,status:updated.status??status}:item));this.busy.set('');this.openMenu.set('');this.success.set(target === 'Active' ? 'Account activated successfully.' : target === 'Inactive' ? 'Account deactivated successfully.' : 'Account suspended successfully.');},error:e=>{this.busy.set('');this.openMenu.set('');this.error.set(httpErrorMessage(e, target === 'Active' ? 'The account could not be activated.' : target === 'Inactive' ? 'The account could not be deactivated.' : 'The account could not be suspended.'));}});}
}
