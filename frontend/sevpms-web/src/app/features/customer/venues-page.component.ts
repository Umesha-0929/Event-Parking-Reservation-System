import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { VenuesService } from '../../core/services/venues.service';
import { VenueDto } from '../../core/models/api.models';
import { LoadingGridComponent } from '../../shared/components/loading-grid.component';
import { PageStateComponent } from '../../shared/components/page-state.component';
import { VenueCardComponent } from '../../shared/components/venue-card.component';

@Component({
  selector:'app-venues-page',
  standalone:true,
  imports:[FormsModule,LoadingGridComponent,PageStateComponent,VenueCardComponent],
  template:`
  <div class="nv-page space-y-6">
    <section class="nv-hero nv-hero-media min-h-[260px] text-white"><video class="nv-section-video" autoplay muted loop playsinline preload="metadata" poster="assets/nvent/images/hero/venues-hero.jpg"><source src="assets/nvent/videos/venue/venue-showcase.mp4" type="video/mp4"></video><div class="nv-section-video-overlay"></div><div class="relative z-10"><p class="font-extrabold text-sm opacity-75">Venue discovery</p><h1 class="nv-page-title mt-1">Venues</h1><p class="mt-2 opacity-80">Explore active venues, facilities and events available through Nvent.</p></div></section>
    <section class="nv-card p-4"><label><span class="sr-only">Search venues</span><input class="nv-input" [(ngModel)]="search" placeholder="Search venue, city or district" aria-label="Search venue, city or district"></label></section>
    @if(loading()){<app-loading-grid/>}
    @else if(error()){<app-page-state kind="error" title="We couldn't load venues" message="Please check your connection and try again." actionLabel="Retry" [action]="retry"/>}
    @else if(filtered.length){<div class="grid md:grid-cols-2 xl:grid-cols-3 gap-4">@for(v of filtered;track v.venueId){<app-venue-card [venue]="v"/>}</div>}
    @else{<app-page-state title="No venues found" message="Try a different venue, city or district."/>}
  </div>`
})
export class VenuesPageComponent implements OnInit{
  private api=inject(VenuesService);venues=signal<VenueDto[]>([]);loading=signal(true);error=signal(false);search='';retry=()=>this.load();
  get filtered(){const q=this.search.trim().toLowerCase();return q?this.venues().filter(v=>`${v.name} ${v.city} ${v.district}`.toLowerCase().includes(q)):this.venues();}
  ngOnInit(){this.load();}
  load(){this.loading.set(true);this.error.set(false);this.api.list().subscribe({next:v=>{this.venues.set(v);this.loading.set(false);},error:()=>{this.error.set(true);this.loading.set(false);}});}
}
