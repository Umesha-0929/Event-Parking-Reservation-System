import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject, debounceTime, takeUntil } from 'rxjs';
import { EventsService } from '../../core/services/events.service';
import { VenuesService } from '../../core/services/venues.service';
import { EventCategory, EventDto, VenueDto } from '../../core/models/api.models';
import { EventCardComponent } from '../../shared/components/event-card.component';
import { LoadingGridComponent } from '../../shared/components/loading-grid.component';
import { PageStateComponent } from '../../shared/components/page-state.component';

@Component({
  selector:'app-events-page',
  standalone:true,
  imports:[FormsModule,EventCardComponent,LoadingGridComponent,PageStateComponent],
  template:`
  <div class="nv-page space-y-6">
    <section class="nv-hero nv-hero-media min-h-[260px] text-white">
      <video class="nv-section-video" autoplay muted loop playsinline preload="metadata" poster="assets/nvent/images/hero/events-hero.jpg"><source src="assets/nvent/videos/events/event-showcase.mp4" type="video/mp4"></video>
      <div class="nv-section-video-overlay"></div>
      <div class="relative z-10 max-w-2xl">
        <p class="text-sm font-extrabold opacity-75">Event discovery</p>
        <h1 class="nv-page-title mt-1">Events</h1>
        <p class="mt-2 opacity-80">Search published events by name, category, venue or date.</p>
      </div>
    </section>

    <section class="nv-card p-4 space-y-3">
      <div class="grid md:grid-cols-2 xl:grid-cols-[minmax(260px,1fr)_220px_190px_auto] gap-3">
        <label class="space-y-1">
          <span class="sr-only">Search events</span>
          <input class="nv-input" [(ngModel)]="search" (ngModelChange)="searchChanged.next($event)" placeholder="Search events" aria-label="Search events">
        </label>
        <label class="space-y-1">
          <span class="sr-only">Venue</span>
          <select class="nv-input" [(ngModel)]="venueId" (ngModelChange)="load()" aria-label="Venue  id">
            <option value="">All venues</option>
            @for(v of venues();track v.venueId){<option [value]="v.venueId">{{v.name}}</option>}
          </select>
        </label>
        <label class="space-y-1">
          <span class="sr-only">Event date</span>
          <input class="nv-input" type="date" [(ngModel)]="date" (ngModelChange)="load()" aria-label="Date">
        </label>
        <button type="button" class="nv-btn nv-btn-secondary" (click)="clear()">Clear Filters</button>
      </div>
      <div class="flex gap-2 overflow-auto pt-1 pb-1" aria-label="Event categories">
        <button type="button" class="nv-chip" [class.active]="!categoryId" (click)="categoryId='';load()">All</button>
        @for(c of categories();track c.eventCategoryId){
          <button type="button" class="nv-chip whitespace-nowrap" [class.active]="categoryId===c.eventCategoryId" (click)="categoryId=c.eventCategoryId;load()">{{c.name}}</button>
        }
      </div>
    </section>

    @if(loading()){
      <app-loading-grid/>
    }@else if(error()){
      <app-page-state kind="error" title="We couldn't load events" message="Please check your connection and try again." actionLabel="Retry" [action]="retry"/>
    }@else if(events().length){
      <div class="flex justify-between items-center"><p class="nv-muted text-sm">{{events().length}} event{{events().length===1?'':'s'}}</p></div>
      <div class="nv-grid-cards">@for(event of events();track event.eventId){<app-event-card [event]="event"/>}</div>
    }@else{
      <app-page-state title="No events match your filters" message="Try a different search, date, venue or category." actionLabel="Clear Filters" [action]="clearAction"/>
    }
  </div>`
})
export class EventsPageComponent implements OnInit,OnDestroy{
  private api=inject(EventsService);
  private venuesApi=inject(VenuesService);
  private route=inject(ActivatedRoute);
  private destroy$=new Subject<void>();
  events=signal<EventDto[]>([]);
  categories=signal<EventCategory[]>([]);
  venues=signal<VenueDto[]>([]);
  loading=signal(true);
  error=signal(false);
  search='';
  categoryId='';
  venueId='';
  date='';
  searchChanged=new Subject<string>();
  retry=()=>this.load();
  clearAction=()=>this.clear();

  ngOnInit(){
    this.route.queryParamMap.pipe(takeUntil(this.destroy$)).subscribe(params=>{
      this.search=params.get('q')??'';
      this.categoryId=params.get('categoryId')??'';
      this.venueId=params.get('venue')??'';
      this.date=params.get('date')??'';
      this.load();
    });
    this.api.categories().subscribe({next:v=>this.categories.set(v.filter(x=>x.isActive)),error:()=>void 0});
    this.venuesApi.list(1,100).subscribe({next:v=>this.venues.set(v.filter(x=>x.isActive)),error:()=>void 0});
    this.searchChanged.pipe(debounceTime(350),takeUntil(this.destroy$)).subscribe(()=>this.load());
  }

  ngOnDestroy(){this.destroy$.next();this.destroy$.complete();}

  load(){
    this.loading.set(true);
    this.error.set(false);
    this.api.list({
      search:this.search||undefined,
      categoryId:this.categoryId||undefined,
      venue:this.venueId||undefined,
      date:this.date||undefined,
      page:1,
      pageSize:80
    }).subscribe({
      next:v=>{this.events.set(v);this.loading.set(false);},
      error:()=>{this.error.set(true);this.loading.set(false);}
    });
  }

  clear(){this.search='';this.categoryId='';this.venueId='';this.date='';this.load();}
}
