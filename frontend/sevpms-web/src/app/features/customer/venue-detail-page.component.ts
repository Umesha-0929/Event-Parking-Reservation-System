import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DecimalPipe } from '@angular/common';
import { catchError, forkJoin, of } from 'rxjs';
import { VenuesService } from '../../core/services/venues.service';
import { EventsService } from '../../core/services/events.service';
import { EventDto, VenueDto, VenueMarketplace } from '../../core/models/api.models';
import { EventCardComponent } from '../../shared/components/event-card.component';
import { PageStateComponent } from '../../shared/components/page-state.component';

@Component({
  selector:'app-venue-detail-page',
  standalone:true,
  imports:[RouterLink,DecimalPipe,EventCardComponent,PageStateComponent],
  template:`
  <div class="nv-page space-y-6">
    @if(loading()){<div class="nv-skeleton h-80"></div>}
    @else if(!venue()){<app-page-state kind="error" title="Venue unavailable" message="This venue could not be loaded."/>}
    @else{
      <section class="nv-card overflow-hidden">
        <div class="relative min-h-[300px] sm:min-h-[390px]">
          @if(heroMedia){<img [src]="heroMedia" [alt]="venue()!.name" class="absolute inset-0 w-full h-full object-cover" (error)="heroMedia=''">}
          @if(!heroMedia){<div class="absolute inset-0 nv-image-fallback venue"></div>}
          <div class="absolute inset-0" style="background:linear-gradient(180deg,rgba(15,23,56,.08),rgba(15,23,56,.82))"></div>
          <div class="relative z-10 min-h-[300px] sm:min-h-[390px] p-6 sm:p-9 flex flex-col justify-end text-white">
            <p class="font-extrabold opacity-80">Venue</p><h1 class="text-3xl sm:text-5xl font-black tracking-[-.045em] mt-1">{{venue()!.name}}</h1><p class="mt-2 text-white/80">{{venue()!.addressLine1}}, {{venue()!.city}}, {{venue()!.district}}</p>
            <div class="flex flex-wrap gap-3 mt-6"><a routerLink="/app/parking" [queryParams]="{venue:venue()!.venueId}" class="nv-btn nv-btn-primary">Find Parking</a><a routerLink="/app/places" [queryParams]="{venue:venue()!.venueId}" class="nv-btn nv-btn-secondary">Explore Places</a></div>
          </div>
        </div>
      </section>

      <section class="grid lg:grid-cols-[1fr_340px] gap-5">
        <div class="space-y-5">
          <article class="nv-card p-6"><h2 class="text-xl font-black">Venue details</h2><p class="nv-muted mt-3">{{venue()!.description}}</p><div class="grid sm:grid-cols-2 gap-3 mt-5"><div class="nv-card-soft p-4"><div class="nv-muted text-sm">Capacity</div><strong>{{venue()!.capacity}}</strong></div><div class="nv-card-soft p-4"><div class="nv-muted text-sm">Country</div><strong>{{venue()!.country}}</strong></div></div></article>
          @if(marketplace()?.facilities?.length){<article class="nv-card p-6"><h2 class="text-xl font-black">Facilities</h2><p class="nv-muted text-sm mt-1">Venue services configured by the venue owner.</p><div class="grid sm:grid-cols-2 lg:grid-cols-3 gap-2 mt-4">@for(f of marketplace()!.facilities;track f.facilityId){<div class="nv-facility-pill"><span class="nv-facility-icon" [style.backgroundImage]="facilityBackground(f.name)" style="background-size:cover;background-position:center">•</span><span class="font-extrabold text-sm">{{f.name}}</span></div>}</div></article>}
          @if(marketplace()?.media?.length){<article class="nv-card p-6"><h2 class="text-xl font-black">Gallery</h2><div class="grid sm:grid-cols-2 lg:grid-cols-3 gap-3 mt-4">@for(m of photoMedia;track m.venueMediaId){<img [src]="m.url" [alt]="venue()!.name+' venue photo'" loading="lazy" class="w-full aspect-[4/3] object-cover rounded-2xl">}</div></article>}
        </div>
        <aside class="space-y-4">
          <article class="nv-card p-5"><h2 class="font-black">Contact</h2><p class="nv-muted text-sm mt-3">{{venue()!.contactPhone||'Phone not provided'}}</p><p class="nv-muted text-sm mt-1">{{venue()!.contactEmail||'Email not provided'}}</p>@if(venue()!.latitude!=null&&venue()!.longitude!=null){<a class="nv-btn nv-btn-secondary w-full mt-5" [href]="mapUrl" target="_blank" rel="noopener">Open Map</a>}</article>
          @if(marketplace()?.rates?.length){<article class="nv-card p-5"><h2 class="font-black">Venue rates</h2><div class="space-y-3 mt-4">@for(r of marketplace()!.rates;track r.venueRateId){<div class="nv-card-soft p-3 flex justify-between gap-3"><span>{{r.rateType}}</span><strong>{{r.currency}} {{r.amount|number:'1.0-2'}}</strong></div>}</div></article>}
        </aside>
      </section>

      <section><h2 class="text-xl font-black mb-4">Events at this venue</h2>@if(events().length){<div class="nv-grid-cards">@for(e of events();track e.eventId){<app-event-card [event]="e"/>}</div>}@else{<app-page-state title="No published events" message="There are no published events for this venue right now."/>}</section>
    }
  </div>`
})
export class VenueDetailPageComponent implements OnInit{
  private route=inject(ActivatedRoute);private venuesApi=inject(VenuesService);private eventsApi=inject(EventsService);
  venue=signal<VenueDto|null>(null);marketplace=signal<VenueMarketplace|null>(null);events=signal<EventDto[]>([]);loading=signal(true);heroMedia='';
  get photoMedia(){return (this.marketplace()?.media??[]).filter(x=>x.type.toLowerCase()==='photo').sort((a,b)=>a.sortOrder-b.sortOrder);}
  get mapUrl(){const v=this.venue();return v&&v.latitude!=null&&v.longitude!=null?`https://www.openstreetmap.org/?mlat=${v.latitude}&mlon=${v.longitude}`:'#';}
  facilityBackground(name:string){const n=name.toLowerCase();const file=n.includes('park')?'parking':n.includes('food')||n.includes('cater')?'food':n.includes('wash')||n.includes('restroom')?'washroom':n.includes('security')?'security':n.includes('access')?'accessibility':n.includes('wifi')||n.includes('wi-fi')?'wifi':'facility';return `linear-gradient(145deg,rgba(128,213,202,.16),rgba(146,171,211,.10)),url('assets/nvent/3d/facilities/${file}.webp')`; }
  ngOnInit(){const id=this.route.snapshot.paramMap.get('id')!;forkJoin({venue:this.venuesApi.get(id),marketplace:this.venuesApi.marketplace(id).pipe(catchError(()=>of(null))),events:this.eventsApi.list({venue:id,page:1,pageSize:20}).pipe(catchError(()=>of([]))) }).subscribe({next:v=>{this.venue.set(v.venue);this.marketplace.set(v.marketplace);this.events.set(v.events);this.heroMedia=(v.marketplace?.media??[]).filter(x=>x.type.toLowerCase()==='photo').sort((a,b)=>a.sortOrder-b.sortOrder)[0]?.url??'';this.loading.set(false);},error:()=>this.loading.set(false)});}
}
