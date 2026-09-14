import { Component, Input, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { VenueDto } from '../../core/models/api.models';
import { VenuesService } from '../../core/services/venues.service';

@Component({selector:'app-venue-card',standalone:true,imports:[RouterLink],template:`
<article class="nv-card overflow-hidden h-full group flex flex-col">
  <a [routerLink]="['/app/venues',venue.venueId]" class="block relative aspect-[16/9] nv-media-shell" [attr.aria-label]="'View '+venue.name">
    <div class="nv-image-fallback venue"></div>
    @if(imageUrl){<img [src]="imageUrl" [alt]="venue.name" loading="lazy" decoding="async" class="nv-img relative z-[1]" (error)="imageUrl=''">}
    <div class="nv-media-vignette z-[2]"></div>
    <div class="absolute left-3 right-3 bottom-3 z-[3] flex items-end justify-between gap-3 text-white"><div><div class="text-[11px] uppercase tracking-[.13em] font-black text-white/70">Venue</div><div class="font-black">{{venue.city}}</div></div><span class="px-2.5 py-1 rounded-full bg-black/25 border border-white/15 text-xs font-black backdrop-blur-md">{{venue.capacity}} guests</span></div>
  </a>
  <div class="p-5 flex-1 flex flex-col">
    <h2 class="font-black text-lg tracking-[-.02em]">{{venue.name}}</h2>
    <p class="nv-muted text-sm mt-1">{{venue.city}}, {{venue.district}}</p>
    <p class="nv-muted text-sm mt-3 line-clamp-2 flex-1">{{venue.description||'Venue details and facilities are available on the venue page.'}}</p>
    <a [routerLink]="['/app/venues',venue.venueId]" class="nv-btn nv-btn-secondary w-full mt-4">View Venue</a>
  </div>
</article>`})
export class VenueCardComponent implements OnInit{
  private venues=inject(VenuesService);
  @Input({required:true}) venue!:VenueDto;
  imageUrl='';
  ngOnInit(){this.venues.marketplace(this.venue.venueId).subscribe({next:m=>{const photo=[...m.media].sort((a,b)=>a.sortOrder-b.sortOrder).find(x=>x.type.toLowerCase()==='photo');this.imageUrl=photo?.url??'';},error:()=>void 0});}
}
