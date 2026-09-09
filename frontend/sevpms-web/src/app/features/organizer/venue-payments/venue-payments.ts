import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { catchError, forkJoin, map, of } from 'rxjs';
import { VenueRentalDto } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { LocalMediaService } from '../../../core/services/local-media.service';
import { httpErrorMessage } from '../../../core/utils/http-error';
interface RentalPayCard{rental:VenueRentalDto;venueName:string;qr:string;}
@Component({selector:'app-organizer-venue-payments',imports:[CommonModule],templateUrl:'./venue-payments.html',styleUrl:'./venue-payments.scss'})
export class OrganizerVenuePaymentsComponent implements OnInit{
 private readonly domain=inject(DomainApiService);private readonly media=inject(LocalMediaService);readonly cards=signal<RentalPayCard[]>([]);readonly loading=signal(true);readonly error=signal('');
 ngOnInit():void{this.domain.venueRentalsMine().subscribe({next:rentals=>{const accepted=rentals.filter(x=>Number(x.status)===1||String(x.status).toLowerCase().includes('accept'));if(!accepted.length){this.loading.set(false);return;}forkJoin(accepted.map(r=>this.domain.venue(r.venueId).pipe(map(v=>({r,v})),catchError(()=>of({r,v:null}))))).subscribe(rows=>{forkJoin(rows.map(row=>{const owner=row.v?.ownerUserId;if(!owner)return of({rental:row.r,venueName:row.v?.name??'Venue',qr:''});return this.media.paymentQrForUser(owner).pipe(map(q=>({rental:row.r,venueName:row.v?.name??'Venue',qr:q.url})),catchError(()=>of({rental:row.r,venueName:row.v?.name??'Venue',qr:''})));})).subscribe(cards=>{this.cards.set(cards);this.loading.set(false);});});},error:e=>{this.loading.set(false);this.error.set(httpErrorMessage(e,'Venue payments could not be loaded.'));}});}
}
