import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { catchError, forkJoin, of } from 'rxjs';
import { EventsService } from '../../core/services/events.service';
import { VenuesService } from '../../core/services/venues.service';
import { AuthService } from '../../core/services/auth.service';
import { EventDto, EventRecommendationDto, VenueDto } from '../../core/models/api.models';
import { EventCardComponent } from '../../shared/components/event-card.component';
import { EventRecommendationCardComponent } from '../../shared/components/event-recommendation-card.component';
import { LoadingGridComponent } from '../../shared/components/loading-grid.component';
import { PageStateComponent } from '../../shared/components/page-state.component';
import { VenueCardComponent } from '../../shared/components/venue-card.component';

@Component({
  selector:'app-home-page',
  standalone:true,
  imports:[FormsModule,RouterLink,EventCardComponent,EventRecommendationCardComponent,VenueCardComponent,LoadingGridComponent,PageStateComponent],
  template:`
  <div class="nv-page space-y-8">
    <section class="nv-hero-premium">
      @if(!heroVideoFailed){<video class="nv-hero-video" autoplay muted loop playsinline preload="metadata" poster="assets/nvent/images/hero/home-hero.jpg" (error)="heroVideoFailed=true"><source src="assets/nvent/videos/hero/home-hero.mp4" type="video/mp4"></video>}
      <div class="nv-hero-video-overlay"></div>
      <div class="relative z-10 max-w-3xl">
        <p class="font-extrabold text-sm opacity-75">{{auth.isAuthenticated()?'Welcome back, '+auth.displayName():'Discover Nvent'}}</p>
        <h1 class="text-4xl sm:text-6xl font-black tracking-[-.055em] mt-2 leading-[.98]">Your whole event day, connected.</h1>
        <p class="mt-4 max-w-2xl opacity-80">Discover events and venues, choose your seat, plan parking, order food and explore nearby places without breaking your flow.</p>
        <form class="mt-6 max-w-2xl flex flex-col sm:flex-row gap-2" (submit)="searchEvents();$event.preventDefault()">
          <label class="flex-1"><span class="sr-only">Search events</span><input class="nv-input" name="homeSearch" [(ngModel)]="search" placeholder="Search events" aria-label="Search events"></label>
          <button type="submit" class="nv-btn nv-btn-primary">Search</button>
        </form>
        <div class="flex flex-wrap gap-3 mt-4"><a routerLink="/app/events" class="nv-btn nv-btn-secondary">Explore Events</a><a routerLink="/app/venues" class="nv-btn nv-btn-secondary">Explore Venues</a></div>
      </div>
    </section>

    <section>
      <div class="flex items-end justify-between gap-4 mb-4"><div><h2 class="text-xl font-black">Quick services</h2><p class="nv-muted text-sm">Jump straight to the part of your event day you need.</p></div></div>
      <div class="grid grid-cols-2 md:grid-cols-3 xl:grid-cols-6 gap-3">
        @for(q of quick;track q.label){<a [routerLink]="q.path" class="nv-card-soft nv-service-tile p-4 min-h-32 flex flex-col justify-between"><span class="nv-service-orb" aria-hidden="true">@if(q.asset){<img [src]="q.asset" alt="" class="w-10 h-10 object-contain" (error)="q.asset=''">}@else{<span>{{q.icon}}</span>}</span><div><span class="font-black block">{{q.label}}</span><span class="text-[11px] nv-muted mt-1 block">{{q.note}}</span></div></a>}
      </div>
    </section>

    @if(isCustomer() && recommendations().length){
      <section>
        <div class="flex items-end justify-between gap-4 mb-4"><div><h2 class="text-xl font-black">Recommended for you</h2><p class="nv-muted text-sm">Suggestions based on your Nvent activity and event preferences.</p></div></div>
        <div class="nv-grid-cards">@for(item of recommendations();track item.eventId){<app-event-recommendation-card [recommendation]="item"/>}</div>
      </section>
    }

    <section>
      <div class="flex items-end justify-between gap-4 mb-4"><div><h2 class="text-xl font-black">Upcoming events</h2><p class="nv-muted text-sm">Published events currently available from Nvent.</p></div><a routerLink="/app/events" class="font-extrabold text-sm">View all</a></div>
      @if(loading()){<app-loading-grid/>}@else if(events().length){<div class="nv-grid-cards">@for(event of events();track event.eventId){<app-event-card [event]="event"/>}</div>}@else{<app-page-state title="No upcoming events" message="There are no published events to show right now."/>}
    </section>

    @if(venues().length){
      <section>
        <div class="flex items-end justify-between gap-4 mb-4"><div><h2 class="text-xl font-black">Explore venues</h2><p class="nv-muted text-sm">Discover active venues available through the platform.</p></div><a routerLink="/app/venues" class="font-extrabold text-sm">View all</a></div>
        <div class="grid md:grid-cols-2 xl:grid-cols-3 gap-4">@for(v of venues().slice(0,3);track v.venueId){<app-venue-card [venue]="v"/>}</div>
      </section>
    }

    <section class="nv-card p-6 sm:p-8 flex flex-col md:flex-row md:items-center justify-between gap-5">
      <div><h2 class="text-2xl font-black">Host an Event</h2><p class="nv-muted mt-1 max-w-xl">Use the organizer workspace to configure your venue, stage, seating and ticket experience.</p></div>
      <a [routerLink]="auth.role()===1?'/organizer/host-event':'/auth/sign-up'" [queryParams]="auth.role()===1?{}:{role:1}" class="nv-btn nv-btn-primary">Host an Event</a>
    </section>
  </div>`
})
export class HomePageComponent implements OnInit{
  private eventsApi=inject(EventsService);
  private venuesApi=inject(VenuesService);
  private router=inject(Router);
  auth=inject(AuthService);
  events=signal<EventDto[]>([]);
  venues=signal<VenueDto[]>([]);
  recommendations=signal<EventRecommendationDto[]>([]);
  loading=signal(true);
  heroVideoFailed=false;
  search='';
  quick=[
    {label:'Events',path:'/app/events',icon:'✦',note:'Discover',asset:'assets/nvent/3d/icons/events.webp'},
    {label:'Venues',path:'/app/venues',icon:'⌂',note:'Explore spaces',asset:'assets/nvent/3d/icons/venues.webp'},
    {label:'Parking',path:'/app/parking',icon:'P',note:'Reserve smart',asset:'assets/nvent/3d/icons/parking.webp'},
    {label:'Food & Drinks',path:'/app/food',icon:'◉',note:'Order ahead',asset:'assets/nvent/3d/icons/food.webp'},
    {label:'Places',path:'/app/places',icon:'⌖',note:'Nearby picks',asset:'assets/nvent/3d/icons/places.webp'},
    {label:'My Bookings',path:'/app/bookings',icon:'▤',note:'Your plans',asset:'assets/nvent/3d/icons/tickets.webp'}
  ];

  ngOnInit(){
    const recommendations$=this.isCustomer()?this.eventsApi.recommendations(6).pipe(catchError(()=>of([]))):of([] as EventRecommendationDto[]);
    forkJoin({
      events:this.eventsApi.list({page:1,pageSize:8}).pipe(catchError(()=>of([]))),
      venues:this.venuesApi.list(1,6).pipe(catchError(()=>of([]))),
      recommendations:recommendations$
    }).subscribe(v=>{
      this.events.set(v.events);
      this.venues.set(v.venues);
      this.recommendations.set(v.recommendations);
      this.loading.set(false);
    });
  }

  isCustomer(){return this.auth.role()===0;}
  searchEvents(){const q=this.search.trim();void this.router.navigate(['/app/events'],{queryParams:q?{q}:{}});}
}
