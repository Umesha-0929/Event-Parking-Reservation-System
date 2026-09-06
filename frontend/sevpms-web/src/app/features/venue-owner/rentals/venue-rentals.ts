
import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { VenueRentalDto, VenueSummary } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';

@Component({
 selector:'app-venue-rentals', imports:[CommonModule,FormsModule], templateUrl:'./venue-rentals.html', styleUrl:'./venue-rentals.scss'
})
export class VenueRentalsComponent implements OnInit{
 private readonly domain=inject(DomainApiService);
 readonly rentals=signal<VenueRentalDto[]>([]); readonly venues=signal<VenueSummary[]>([]); readonly loading=signal(true); readonly error=signal(''); readonly busy=signal<string|null>(null);
 query=''; status='all'; ownerMessage:Record<string,string>={};
 ngOnInit():void{this.load();}
 load():void{this.loading.set(true);this.error.set('');this.domain.myVenues().subscribe({next:v=>{this.venues.set(v);this.domain.venueRentalsIncoming().subscribe({next:r=>{this.rentals.set(r);this.loading.set(false);},error:e=>{this.loading.set(false);this.error.set(httpErrorMessage(e,'Rental requests could not be loaded.'));}});},error:e=>{this.loading.set(false);this.error.set(httpErrorMessage(e,'Your venues could not be loaded.'));}});}
 venueName(id:string):string{return this.venues().find(v=>(v.venueId??v.id)===id)?.name??'Venue';}
 statusLabel(v:number|string):string{const n=typeof v==='number'?v:Number(v);return ['Pending','Accepted','Rejected','Negotiating','Cancelled'][n]??String(v);}
 filtered():VenueRentalDto[]{const q=this.query.trim().toLowerCase();return this.rentals().filter(r=>(this.status==='all'||this.statusLabel(r.status).toLowerCase()===this.status)&&(!q||`${r.purpose} ${this.venueName(r.venueId)} ${r.offeredAmount}`.toLowerCase().includes(q)));}
 update(r:VenueRentalDto,status:1|2|3):void{if(this.busy())return;this.busy.set(r.rentalRequestId);this.error.set('');this.domain.updateVenueRentalStatus(r.rentalRequestId,status,this.ownerMessage[r.rentalRequestId]||null).subscribe({next:()=>{this.busy.set(null);this.load();},error:e=>{this.busy.set(null);this.error.set(httpErrorMessage(e,'Rental request could not be updated.'));}});}
}
