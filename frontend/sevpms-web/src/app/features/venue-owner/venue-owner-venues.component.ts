import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { VenuesService } from '../../core/services/venues.service';
import { VenueDto } from '../../core/models/api.models';
import { PageStateComponent } from '../../shared/components/page-state.component';

@Component({
  selector:'app-venue-owner-venues',
  standalone:true,
  imports:[RouterLink,PageStateComponent],
  template:`<div class="nv-page space-y-6">
    <div class="flex flex-wrap justify-between gap-3 items-start"><div><p class="font-extrabold text-sm nv-muted">Venue Management</p><h1 class="nv-page-title">My Venues</h1><p class="nv-muted mt-1">Edit venue details and continue into parking or marketplace configuration.</p></div><a routerLink="/venue-owner/add-venue" class="nv-btn nv-btn-primary">Add Venue</a></div>
    @if(loading()){
      <div class="grid md:grid-cols-2 xl:grid-cols-3 gap-4" aria-label="Loading venues">@for(i of [1,2,3];track i){<div class="nv-card overflow-hidden"><div class="nv-skeleton h-32"></div><div class="p-5"><div class="nv-skeleton h-6 w-2/3"></div><div class="nv-skeleton h-4 w-1/2 mt-3"></div><div class="nv-skeleton h-20 mt-5"></div></div></div>}</div>
    } @else if(loadError()){
      <app-page-state kind="error" title="Venues unavailable" message="We couldn't load your venues right now." actionLabel="Retry" [action]="retry"/>
    } @else if(visibleVenues().length){
      <div class="grid md:grid-cols-2 xl:grid-cols-3 gap-4">@for(v of visibleVenues();track v.venueId){<article class="nv-card overflow-hidden"><div class="h-32" style="background:linear-gradient(135deg,var(--primary),var(--accent-2))"></div><div class="p-5"><div class="flex justify-between gap-3"><div><h2 class="font-black text-xl">{{v.name}}</h2><p class="nv-muted text-sm mt-1">{{v.city}}, {{v.district}}</p></div><span class="nv-status" [class.success]="v.isActive">{{v.isActive?'Active':'Inactive'}}</span></div><div class="grid grid-cols-2 gap-2 mt-4 text-sm"><div class="nv-card-soft p-3"><span class="nv-muted">Capacity</span><div class="font-black mt-1">{{v.capacity}}</div></div><div class="nv-card-soft p-3"><span class="nv-muted">Country</span><div class="font-black mt-1">{{v.country}}</div></div></div><div class="flex gap-2 mt-4"><a [routerLink]="['/venue-owner/venues',v.venueId,'edit']" class="nv-btn nv-btn-primary flex-1">Edit</a><a routerLink="/venue-owner/parking" [queryParams]="{venue:v.venueId}" class="nv-btn nv-btn-secondary">Parking</a></div></div></article>}</div>
    } @else if(filter()){
      <app-page-state title="No matching venues" message="No venues match your current search." actionLabel="Clear Search" [action]="clearSearch"/>
    } @else {
      <app-page-state title="No venues yet" message="Create your first venue to receive organizer rental requests." actionLabel="Add Venue" [action]="addVenue"/>
    }
  </div>`
})
export class VenueOwnerVenuesComponent implements OnInit{
  private api=inject(VenuesService);
  private route=inject(ActivatedRoute);
  private router=inject(Router);
  venues=signal<VenueDto[]>([]);
  filter=signal('');
  loading=signal(true);
  loadError=signal(false);
  visibleVenues=computed(()=>{const q=this.filter().trim().toLowerCase();return q?this.venues().filter(v=>`${v.name} ${v.city} ${v.district} ${v.country}`.toLowerCase().includes(q)):this.venues();});
  retry=()=>this.load();
  clearSearch=()=>void this.router.navigate(['/venue-owner/venues']);
  addVenue=()=>void this.router.navigate(['/venue-owner/add-venue']);
  ngOnInit(){this.route.queryParamMap.subscribe(params=>this.filter.set(params.get('q')??''));this.load();}
  load(){this.loading.set(true);this.loadError.set(false);this.api.mine().subscribe({next:v=>{this.venues.set(v);this.loading.set(false);},error:()=>{this.loading.set(false);this.loadError.set(true);}});}
}
