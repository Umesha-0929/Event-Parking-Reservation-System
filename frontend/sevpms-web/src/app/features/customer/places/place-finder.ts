import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NearbyPlaceDto, VenueSummary } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';

@Component({selector:'app-place-finder',imports:[CommonModule,FormsModule],templateUrl:'./place-finder.html',styleUrl:'./place-finder.scss'})
export class PlaceFinderComponent implements OnInit{
 private readonly domain=inject(DomainApiService);
 readonly venues=signal<VenueSummary[]>([]); readonly places=signal<NearbyPlaceDto[]>([]); readonly loading=signal(true); readonly error=signal('');
 venueId=''; audienceMode='Friends'; category=''; maxDistanceKm=5; includeClosed=false;
 ngOnInit():void{this.domain.venues().subscribe({next:v=>{this.venues.set(v);this.venueId=v[0]?.venueId??v[0]?.id??'';this.loading.set(false);if(this.venueId)this.search();},error:e=>{this.loading.set(false);this.error.set(httpErrorMessage(e,'Venues could not be loaded.'));}});}
 search():void{if(!this.venueId)return;this.loading.set(true);this.error.set('');this.domain.placeRecommendations(this.venueId,this.audienceMode,this.category,Number(this.maxDistanceKm)||undefined,this.includeClosed).subscribe({next:p=>{this.places.set(p);this.loading.set(false);},error:e=>{this.loading.set(false);this.error.set(httpErrorMessage(e,'Nearby places could not be loaded.'));}});}
}
