import { Component, OnInit, inject, signal } from '@angular/core';
import { ReportsService } from '../../core/services/reports.service';
import { OrganizerReportDto } from '../../core/models/api.models';
import { PageStateComponent } from '../../shared/components/page-state.component';

@Component({
  selector:'app-organizer-analytics',
  standalone:true,
  imports:[PageStateComponent],
  template:`<div class="nv-page space-y-6">
    <div><p class="font-extrabold text-sm nv-muted">Performance</p><h1 class="nv-page-title">Analytics</h1><p class="nv-muted mt-1">Event performance, attendance and service activity.</p></div>
    @if(loading()){
      <div class="grid sm:grid-cols-2 xl:grid-cols-4 gap-4" aria-label="Loading analytics">
        @for(i of [1,2,3,4];track i){<div class="nv-card p-5"><div class="nv-skeleton h-4 w-28"></div><div class="nv-skeleton h-9 w-20 mt-3"></div></div>}
      </div>
      <div class="nv-card p-6"><div class="nv-skeleton h-6 w-36"></div><div class="grid md:grid-cols-3 gap-4 mt-5">@for(i of [1,2,3];track i){<div class="nv-skeleton h-28"></div>}</div></div>
    } @else if(report();as r){
      <div class="grid sm:grid-cols-2 xl:grid-cols-4 gap-4">@for(k of cards(r);track k.label){<article class="nv-card p-5"><div class="nv-muted text-sm">{{k.label}}</div><div class="text-3xl font-black mt-2">{{k.prefix}}{{k.value}}</div></article>}</div>
      <section class="nv-card p-6"><h2 class="font-black text-xl">Event funnel</h2><div class="grid md:grid-cols-3 gap-4 mt-5"><div class="nv-card-soft p-4"><div class="nv-muted text-sm">Published rate</div><div class="font-black text-2xl mt-2">{{pct(r.publishedEvents,r.events)}}%</div><div class="h-2 rounded-full mt-3 overflow-hidden" style="background:var(--surface-elevated)"><div class="h-full rounded-full" [style.width.%]="pct(r.publishedEvents,r.events)" style="background:var(--accent)"></div></div></div><div class="nv-card-soft p-4"><div class="nv-muted text-sm">Attendance / bookings</div><div class="font-black text-2xl mt-2">{{pct(r.attendance,r.confirmedBookings)}}%</div><div class="h-2 rounded-full mt-3 overflow-hidden" style="background:var(--surface-elevated)"><div class="h-full rounded-full" [style.width.%]="pct(r.attendance,r.confirmedBookings)" style="background:var(--accent-2)"></div></div></div><div class="nv-card-soft p-4"><div class="nv-muted text-sm">Services</div><div class="font-black text-2xl mt-2">{{r.parkingReservations + r.foodOrders}}</div><p class="nv-muted text-xs mt-2">Parking + food activity</p></div></div></section>
    } @else {
      <app-page-state kind="error" title="Analytics unavailable" message="We couldn't load organizer analytics right now." actionLabel="Retry" [action]="retry"/>
    }
  </div>`
})
export class OrganizerAnalyticsComponent implements OnInit{
  private reports=inject(ReportsService);
  report=signal<OrganizerReportDto|null>(null);
  loading=signal(true);
  retry=()=>this.load();
  ngOnInit(){this.load();}
  load(){this.loading.set(true);this.reports.organizer().subscribe({next:r=>{this.report.set(r);this.loading.set(false);},error:()=>{this.report.set(null);this.loading.set(false);}});}
  cards(r:OrganizerReportDto){return[{label:'Events',value:r.events??0,prefix:''},{label:'Confirmed bookings',value:r.confirmedBookings??0,prefix:''},{label:'Attendance',value:r.attendance??0,prefix:''},{label:'Revenue',value:Number(r.revenue??0).toLocaleString(),prefix:'LKR '},{label:'Parking reservations',value:r.parkingReservations??0,prefix:''},{label:'Food orders',value:r.foodOrders??0,prefix:''},{label:'Food revenue',value:Number(r.foodRevenue??0).toLocaleString(),prefix:'LKR '}];}
  pct(a:number,b:number){return b?Math.min(100,Math.round(a/b*100)):0;}
}
