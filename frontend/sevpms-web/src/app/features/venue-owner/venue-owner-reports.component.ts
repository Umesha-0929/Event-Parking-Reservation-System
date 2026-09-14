import { Component, OnInit, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { ReportsService } from '../../core/services/reports.service';
import { VenueOwnerReportDto } from '../../core/models/api.models';
import { PageStateComponent } from '../../shared/components/page-state.component';

@Component({
  selector:'app-venue-owner-reports',
  standalone:true,
  imports:[DecimalPipe,PageStateComponent],
  template:`<div class="nv-page space-y-6">
    <div><p class="font-extrabold text-sm nv-muted">Performance</p><h1 class="nv-page-title">Reports</h1><p class="nv-muted mt-1">Venue-owner metrics provided by the reporting service.</p></div>
    @if(loading()){
      <div class="grid sm:grid-cols-2 xl:grid-cols-4 gap-4" aria-label="Loading venue report">@for(i of [1,2,3,4];track i){<div class="nv-card p-6"><div class="nv-skeleton h-4 w-28"></div><div class="nv-skeleton h-10 w-24 mt-3"></div></div>}</div>
      <div class="nv-card p-6"><div class="nv-skeleton h-6 w-36"></div><div class="nv-skeleton h-3 w-full max-w-xl mt-6"></div></div>
    } @else if(report();as r){
      <div class="grid sm:grid-cols-2 xl:grid-cols-4 gap-4"><article class="nv-card p-6"><div class="nv-muted">Venues</div><div class="text-4xl font-black mt-2">{{r.venues}}</div></article><article class="nv-card p-6"><div class="nv-muted">Rental requests</div><div class="text-4xl font-black mt-2">{{r.rentalRequests}}</div></article><article class="nv-card p-6"><div class="nv-muted">Accepted rentals</div><div class="text-4xl font-black mt-2">{{r.acceptedRentals}}</div></article><article class="nv-card p-6"><div class="nv-muted">Accepted rental value</div><div class="text-3xl font-black mt-2">LKR {{r.acceptedRentalValue|number:'1.0-0'}}</div></article></div>
      <section class="nv-card p-6"><h2 class="font-black text-xl">Rental conversion</h2><div class="max-w-xl mt-5"><div class="flex justify-between text-sm"><span class="nv-muted">Accepted / requested</span><strong>{{pct(r.acceptedRentals,r.rentalRequests)}}%</strong></div><div class="h-3 rounded-full overflow-hidden mt-2" style="background:var(--surface-elevated)"><div class="h-full rounded-full" style="background:var(--accent)" [style.width.%]="pct(r.acceptedRentals,r.rentalRequests)"></div></div></div></section>
    } @else {
      <app-page-state kind="error" title="Report unavailable" message="We couldn't load venue performance data right now." actionLabel="Retry" [action]="retry"/>
    }
  </div>`
})
export class VenueOwnerReportsComponent implements OnInit{
  private api=inject(ReportsService);
  report=signal<VenueOwnerReportDto|null>(null);
  loading=signal(true);
  retry=()=>this.load();
  ngOnInit(){this.load();}
  load(){this.loading.set(true);this.api.venueOwner().subscribe({next:r=>{this.report.set(r);this.loading.set(false);},error:()=>{this.report.set(null);this.loading.set(false);}});}
  pct(a:number,b:number){return b?Math.min(100,Math.round(a/b*100)):0;}
}
