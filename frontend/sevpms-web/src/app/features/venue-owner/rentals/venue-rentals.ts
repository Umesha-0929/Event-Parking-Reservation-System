import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { VenueRentalDto, VenueSummary } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';
import { NavIconComponent } from '../../../shared/components/nav-icon/nav-icon';

const ARCHIVE_KEY='nvent.venue-owner.archived-rentals';
@Component({selector:'app-venue-rentals',imports:[CommonModule,FormsModule,NavIconComponent],templateUrl:'./venue-rentals.html',styleUrl:'./venue-rentals.scss'})
export class VenueRentalsComponent implements OnInit{
 private readonly domain=inject(DomainApiService);readonly rentals=signal<VenueRentalDto[]>([]);readonly venues=signal<VenueSummary[]>([]);readonly loading=signal(true);readonly error=signal('');readonly success=signal('');readonly busy=signal<string|null>(null);readonly archivedIds=signal<Set<string>>(this.readArchived());
 query='';status='all';ownerMessage:Record<string,string>={};
 ngOnInit():void{this.load();}
 load():void{this.loading.set(true);this.error.set('');this.domain.myVenues().subscribe({next:v=>{this.venues.set(v);this.domain.venueRentalsIncoming().subscribe({next:r=>{this.rentals.set(r);this.loading.set(false);},error:e=>{this.loading.set(false);this.error.set(httpErrorMessage(e,'Rental requests could not be loaded.'));}});},error:e=>{this.loading.set(false);this.error.set(httpErrorMessage(e,'Your venues could not be loaded.'));}});}
 venueName(id:string):string{return this.venues().find(v=>(v.venueId??v.id)===id)?.name??'Venue';}
 statusLabel(v:number|string):string{const n=typeof v==='number'?v:Number(v);return ['Pending','Accepted','Rejected','Negotiating','Cancelled'][n]??String(v);}
 isArchived(r:VenueRentalDto):boolean{return this.archivedIds().has(r.rentalRequestId);}
 filtered():VenueRentalDto[]{const q=this.query.trim().toLowerCase();return this.rentals().filter(r=>{const archived=this.isArchived(r);if(this.status==='archived')return archived&&(!q||this.matches(r,q));if(archived)return false;return(this.status==='all'||this.statusLabel(r.status).toLowerCase()===this.status)&&(!q||this.matches(r,q));});}
 update(r:VenueRentalDto,status:1|2|3):void{if(this.busy())return;this.busy.set(r.rentalRequestId);this.error.set('');this.success.set('');this.domain.updateVenueRentalStatus(r.rentalRequestId,status,this.ownerMessage[r.rentalRequestId]||null).subscribe({next:()=>{this.busy.set(null);this.success.set(status===1?'Rental request accepted.':status===2?'Rental request rejected.':'Negotiation response sent.');this.load();},error:e=>{this.busy.set(null);this.error.set(httpErrorMessage(e,'Rental request could not be updated.'));}});}
 canArchive(r:VenueRentalDto):boolean{return !['Pending','Negotiating'].includes(this.statusLabel(r.status));}
 archive(r:VenueRentalDto):void{if(!this.canArchive(r)||!confirm('Clear this resolved rental from the normal list? Its server record and history will be kept.'))return;const next=new Set(this.archivedIds());next.add(r.rentalRequestId);this.archivedIds.set(next);this.persist(next);this.success.set('Rental cleared from the normal list.');}
 restore(r:VenueRentalDto):void{const next=new Set(this.archivedIds());next.delete(r.rentalRequestId);this.archivedIds.set(next);this.persist(next);this.success.set('Rental restored.');}
 clearResolved():void{const resolved=this.rentals().filter(r=>this.canArchive(r)&&!this.isArchived(r));if(!resolved.length){this.success.set('There are no resolved rental requests to clear.');return;}if(!confirm(`Clear ${resolved.length} resolved rental request${resolved.length===1?'':'s'} from the normal list?`))return;const next=new Set(this.archivedIds());resolved.forEach(r=>next.add(r.rentalRequestId));this.archivedIds.set(next);this.persist(next);this.success.set(`${resolved.length} resolved rental request${resolved.length===1?'':'s'} cleared from the normal list.`);}
 private matches(r:VenueRentalDto,q:string):boolean{return `${r.purpose} ${this.venueName(r.venueId)} ${r.offeredAmount} ${this.statusLabel(r.status)}`.toLowerCase().includes(q);}
 private readArchived():Set<string>{if(typeof localStorage==='undefined')return new Set();try{return new Set(JSON.parse(localStorage.getItem(ARCHIVE_KEY)??'[]') as string[]);}catch{return new Set();}}
 private persist(ids:Set<string>):void{if(typeof localStorage!=='undefined')localStorage.setItem(ARCHIVE_KEY,JSON.stringify([...ids]));}
}
