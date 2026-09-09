import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { catchError, forkJoin, of } from 'rxjs';
import { UpsertVenueRequest, VenueMarketplaceDto, VenueSummary } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';
import { NavIconComponent } from '../../../shared/components/nav-icon/nav-icon';
import { MediaDropzoneComponent } from '../../../shared/components/media-dropzone/media-dropzone';
import { LocationMapComponent, MapLocationSelection } from '../../../shared/components/location-map/location-map';
import { UploadedLocalMedia } from '../../../core/services/local-media.service';

@Component({selector:'app-venue-form',imports:[ReactiveFormsModule,RouterLink,NavIconComponent,MediaDropzoneComponent,LocationMapComponent],templateUrl:'./venue-form.html',styleUrl:'./venue-form.scss'})
export class VenueFormComponent implements OnInit{
  private readonly route=inject(ActivatedRoute);private readonly router=inject(Router);private readonly domain=inject(DomainApiService);
  readonly venueId=this.route.snapshot.paramMap.get('id');readonly editing=!!this.venueId;readonly loading=signal(this.editing);readonly saving=signal(false);readonly error=signal('');readonly success=signal('');
  private loadedCoverImageUrl='';
  private nextCoverSortOrder=-100;

  readonly form=new FormGroup({
    name:new FormControl('',{nonNullable:true,validators:[Validators.required,Validators.maxLength(160)]}),
    description:new FormControl('',{nonNullable:true,validators:[Validators.required,Validators.minLength(10),Validators.maxLength(3000)]}),
    coverImageUrl:new FormControl('',{nonNullable:true,validators:[Validators.maxLength(2000)]}),
    addressLine1:new FormControl('',{nonNullable:true,validators:[Validators.required,Validators.maxLength(250)]}),addressLine2:new FormControl('',{nonNullable:true,validators:[Validators.maxLength(250)]}),
    city:new FormControl('',{nonNullable:true,validators:[Validators.required,Validators.maxLength(120)]}),district:new FormControl('',{nonNullable:true,validators:[Validators.required,Validators.maxLength(120)]}),country:new FormControl('Sri Lanka',{nonNullable:true,validators:[Validators.required,Validators.maxLength(120)]}),capacity:new FormControl<number|null>(null,{validators:[Validators.required,Validators.min(1),Validators.max(1000000)]}),latitude:new FormControl<number|null>(null),longitude:new FormControl<number|null>(null),contactPhone:new FormControl('',{nonNullable:true,validators:[Validators.maxLength(40)]}),contactEmail:new FormControl('',{nonNullable:true,validators:[Validators.email,Validators.maxLength(200)]}),
  });

  ngOnInit():void{
    if(!this.venueId)return;
    forkJoin({venue:this.domain.venue(this.venueId),marketplace:this.domain.venueMarketplace(this.venueId).pipe(catchError(()=>of(null as VenueMarketplaceDto|null)))}).subscribe({
      next:({venue,marketplace})=>{this.patchVenue(venue);this.loadedCoverImageUrl=this.firstPhoto(marketplace);this.nextCoverSortOrder=this.coverSortOrder(marketplace);this.form.controls.coverImageUrl.setValue(this.loadedCoverImageUrl);this.loading.set(false);},
      error:(error)=>{this.error.set(httpErrorMessage(error,'The venue could not be loaded.'));this.loading.set(false);},
    });
  }

  submit():void{
    this.form.markAllAsTouched();if(this.form.invalid||this.saving())return;
    const value=this.form.getRawValue();const body:UpsertVenueRequest={name:value.name.trim(),description:value.description.trim(),addressLine1:value.addressLine1.trim(),addressLine2:value.addressLine2.trim()||null,city:value.city.trim(),district:value.district.trim(),country:value.country.trim(),capacity:Number(value.capacity),latitude:value.latitude==null?null:Number(value.latitude),longitude:value.longitude==null?null:Number(value.longitude),contactPhone:value.contactPhone.trim()||null,contactEmail:value.contactEmail.trim()||null};
    this.saving.set(true);this.error.set('');this.success.set('');
    const request=this.venueId?this.domain.updateVenue(this.venueId,body):this.domain.createVenue(body);
    request.subscribe({next:(venue)=>this.finishVenueSave(venue),error:(error)=>{this.saving.set(false);this.error.set(httpErrorMessage(error,'The venue could not be saved.'));}});
  }

  coverPreview():string{return this.form.controls.coverImageUrl.value.trim();}
  onCoverUploaded(media:UploadedLocalMedia):void{this.form.controls.coverImageUrl.setValue(media.url);this.success.set('Venue image uploaded. Save the venue to make it the cover image.');}
  onMapSelected(location:MapLocationSelection):void{this.form.patchValue({latitude:location.latitude,longitude:location.longitude,addressLine1:location.addressLine1||location.displayName||this.form.controls.addressLine1.value,city:location.city||this.form.controls.city.value,district:location.district||this.form.controls.district.value,country:location.country||this.form.controls.country.value});}



  private finishVenueSave(venue:VenueSummary):void{
    const id=venue.venueId??venue.id??this.venueId??'';const cover=this.coverPreview();
    if(!id){this.saving.set(false);this.error.set('The venue was saved but its ID was not returned.');return;}
    if(!cover||cover===this.loadedCoverImageUrl){this.complete(id);return;}
    this.domain.addVenueMedia(id,{url:cover,type:'Photo',sortOrder:this.nextCoverSortOrder}).subscribe({
      next:()=>{this.loadedCoverImageUrl=cover;this.complete(id);},
      error:(error)=>{this.saving.set(false);this.success.set(this.editing?'Venue details were saved.':'Venue was registered.');this.error.set(httpErrorMessage(error,'The venue was saved, but the cover image could not be added. You can add it later from Marketplace.'));},
    });
  }

  private complete(id:string):void{
    this.saving.set(false);this.success.set(this.editing?'Venue updated successfully.':'Venue registered successfully.');
    if(!this.editing)void this.router.navigate(['/venue-owner/venues',id,'edit'],{replaceUrl:true});
  }

  private patchVenue(venue:VenueSummary):void{this.form.patchValue({name:venue.name??'',description:venue.description??'',addressLine1:venue.addressLine1??'',addressLine2:venue.addressLine2??'',city:venue.city??'',district:venue.district??'',country:venue.country??'Sri Lanka',capacity:venue.capacity??null,latitude:venue.latitude??null,longitude:venue.longitude??null,contactPhone:venue.contactPhone??'',contactEmail:venue.contactEmail??''});}
  private firstPhoto(marketplace:VenueMarketplaceDto|null):string{return [...(marketplace?.media??[])].filter(x=>x.type?.toLowerCase()==='photo'||/\.(png|jpe?g|webp|gif)(\?|$)/i.test(x.url??'')).sort((a,b)=>Number(a.sortOrder??0)-Number(b.sortOrder??0))[0]?.url??'';}
  private coverSortOrder(marketplace:VenueMarketplaceDto|null):number{const orders=(marketplace?.media??[]).map(x=>Number(x.sortOrder??0)).filter(Number.isFinite);return orders.length?Math.min(...orders)-1:-100;}
}
