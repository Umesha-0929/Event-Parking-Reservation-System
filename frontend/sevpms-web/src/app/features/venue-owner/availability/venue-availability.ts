
import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { VenueMarketplaceDto, VenueSummary } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';

@Component({
  selector: 'app-venue-availability',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './venue-availability.html',
  styleUrl: './venue-availability.scss',
})
export class VenueAvailabilityComponent implements OnInit {
  private readonly domain=inject(DomainApiService); private readonly route=inject(ActivatedRoute);
  readonly venues=signal<VenueSummary[]>([]); readonly marketplace=signal<VenueMarketplaceDto|null>(null);
  readonly loading=signal(true); readonly saving=signal(false); readonly error=signal(''); readonly success=signal('');
  selectedVenueId=''; start=''; end=''; type=0; notes='';

  ngOnInit():void{
    this.domain.myVenues().subscribe({next:v=>{this.venues.set(v.filter(x=>x.isActive!==false)); const q=this.route.snapshot.queryParamMap.get('venueId'); this.selectedVenueId=(q&&v.some(x=>this.id(x)===q))?q:this.id(v[0]); if(this.selectedVenueId)this.load();else this.loading.set(false);},error:e=>{this.loading.set(false);this.error.set(httpErrorMessage(e,'Your venues could not be loaded.'));}});
  }
  id(v?:VenueSummary):string{return v?.venueId??v?.id??'';}
  load():void{if(!this.selectedVenueId)return;this.loading.set(true);this.error.set('');this.domain.venueMarketplace(this.selectedVenueId).subscribe({next:x=>{this.marketplace.set(x);this.loading.set(false);},error:e=>{this.loading.set(false);this.error.set(httpErrorMessage(e,'Availability could not be loaded.'));}});}
  add():void{
    if(!this.start||!this.end||new Date(this.end).getTime() <= new Date(this.start).getTime()||this.saving())return;
    this.saving.set(true);this.error.set('');this.success.set('');
    this.domain.addVenueAvailability(this.selectedVenueId,{startAtUtc:new Date(this.start).toISOString(),endAtUtc:new Date(this.end).toISOString(),type:Number(this.type),notes:this.notes||null}).subscribe({
      next:()=>{this.saving.set(false);this.success.set('Availability period added.');this.start='';this.end='';this.notes='';this.load();},
      error:e=>{this.saving.set(false);this.error.set(httpErrorMessage(e,'Availability could not be saved.'));}
    });
  }
  typeLabel(value:number|string):string{const n=typeof value==='number'?value:Number(value);return n===1?'Blocked':n===2?'Maintenance':'Available';}
}
