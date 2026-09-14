import { DatePipe } from '@angular/common';
import { Component, Input, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { EventDto } from '../../core/models/api.models';
import { EventsService } from '../../core/services/events.service';
import { StatusBadgeComponent } from './status-badge.component';

@Component({selector:'app-event-card',standalone:true,imports:[DatePipe,RouterLink,StatusBadgeComponent],template:`
<article class="nv-card overflow-hidden group h-full flex flex-col">
  <a [routerLink]="['/app/events',event.eventId]" class="block relative aspect-[16/9] nv-media-shell" [attr.aria-label]="'View '+event.title">
    <div class="nv-image-fallback event" [style.backgroundImage]="fallbackBackground"></div>
    @if(cover){<img [src]="cover" [alt]="event.title" loading="lazy" decoding="async" class="nv-img relative z-[1]" (error)="cover='';imageError=true">}
    <div class="nv-media-vignette z-[2]"></div>
    <div class="absolute inset-x-0 top-0 z-[3] p-3 flex items-start justify-between gap-2">
      <span class="px-2.5 py-1 rounded-full bg-black/30 border border-white/15 text-white text-[11px] font-black backdrop-blur-md">{{event.category||'Event'}}</span>
      <app-status-badge [label]="status" [tone]="statusTone"/>
    </div>
    <div class="absolute left-3 bottom-3 z-[3] text-white"><div class="text-[11px] uppercase tracking-[.14em] font-black text-white/70">Starts</div><div class="font-black text-sm mt-0.5">{{event.startAtUtc|date:'MMM d · h:mm a'}}</div></div>
  </a>
  <div class="p-4 flex flex-col gap-3 flex-1">
    <h3 class="font-black text-lg leading-tight tracking-[-.02em]">{{event.title}}</h3>
    <p class="nv-muted text-sm line-clamp-2 flex-1">{{event.description||'Event details are available on the event page.'}}</p>
    <div class="flex items-center justify-between gap-3 pt-1"><span class="text-xs font-bold nv-muted">{{event.startAtUtc|date:'EEE, MMM d'}}</span><a class="font-black text-sm" [routerLink]="['/app/events',event.eventId]">View details →</a></div>
  </div>
</article>`})
export class EventCardComponent implements OnInit{
  private events=inject(EventsService);
  @Input({required:true})event!:EventDto;
  cover='';imageError=false;
  get status(){
    if(this.event.status===0)return 'Draft';
    if(this.event.status===2)return 'Cancelled';
    if(this.event.status===3)return 'Completed';
    const now=Date.now(),start=Date.parse(this.event.startAtUtc),end=Date.parse(this.event.endAtUtc);
    if(Number.isFinite(start)&&Number.isFinite(end)&&start<=now&&now<end)return 'Ongoing';
    if(Number.isFinite(end)&&end<=now)return 'Past';
    return 'Upcoming';
  }
  get statusTone(){return this.status==='Cancelled'?'danger':this.status==='Ongoing'?'info':this.status==='Upcoming'?'success':'neutral';}
  get fallbackBackground(){
    const c=(this.event.category||'').toLowerCase();
    const file=c.includes('concert')?'concert':c.includes('festival')?'festival':c.includes('sport')?'sports':c.includes('conference')?'conference':c.includes('theatre')||c.includes('theater')?'theatre':c.includes('family')?'family-event':c.includes('wedding')?'wedding-event':'event-default';
    return `linear-gradient(135deg,rgba(15,23,56,.10),rgba(15,23,56,.58)),url('assets/nvent/images/events/${file}.webp'),linear-gradient(135deg,#92ABD3,#E49C95,#FAC098)`;
  }
  ngOnInit(){this.events.cover(this.event.eventId).subscribe({next:v=>this.cover=v.url,error:()=>{this.cover='';}});}
}
