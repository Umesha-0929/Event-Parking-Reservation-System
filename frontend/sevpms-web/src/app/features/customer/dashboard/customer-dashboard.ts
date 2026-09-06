import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';

@Component({selector:'app-customer-dashboard',imports:[RouterLink],templateUrl:'./customer-dashboard.html',styleUrl:'./customer-dashboard.scss'})
export class CustomerDashboardComponent implements OnInit {
  private readonly domain = inject(DomainApiService);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly bookingCount = signal(0);
  readonly unreadCount = signal(0);
  readonly totalSpend = signal(0);
  readonly quick=[{icon:'◇',title:'Discover events',copy:'Browse live event records and choose an experience.',path:'/customer/browse-events'},{icon:'▤',title:'My tickets',copy:'Open issued tickets and the QR codes you need for entry.',path:'/customer/tickets'},{icon:'P',title:'Parking',copy:'Use the parking recommendation and reservation flow.',path:'/customer/parking'},{icon:'⌖',title:'Place finder',copy:'Discover venue-context recommendations.',path:'/customer/places'}];
  ngOnInit():void{this.load();}
  load():void{this.loading.set(true);this.error.set('');forkJoin({bookings:this.domain.bookings(),notifications:this.domain.notifications()}).subscribe({next:({bookings,notifications})=>{this.bookingCount.set(bookings.length);this.unreadCount.set(notifications.filter((n)=>!n.isRead).length);this.totalSpend.set(bookings.reduce((s,b)=>s+Number(b.totalAmount??0),0));this.loading.set(false);},error:(error)=>{this.error.set(httpErrorMessage(error,'Your dashboard could not be loaded.'));this.loading.set(false);}});}
}
