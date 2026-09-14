import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { VenuesService } from '../../core/services/venues.service';
import { VenueRentalDto } from '../../core/models/api.models';
import { PageStateComponent } from '../../shared/components/page-state.component';

@Component({
  selector:'app-venue-owner-rentals',
  standalone:true,
  imports:[FormsModule,DatePipe,DecimalPipe,PageStateComponent],
  template:`<div class="nv-page space-y-6">
    <div><p class="font-extrabold text-sm nv-muted">Venue Rentals</p><h1 class="nv-page-title">Incoming Requests</h1><p class="nv-muted mt-1">Review organizer requests for venues you own.</p></div>
    @if(loading()){
      <div class="grid gap-4" aria-label="Loading rental requests">@for(i of [1,2,3];track i){<div class="nv-card p-5"><div class="nv-skeleton h-5 w-24"></div><div class="nv-skeleton h-7 w-2/3 mt-4"></div><div class="nv-skeleton h-4 w-1/2 mt-3"></div><div class="nv-skeleton h-12 mt-5"></div></div>}</div>
    } @else if(loadError()){
      <app-page-state kind="error" title="Rental requests unavailable" message="We couldn't load incoming rental requests right now." actionLabel="Retry" [action]="retry"/>
    } @else if(rentals().length){
      <div class="grid gap-4">@for(r of rentals();track r.rentalRequestId){<article class="nv-card p-5"><div class="flex flex-wrap justify-between gap-4"><div><span class="nv-status" [class.success]="r.status===1">{{status(r.status)}}</span><h2 class="font-black text-xl mt-3">{{r.purpose}}</h2><p class="nv-muted text-sm mt-1">{{r.startAtUtc|date:'medium'}} – {{r.endAtUtc|date:'medium'}}</p></div><div class="text-right"><div class="nv-muted text-sm">Offer</div><div class="font-black text-2xl">LKR {{r.offeredAmount|number:'1.0-0'}}</div></div></div>@if(r.status===0||r.status===3){<div class="grid md:grid-cols-[1fr_auto] gap-3 mt-5"><input class="nv-input" [(ngModel)]="messages[r.rentalRequestId]" placeholder="Optional message to organizer" aria-label="Optional message to organizer"><div class="flex flex-wrap gap-2"><button type="button" class="nv-btn nv-btn-primary" (click)="setStatus(r,1)">Accept</button><button type="button" class="nv-btn nv-btn-secondary" (click)="setStatus(r,3)">Negotiate</button><button type="button" class="nv-btn nv-btn-secondary" (click)="setStatus(r,2)">Reject</button></div></div>}@if(r.ownerMessage){<div class="nv-card-soft p-3 text-sm mt-4">{{r.ownerMessage}}</div>}</article>}</div>
    } @else {
      <app-page-state title="No rental requests" message="Incoming organizer venue-rental requests will appear here."/>
    }
    @if(message()){<div class="fixed right-4 bottom-4 nv-card px-4 py-3" role="status">{{message()}}</div>}
  </div>`
})
export class VenueOwnerRentalsComponent implements OnInit{
  private api=inject(VenuesService);
  rentals=signal<VenueRentalDto[]>([]);
  messages:Record<string,string>={};
  message=signal('');
  loading=signal(true);
  loadError=signal(false);
  retry=()=>this.load();
  ngOnInit(){this.load();}
  load(){this.loading.set(true);this.loadError.set(false);this.api.rentalsIncoming().subscribe({next:v=>{this.rentals.set(v);this.loading.set(false);},error:()=>{this.loading.set(false);this.loadError.set(true);}});}
  status(v:number){return ['Pending','Accepted','Rejected','Negotiating','Cancelled'][Number(v)]??String(v);}
  setStatus(r:VenueRentalDto,status:number){this.api.rentalStatus(r.rentalRequestId,status,this.messages[r.rentalRequestId]).subscribe({next:()=>{this.message.set('Rental request updated.');this.load();},error:e=>this.message.set(e?.error?.message||'Request could not be updated.')});}
}
