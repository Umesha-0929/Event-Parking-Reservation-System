import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NotificationItem } from '../../core/models/api.models';
import { NotificationCenterService } from '../../core/services/notification-center.service';
import { httpErrorMessage } from '../../core/utils/http-error';
import { NavIconComponent } from '../../shared/components/nav-icon/nav-icon';

@Component({selector:'app-notifications',imports:[FormsModule,NavIconComponent],templateUrl:'./notifications.html',styleUrl:'./notifications.scss'})
export class NotificationsComponent implements OnInit{
 readonly center=inject(NotificationCenterService);readonly loading=signal(true);readonly error=signal('');readonly success=signal('');readonly busyId=signal('');readonly bulkBusy=signal(false);query='';state:'All'|'Unread'|'Read'='All';
 readonly filtered=computed(()=>{const query=this.query.trim().toLowerCase();return this.center.items().filter(item=>{const stateMatches=this.state==='All'||(this.state==='Unread'?!item.isRead:!!item.isRead);const text=`${item.title??''} ${item.message??''} ${item.type??''}`.toLowerCase();return stateMatches&&(!query||text.includes(query));});});
 ngOnInit():void{this.load();}
 load():void{this.loading.set(true);this.error.set('');this.center.refresh().subscribe({next:()=>this.loading.set(false),error:(error)=>{this.loading.set(false);this.error.set(httpErrorMessage(error,'Notifications could not be loaded.'));}});}
 id(item:NotificationItem):string{return item.notificationId??item.id??'';}
 markRead(item:NotificationItem):void{const id=this.id(item);if(!id||item.isRead||this.busyId())return;this.busyId.set(id);this.error.set('');this.center.markRead(id).subscribe({next:()=>this.busyId.set(''),error:(error)=>{this.busyId.set('');this.error.set(httpErrorMessage(error,'The notification could not be marked as read.'));}});}
 markAllRead():void{if(this.bulkBusy()||this.center.unreadCount()===0)return;this.bulkBusy.set(true);this.error.set('');this.success.set('');this.center.markAllRead().subscribe({next:()=>{this.bulkBusy.set(false);this.success.set('All notifications marked as read.');},error:e=>{this.bulkBusy.set(false);this.error.set(httpErrorMessage(e,'Notifications could not be marked as read.'));}});}
 clear(item:NotificationItem):void{const id=this.id(item);if(!id||this.busyId())return;if(!confirm('Clear this notification?'))return;this.busyId.set(id);this.error.set('');this.center.clearOne(id).subscribe({next:()=>{this.busyId.set('');this.success.set('Notification cleared.');},error:e=>{this.busyId.set('');this.error.set(httpErrorMessage(e,'The notification could not be cleared.'));}});}
 clearAll():void{if(this.bulkBusy()||this.center.items().length===0)return;if(!confirm('Clear all notifications? This removes them from your notification list.'))return;this.bulkBusy.set(true);this.error.set('');this.success.set('');this.center.clearAll().subscribe({next:()=>{this.bulkBusy.set(false);this.success.set('All notifications cleared.');},error:e=>{this.bulkBusy.set(false);this.error.set(httpErrorMessage(e,'Notifications could not be cleared.'));}});}
 when(value?:string):string{if(!value)return'';const date=new Date(value);return Number.isNaN(date.getTime())?value:new Intl.DateTimeFormat(undefined,{month:'short',day:'numeric',year:'numeric',hour:'numeric',minute:'2-digit'}).format(date);}
}
