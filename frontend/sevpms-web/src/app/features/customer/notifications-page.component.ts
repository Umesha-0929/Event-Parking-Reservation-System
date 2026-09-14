import { Component, OnInit, inject, signal } from '@angular/core';
import { NotificationsService } from '../../core/services/notifications.service';
import { NotificationDto, USER_ROLE } from '../../core/models/api.models';
import { PageStateComponent } from '../../shared/components/page-state.component';
import { NotificationItemComponent } from '../../shared/components/notification-item.component';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector:'app-notifications-page',
  standalone:true,
  imports:[PageStateComponent,NotificationItemComponent],
  template:`
  <div class="nv-page space-y-6">
    <section class="nv-hero min-h-[180px]">
      <div class="relative z-10">
        <p class="font-extrabold text-sm opacity-75">Updates</p>
        <h1 class="nv-page-title mt-1">Notifications</h1>
        <p class="mt-2 opacity-80">Booking, ticket, event, parking, food, payment and system updates.</p>
      </div>
    </section>

    <section class="nv-card p-4">
      <div class="flex flex-wrap gap-2 items-center">
        @for(c of categories;track c){
          <button type="button" class="nv-chip" [class.active]="category===c" (click)="category=c">{{c}}</button>
        }
        <button type="button" class="nv-btn nv-btn-secondary ml-auto" (click)="markAll()" [disabled]="items().length===0||unreadCount===0">Mark All as Read</button>
      </div>
    </section>

    @if(loading()){
      <div class="space-y-3">@for(i of [1,2,3,4];track i){<div class="nv-skeleton h-24"></div>}</div>
    }@else if(error()){
      <app-page-state kind="error" title="We couldn't load notifications" message="Please check your connection and try again." actionLabel="Retry" [action]="retry"/>
    }@else if(filtered.length){
      <div class="space-y-3">
        @for(notification of filtered;track notification.notificationId){
          <app-notification-item
            [notification]="notification"
            [category]="group(notification.type)"
            [icon]="icon(notification.type)"
            (markRead)="read($event)"
            (removed)="remove($event)"/>
        }
      </div>
    }@else{
      <app-page-state title="You're all caught up" message="There are no notifications in this category."/>
    }
  </div>`
})
export class NotificationsPageComponent implements OnInit {
  private api=inject(NotificationsService);
  private auth=inject(AuthService);
  items=this.api.items;
  loading=signal(true);
  error=signal(false);
  category='All';
  retry=()=>this.load();

  get categories(){
    const base=['All','Bookings','Tickets','Events','Parking','Food','Payments'];
    return this.auth.role()===USER_ROLE.EventOrganizer?[...base,'Organizer','System']:[...base,'System'];
  }
  get filtered(){return this.items().filter(n=>this.category==='All'||this.group(n.type)===this.category);}
  get unreadCount(){return this.items().filter(item=>!item.isRead).length;}

  ngOnInit(){this.load();}
  load(){this.loading.set(true);this.error.set(false);this.api.list().subscribe({next:()=>this.loading.set(false),error:()=>{this.error.set(true);this.loading.set(false);}});}

  group(type?:string|number){
    const s=String(type??'').toLowerCase();
    if(s.includes('booking'))return'Bookings';
    if(s.includes('ticket')||s.includes('check'))return'Tickets';
    if(s.includes('event')||s.includes('waitlist'))return'Events';
    if(s.includes('parking'))return'Parking';
    if(s.includes('food')||s.includes('order'))return'Food';
    if(s.includes('payment')||s.includes('receipt')||s.includes('refund'))return'Payments';
    if(s.includes('organizer')||s.includes('publish')||s.includes('rental'))return'Organizer';
    return'System';
  }

  icon(type?:string|number){
    const g=this.group(type);
    return g==='Bookings'?'▤':g==='Tickets'?'⌗':g==='Events'?'◫':g==='Parking'?'P':g==='Food'?'◉':g==='Payments'?'¤':'●';
  }

  read(notification:NotificationDto){if(notification.isRead)return;this.api.read(notification.notificationId).subscribe();}
  markAll(){if(this.unreadCount>0)this.api.readAll().subscribe();}
  remove(notification:NotificationDto){this.api.remove(notification.notificationId).subscribe();}
}
