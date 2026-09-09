import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { catchError, of } from 'rxjs';
import { EventSummary, VenueMarketplaceDto, VenueSummary } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { LocalMediaService } from '../../../core/services/local-media.service';
import { SeoService } from '../../../core/services/seo.service';
import { httpErrorMessage } from '../../../core/utils/http-error';
import { RevealOnScrollDirective } from '../../../shared/directives/reveal-on-scroll.directive';
import { LocationMapComponent, PlatformMapPin } from '../../../shared/components/location-map/location-map';

@Component({selector:'app-detail',imports:[CommonModule,RouterLink,RevealOnScrollDirective,LocationMapComponent],templateUrl:'./detail.html',styleUrl:'./detail.scss'})
export class DetailComponent implements OnInit{
 private readonly route=inject(ActivatedRoute);private readonly seo=inject(SeoService);private readonly domain=inject(DomainApiService);private readonly media=inject(LocalMediaService);
 readonly kind=(this.route.snapshot.data['kind']??'event') as 'event'|'venue';readonly backLink=(this.route.snapshot.data['backLink'] as string|undefined)??(this.kind==='event'?'/events':'/venues');readonly eventsLink=(this.route.snapshot.data['eventsLink'] as string|undefined)??'/events';readonly id=this.route.snapshot.paramMap.get('slug')??'';
 readonly loading=signal(true);readonly error=signal('');readonly event=signal<EventSummary|null>(null);readonly venue=signal<VenueSummary|null>(null);readonly marketplace=signal<VenueMarketplaceDto|null>(null);readonly cover=signal('');
 ngOnInit():void{if(!this.id){this.error.set('No item identifier was supplied.');this.loading.set(false);return;}if(this.kind==='event'){this.domain.event(this.id).subscribe({next:e=>{this.event.set(e);this.loading.set(false);this.seo.setPage(`${e.title??e.name??'Event'} | Nvent`,e.description??'Nvent event details');this.media.eventCover(this.id).pipe(catchError(()=>of(null))).subscribe(x=>this.cover.set(x?.url??''));if(e.venueId)this.domain.venue(e.venueId).pipe(catchError(()=>of(null))).subscribe(v=>this.venue.set(v));},error:e=>{this.error.set(httpErrorMessage(e,'Event details could not be loaded.'));this.loading.set(false);}});}else{this.domain.venue(this.id).subscribe({next:v=>{this.venue.set(v);this.loading.set(false);this.seo.setPage(`${v.name} | Nvent`,v.description??'Nvent venue details');this.domain.venueMarketplace(this.id).subscribe({next:m=>{this.marketplace.set(m);const first=[...(m.media??[])].filter(x=>x.type?.toLowerCase()==='photo'||/\.(png|jpe?g|webp|gif)(\?|$)/i.test(x.url??'')).sort((a,b)=>a.sortOrder-b.sortOrder)[0];this.cover.set(first?.url??'');},error:()=>this.marketplace.set(null)});},error:e=>{this.error.set(httpErrorMessage(e,'Venue details could not be loaded.'));this.loading.set(false);}});}}
 get title():string{return this.kind==='event'?(this.event()?.title??this.event()?.name??'Event'):(this.venue()?.name??'Venue');}
 get description():string{return this.kind==='event'?(this.event()?.description??'View event information and continue to ticket selection.'):(this.venue()?.description??'View venue information and upcoming events.');}
 statusText(value:unknown):string{const n=Number(value);if(n===0)return'Draft';if(n===1)return'Published';if(n===2)return'Cancelled';if(n===3)return'Completed';const s=String(value??'').trim();return s||'Available';}
 mapPins():PlatformMapPin[]{const v=this.venue();if(!v||v.latitude==null||v.longitude==null)return[];return[{id:v.venueId??v.id??'venue',name:v.name,latitude:Number(v.latitude),longitude:Number(v.longitude),kind:'venue',subtitle:v.city}];}
}
