import { Component, Input } from '@angular/core';
import { NearbyPlace } from '../../core/models/api.models';

@Component({selector:'app-place-card',standalone:true,template:`
<article class="nv-card overflow-hidden h-full flex flex-col group">
  <div class="aspect-[16/8] relative nv-media-shell">
    <div class="nv-image-fallback place" [style.backgroundImage]="fallbackBackground"></div>
    <div class="nv-media-vignette"></div>
    <div class="absolute left-4 right-4 bottom-4 z-[2] text-white flex items-end justify-between gap-3"><div><span class="text-[11px] uppercase tracking-[.13em] font-black text-white/70">{{place.category}}</span><h2 class="font-black text-lg mt-1">{{place.name}}</h2></div><strong class="text-sm whitespace-nowrap">{{place.distanceKm}} km</strong></div>
  </div>
  <div class="p-5 flex-1 flex flex-col">
    <p class="nv-muted text-sm">{{place.address}}</p>
    @if(place.recommendationReason){<p class="text-sm mt-3">{{place.recommendationReason}}</p>}
    <div class="flex flex-wrap gap-2 mt-3">@for(tag of place.tags;track tag){<span class="nv-status">{{tag}}</span>}@for(audience of place.audienceModes;track audience){<span class="nv-status">{{audience}}</span>}</div>
    <div class="flex gap-2 mt-auto pt-5"><a [href]="directionsUrl" target="_blank" rel="noopener" class="nv-btn nv-btn-primary flex-1">{{place.directionsUrl?'Get Directions':'Open Map'}}</a><span class="nv-btn nv-btn-secondary" [attr.aria-label]="place.isOpen?'Place is open':'Place is closed'">{{place.isOpen?'Open':'Closed'}}</span></div>
  </div>
</article>`})
export class PlaceCardComponent{
  @Input({required:true}) place!:NearbyPlace;
  get directionsUrl(){return this.place.directionsUrl||`https://www.openstreetmap.org/?mlat=${this.place.latitude}&mlon=${this.place.longitude}`;}
  get fallbackBackground(){const c=(this.place.category||'').toLowerCase();const file=c.includes('beach')?'beach':c.includes('cafe')?'cafe':c.includes('museum')?'museum':c.includes('park')?'park':c.includes('restaurant')?'romantic-restaurant':c.includes('adventure')?'adventure':c.includes('night')?'night-activity':c.includes('view')||c.includes('scenic')?'viewpoint':'places-default';return `linear-gradient(180deg,rgba(15,23,56,.04),rgba(15,23,56,.40)),url('assets/nvent/images/places/${file}.webp'),linear-gradient(135deg,#80D5CA,#92ABD3,#665A88)`;}
}
