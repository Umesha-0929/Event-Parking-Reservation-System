import { Component, OnInit, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../core/services/admin.service';
import { PlatformReportDto } from '../../core/models/api.models';
import { PageStateComponent } from '../../shared/components/page-state.component';

@Component({
  selector:'app-admin-reports',
  standalone:true,
  imports:[FormsModule,DecimalPipe,PageStateComponent],
  template:`
  <div class="nv-page space-y-6">
    <div class="flex flex-wrap justify-between gap-4 items-end"><div><p class="font-extrabold text-sm nv-muted">Platform Analytics</p><h1 class="nv-page-title">Reports</h1><p class="nv-muted mt-1">Platform totals and trends for the selected date range.</p></div><button type="button" class="nv-btn nv-btn-secondary" (click)="downloadCsv()">Download CSV</button></div>
    <section class="nv-card p-5"><form class="grid sm:grid-cols-2 lg:grid-cols-[1fr_1fr_auto] gap-3" (ngSubmit)="load()"><div><label class="nv-label">From date</label><input class="nv-input" type="date" [(ngModel)]="fromDate" name="from" aria-label="From date"></div><div><label class="nv-label">To date</label><input class="nv-input" type="date" [(ngModel)]="toDate" name="to" aria-label="To date"></div><div class="flex items-end"><button type="submit" class="nv-btn nv-btn-primary w-full" [disabled]="loading()">{{loading()?'Loading...':'Apply Range'}}</button></div></form></section>
    @if(loading() && !report()){
      <div class="grid grid-cols-2 xl:grid-cols-4 gap-4" aria-label="Loading report">@for(i of [1,2,3,4];track i){<div class="nv-card p-5"><div class="nv-skeleton h-4 w-24"></div><div class="nv-skeleton h-9 w-20 mt-3"></div></div>}</div>
    } @else if(loadError()){
      <app-page-state kind="error" title="Report unavailable" message="We couldn't load platform report data for this range." actionLabel="Retry" [action]="retry"/>
    } @else if(report();as r){
      <div class="grid grid-cols-2 xl:grid-cols-4 gap-4">
        @for(k of cards(r);track k.label){<article class="nv-card p-5"><div class="text-sm nv-muted">{{k.label}}</div><div class="text-3xl font-black mt-2">{{k.money?'LKR ':''}}{{k.value|number:(k.money?'1.0-0':'1.0-0')}}</div></article>}
      </div>
      <div class="grid xl:grid-cols-2 gap-5">
        <section class="nv-card p-6"><h2 class="font-black text-xl">Activity mix</h2><div class="space-y-4 mt-5">@for(row of activityRows(r);track row.label){<div><div class="flex justify-between text-sm"><span class="nv-muted">{{row.label}}</span><strong>{{row.value}}</strong></div><div class="h-3 rounded-full overflow-hidden mt-2" style="background:var(--surface-2)"><div class="h-full rounded-full" style="background:var(--primary)" [style.width.%]="row.pct"></div></div></div>}</div></section>
        <section class="nv-card p-6"><h2 class="font-black text-xl">Revenue summary</h2><dl class="space-y-4 mt-5"><div class="flex justify-between"><dt class="nv-muted">Gross revenue</dt><dd class="font-black">LKR {{r.grossRevenue|number:'1.0-2'}}</dd></div><div class="flex justify-between"><dt class="nv-muted">Refunded amount</dt><dd class="font-black">LKR {{r.refundedAmount|number:'1.0-2'}}</dd></div><div class="flex justify-between border-t pt-4" style="border-color:var(--border)"><dt class="font-extrabold">Net revenue</dt><dd class="font-black text-xl">LKR {{r.netRevenue|number:'1.0-2'}}</dd></div><div class="flex justify-between"><dt class="nv-muted">Food revenue</dt><dd class="font-black">LKR {{r.foodRevenue|number:'1.0-2'}}</dd></div></dl></section>
      </div>
    } @else {<app-page-state title="No report data" message="No report data is available for the selected range."/>}
    @if(message()){<div class="fixed right-4 bottom-4 nv-card px-4 py-3" role="status">{{message()}}</div>}
  </div>`
})
export class AdminReportsComponent implements OnInit{
  private api=inject(AdminService); report=signal<PlatformReportDto|null>(null); loading=signal(false); loadError=signal(false); message=signal(''); retry=()=>this.load();
  fromDate='';toDate='';
  ngOnInit(){const now=new Date();const from=new Date(now);from.setDate(now.getDate()-30);this.fromDate=this.dateValue(from);this.toDate=this.dateValue(now);this.load();}
  params(){return{fromUtc:this.fromDate?new Date(`${this.fromDate}T00:00:00`).toISOString():undefined,toUtc:this.toDate?new Date(`${this.toDate}T23:59:59`).toISOString():undefined};}
  load(){this.loading.set(true);this.loadError.set(false);this.api.platformReport(this.params()).subscribe({next:r=>{this.report.set(r);this.loading.set(false);},error:()=>{this.report.set(null);this.loading.set(false);this.loadError.set(true);}});}
  downloadCsv(){this.api.platformCsv(this.params()).subscribe({next:blob=>{const url=URL.createObjectURL(blob);const a=document.createElement('a');a.href=url;a.download=`nvent-platform-report-${this.toDate||'report'}.csv`;a.click();URL.revokeObjectURL(url);},error:()=>this.message.set('CSV report could not be downloaded.')});}
  cards(r:PlatformReportDto){return[{label:'Users',value:r.users},{label:'Events',value:r.events},{label:'Venues',value:r.venues},{label:'Confirmed bookings',value:r.confirmedBookings},{label:'Successful payments',value:r.successfulPayments},{label:'Attendance',value:r.attendance},{label:'Parking reservations',value:r.parkingReservations},{label:'Food orders',value:r.foodOrders},{label:'Net revenue',value:r.netRevenue,money:true},{label:'Food revenue',value:r.foodRevenue,money:true}];}
  activityRows(r:PlatformReportDto){const max=Math.max(1,r.bookings||0,r.attendance||0,r.parkingReservations||0,r.foodOrders||0);return[{label:'Bookings',value:r.bookings||0,pct:(r.bookings||0)/max*100},{label:'Attendance',value:r.attendance||0,pct:(r.attendance||0)/max*100},{label:'Parking',value:r.parkingReservations||0,pct:(r.parkingReservations||0)/max*100},{label:'Food orders',value:r.foodOrders||0,pct:(r.foodOrders||0)/max*100}];}
  dateValue(d:Date){return `${d.getFullYear()}-${String(d.getMonth()+1).padStart(2,'0')}-${String(d.getDate()).padStart(2,'0')}`;}
}
