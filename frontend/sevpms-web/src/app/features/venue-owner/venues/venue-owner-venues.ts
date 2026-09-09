import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { catchError, forkJoin, map, of } from 'rxjs';
import { VenueSummary } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';
import { NavIconComponent } from '../../../shared/components/nav-icon/nav-icon';

@Component({selector:'app-venue-owner-venues',imports:[RouterLink,NavIconComponent],templateUrl:'./venue-owner-venues.html',styleUrl:'./venue-owner-venues.scss'})
export class VenueOwnerVenuesComponent implements OnInit{
  private readonly domain=inject(DomainApiService);readonly venues=signal<VenueSummary[]>([]);readonly loading=signal(true);readonly error=signal('');readonly actionId=signal<string|null>(null);readonly coverImages=signal<Record<string,string>>({});
  ngOnInit():void{this.load();}
  load():void{this.loading.set(true);this.error.set('');this.domain.myVenues().subscribe({next:(items)=>{this.venues.set(items);this.loading.set(false);this.loadImages(items);},error:(error)=>{this.error.set(httpErrorMessage(error,'Your venues could not be loaded.'));this.loading.set(false);}});}
  deletePermanent(venue:VenueSummary):void{const id=this.venueId(venue);if(!id||venue.isActive!==false||this.actionId())return;if(typeof window!=='undefined'&&!window.confirm(`Delete inactive venue “${venue.name}” permanently? This only works when it is not referenced by event or rental history.`))return;this.actionId.set(id);this.domain.deleteVenuePermanent(id).subscribe({next:()=>{this.actionId.set(null);this.load();},error:(error)=>{this.actionId.set(null);this.error.set(httpErrorMessage(error,'The inactive venue could not be permanently deleted.'));}});}
  deactivate(venue:VenueSummary):void{const id=this.venueId(venue);if(!id||this.actionId())return;if(typeof window!=='undefined'&&!window.confirm(`Deactivate “${venue.name}”? It will no longer appear as an active venue.`))return;this.actionId.set(id);this.domain.deactivateVenue(id).subscribe({next:()=>{this.actionId.set(null);this.load();},error:(error)=>{this.actionId.set(null);this.error.set(httpErrorMessage(error,'The venue could not be deactivated.'));}});}
  venueId(venue:VenueSummary):string{return venue.venueId??venue.id??'';}
  coverImage(venue:VenueSummary):string{return this.coverImages()[this.venueId(venue)]??'';}
  private loadImages(venues:VenueSummary[]):void{const requests=venues.map((venue)=>{const id=this.venueId(venue);if(!id)return of({id:'',image:''});return this.domain.venueMarketplace(id).pipe(map((marketplace)=>{const media=[...(marketplace.media??[])].filter(x=>x.type?.toLowerCase()==='photo'||/\.(png|jpe?g|webp|gif)(\?|$)/i.test(x.url??'')).sort((a,b)=>Number(a.sortOrder??0)-Number(b.sortOrder??0));return{id,image:media[0]?.url??''};}),catchError(()=>of({id,image:''})));});if(!requests.length)return;forkJoin(requests).subscribe(results=>{const next:Record<string,string>={};for(const r of results)if(r.id&&r.image)next[r.id]=r.image;this.coverImages.set(next);});}
}
