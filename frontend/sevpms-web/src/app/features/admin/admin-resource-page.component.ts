import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AdminService } from '../../core/services/admin.service';
import { EventsService } from '../../core/services/events.service';
import { VenuesService } from '../../core/services/venues.service';
import { FoodService } from '../../core/services/food.service';
import { BookingDto, EventDto, FoodOrder, VenueDto } from '../../core/models/api.models';
import { PageStateComponent } from '../../shared/components/page-state.component';

@Component({
  selector:'app-admin-resource-page',
  standalone:true,
  imports:[RouterLink,DatePipe,DecimalPipe,PageStateComponent],
  template:`
  <div class="nv-page space-y-6">
    <div>
      <p class="font-extrabold text-sm nv-muted">Platform Management</p>
      <h1 class="nv-page-title">{{title}}</h1>
      <p class="nv-muted mt-1">{{description}}</p>
    </div>

    @if(loading()){
      <div class="grid md:grid-cols-2 xl:grid-cols-3 gap-4" aria-label="Loading management records">@for(i of [1,2,3];track i){<div class="nv-card p-5"><div class="nv-skeleton h-5 w-24"></div><div class="nv-skeleton h-7 w-2/3 mt-4"></div><div class="nv-skeleton h-4 w-1/2 mt-3"></div><div class="nv-skeleton h-10 w-32 mt-5"></div></div>}</div>
    } @else if(loadError()){
      <app-page-state kind="error" title="Records unavailable" message="We couldn't load this management view right now." actionLabel="Retry" [action]="retry"/>
    } @else if(mode==='events'){
      <div class="grid md:grid-cols-2 xl:grid-cols-3 gap-4">
        @for(e of events();track e.eventId){
          <article class="nv-card p-5">
            <span class="nv-status" [class.success]="e.status===1">{{eventStatus(e.status)}}</span>
            <h2 class="font-black text-xl mt-3">{{e.title}}</h2>
            <p class="nv-muted text-sm mt-2">{{e.startAtUtc|date:'medium'}}</p>
            <a [routerLink]="['/app/events',e.eventId]" class="nv-btn nv-btn-secondary mt-4">View Public Detail</a>
          </article>
        } @empty {
          <div class="nv-card p-10 text-center nv-muted md:col-span-2 xl:col-span-3">No public events are available.</div>
        }
      </div>
    } @else if(mode==='venues'){
      <div class="grid md:grid-cols-2 xl:grid-cols-3 gap-4">
        @for(v of venues();track v.venueId){
          <article class="nv-card p-5">
            <span class="nv-status" [class.success]="v.isActive">{{v.isActive?'Active':'Inactive'}}</span>
            <h2 class="font-black text-xl mt-3">{{v.name}}</h2>
            <p class="nv-muted text-sm mt-2">{{v.city}} · {{v.capacity}} capacity</p>
            <a [routerLink]="['/app/venues',v.venueId]" class="nv-btn nv-btn-secondary mt-4">View Venue</a>
          </article>
        } @empty {
          <div class="nv-card p-10 text-center nv-muted md:col-span-2 xl:col-span-3">No venues are available.</div>
        }
      </div>
    } @else if(mode==='bookings'){
      <section class="nv-card p-5 md:p-6">
        <div class="overflow-x-auto">
          <table class="w-full text-sm">
            <caption class="sr-only">Platform booking records</caption>
            <thead><tr class="text-left nv-muted"><th class="py-3 pr-4">Booking</th><th class="py-3 pr-4">Event</th><th class="py-3 pr-4">Customer</th><th class="py-3 pr-4">Seats</th><th class="py-3 pr-4">Amount</th><th class="py-3 pr-4">Status</th><th class="py-3">Created</th></tr></thead>
            <tbody>
              @for(b of bookings();track b.bookingId){
                <tr class="border-t" style="border-color:var(--border)">
                  <td class="py-4 pr-4"><div class="font-extrabold">{{b.bookingNumber}}</div><div class="text-xs nv-muted mt-1">{{short(b.bookingId)}}</div></td>
                  <td class="py-4 pr-4">{{short(b.eventId)}}</td>
                  <td class="py-4 pr-4">{{short(b.customerUserId)}}</td>
                  <td class="py-4 pr-4">{{b.seatIds.length}}</td>
                  <td class="py-4 pr-4">LKR {{b.totalAmount|number:'1.0-2'}}</td>
                  <td class="py-4 pr-4"><span class="nv-status" [class.success]="b.status===1">{{bookingStatus(b.status)}}</span></td>
                  <td class="py-4">{{b.createdAtUtc|date:'medium'}}</td>
                </tr>
              } @empty {
                <tr><td colspan="7" class="py-12 text-center nv-muted">No booking records are available.</td></tr>
              }
            </tbody>
          </table>
        </div>
      </section>
    } @else if(mode==='food'){
      <section class="nv-card p-5 md:p-6">
        <div class="overflow-x-auto">
          <table class="w-full text-sm">
            <caption class="sr-only">Platform food-order records</caption>
            <thead><tr class="text-left nv-muted"><th class="py-3 pr-4">Order</th><th class="py-3 pr-4">Event</th><th class="py-3 pr-4">Customer</th><th class="py-3 pr-4">Fulfilment</th><th class="py-3 pr-4">Items</th><th class="py-3 pr-4">Total</th><th class="py-3 pr-4">Status</th><th class="py-3">Created</th></tr></thead>
            <tbody>
              @for(order of foodOrders();track order.id){
                <tr class="border-t" style="border-color:var(--border)">
                  <td class="py-4 pr-4"><div class="font-extrabold">{{order.orderNo}}</div><div class="text-xs nv-muted mt-1">{{short(order.id)}}</div></td>
                  <td class="py-4 pr-4">{{short(order.eventId)}}</td>
                  <td class="py-4 pr-4">{{short(order.customerUserId)}}</td>
                  <td class="py-4 pr-4">{{order.fulfillmentType}}</td>
                  <td class="py-4 pr-4">{{foodItemCount(order)}}</td>
                  <td class="py-4 pr-4">LKR {{order.total|number:'1.0-2'}}</td>
                  <td class="py-4 pr-4"><span class="nv-status" [class.success]="order.status==='Completed'">{{order.status}}</span></td>
                  <td class="py-4">{{order.createdAtUtc|date:'medium'}}</td>
                </tr>
              } @empty {
                <tr><td colspan="8" class="py-12 text-center nv-muted">No food-order records are available.</td></tr>
              }
            </tbody>
          </table>
        </div>
      </section>
    } @else {
      <section class="nv-card p-8">
        <div class="max-w-3xl">
          <h2 class="font-black text-xl">{{coverageTitle}}</h2>
          <p class="nv-muted mt-3">{{coverageText}}</p>
          <div class="nv-card-soft p-4 text-sm mt-5">Detailed records are not available in this workspace. Use the dashboard and reports for the current platform totals.</div>
        </div>
      </section>
    }
  </div>`
})
export class AdminResourcePageComponent implements OnInit {
  private route=inject(ActivatedRoute);
  private eventsApi=inject(EventsService);
  private venuesApi=inject(VenuesService);
  private food=inject(FoodService);
  private admin=inject(AdminService);

  mode='';
  title='';
  description='';
  coverageTitle='Data availability';
  coverageText='';
  events=signal<EventDto[]>([]);
  venues=signal<VenueDto[]>([]);
  bookings=signal<BookingDto[]>([]);
  foodOrders=signal<FoodOrder[]>([]);
  loading=signal(true);
  loadError=signal(false);
  retry=()=>this.load();

  ngOnInit(){
    this.mode=this.route.snapshot.data['mode']??'';
    const map:Record<string,[string,string]>={
      events:['Events','Review published and upcoming events across Nvent.'],
      venues:['Venues','Review active venues across Nvent.'],
      bookings:['Bookings','Review booking records, seat counts and current status across the platform.'],
      food:['Food Orders','Review food orders and fulfilment status across the platform.']
    };
    [this.title,this.description]=map[this.mode]??['Management',''];
    this.load();
  }

  load(){
    this.loading.set(true);
    this.loadError.set(false);
    const success=()=>this.loading.set(false);
    const failed=()=>{this.loading.set(false);this.loadError.set(true);};
    if(this.mode==='events'){this.eventsApi.list().subscribe({next:v=>{this.events.set(v);success();},error:failed});return;}
    if(this.mode==='venues'){this.venuesApi.list().subscribe({next:v=>{this.venues.set(v);success();},error:failed});return;}
    if(this.mode==='bookings'){this.admin.bookings().subscribe({next:v=>{this.bookings.set(v);success();},error:failed});return;}
    if(this.mode==='food'){this.food.adminOrders().subscribe({next:v=>{this.foodOrders.set(v);success();},error:failed});return;}
    success();
  }

  eventStatus(v:number){return ['Draft','Published','Cancelled','Completed'][Number(v)]??String(v);}
  bookingStatus(v:number){return ['Pending','Confirmed','Cancelled','Completed'][Number(v)]??String(v);}
  foodItemCount(order:FoodOrder){return order.items.reduce((total,item)=>total+item.quantity,0);}
  short(value:unknown){const s=String(value??'');return s.length>12?`${s.slice(0,8)}…${s.slice(-4)}`:(s||'—');}
}
