import { DatePipe } from '@angular/common';
import { Component, Input, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { EventRecommendationDto } from '../../core/models/api.models';
import { EventsService } from '../../core/services/events.service';

@Component({selector:'app-event-recommendation-card',standalone:true,imports:[DatePipe,RouterLink],template:`
<article class="nv-card overflow-hidden group h-full flex flex-col">
  <a [routerLink]="['/app/events',recommendation.eventId]" class="block relative aspect-[16/9] nv-media-shell" [attr.aria-label]="'View '+recommendation.title">
    <div class="nv-image-fallback event" [style.backgroundImage]="fallbackBackground"></div>
    @if(cover){<img [src]="cover" [alt]="recommendation.title" loading="lazy" decoding="async" class="nv-img relative z-[1]" (error)="cover='';imageError=true">}
    <div class="nv-media-vignette z-[2]"></div>
    <div class="absolute right-3 top-3 z-[3] rounded-full px-2.5 py-1 text-xs font-black text-white border border-white/15 backdrop-blur-md" style="background:rgba(15,23,56,.55)">{{recommendation.recommendationScore}} match</div>
    <div class="absolute left-3 bottom-3 z-[3] text-white font-black text-sm">{{recommendation.startAtUtc|date:'MMM d · h:mm a'}}</div>
  </a>
  <div class="p-4 flex flex-col gap-3 flex-1">
    <span class="text-xs font-black nv-muted uppercase tracking-[.09em]">{{recommendation.category||'Event'}}</span>
    <h3 class="font-black text-lg leading-tight tracking-[-.02em]">{{recommendation.title}}</h3>
    <p class="nv-muted text-sm line-clamp-2">{{recommendation.description}}</p>
    @if(recommendation.reasons.length){<div class="flex flex-wrap gap-1.5">@for(reason of recommendation.reasons.slice(0,2);track reason){<span class="nv-chip text-xs">{{reason}}</span>}</div>}
    <div class="text-sm nv-muted mt-auto">{{recommendation.startAtUtc|date:'EEE, MMM d'}}</div>
    <a class="nv-btn nv-btn-secondary w-full" [routerLink]="['/app/events',recommendation.eventId]">View Details</a>
  </div>
</article>`})
export class EventRecommendationCardComponent implements OnInit{
  private events=inject(EventsService);
  @Input({required:true}) recommendation!:EventRecommendationDto;
  cover='';imageError=false;
  get fallbackBackground(){const c=(this.recommendation.category||'').toLowerCase();const file=c.includes('concert')?'concert':c.includes('festival')?'festival':c.includes('sport')?'sports':c.includes('conference')?'conference':c.includes('theatre')||c.includes('theater')?'theatre':c.includes('family')?'family-event':c.includes('wedding')?'wedding-event':'event-default';return `linear-gradient(135deg,rgba(15,23,56,.10),rgba(15,23,56,.58)),url('assets/nvent/images/events/${file}.webp'),linear-gradient(135deg,#92ABD3,#E49C95,#FAC098)`;}
  ngOnInit(){this.events.cover(this.recommendation.eventId).subscribe({next:v=>this.cover=v.url,error:()=>{this.cover='';}});}
}
