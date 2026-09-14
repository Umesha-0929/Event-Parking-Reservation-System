import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import { AddVenueAvailabilityRequest, AddVenueLayoutTemplateRequest, AddVenueMediaRequest, AddVenueRateRequest, CreateVenueRentalRequest, UpdateVenueRentalStatusRequest, VenueAvailability, VenueDto, VenueFacility, VenueLayoutTemplate, VenueMarketplace, VenueMedia, VenueRate, VenueRentalDto, VenueUpsertRequest } from '../models/api.models';
@Injectable({providedIn:'root'}) export class VenuesService{
 private api=inject(ApiService); list(page=1,pageSize=50){return this.api.get<VenueDto[]>('venues',{page,pageSize});} get(id:string){return this.api.get<VenueDto>(`venues/${id}`);} mine(){return this.api.get<VenueDto[]>('venues/mine',{page:1,pageSize:100});}
 create(body:VenueUpsertRequest){return this.api.post<VenueDto>('venues',body);} update(id:string,body:VenueUpsertRequest){return this.api.put<VenueDto>(`venues/${id}`,body);} deactivate(id:string){return this.api.delete<void>(`venues/${id}`);} marketplace(id:string){return this.api.get<VenueMarketplace>(`venues/${id}/marketplace`);} facilities(){return this.api.get<VenueFacility[]>('venue-facilities');}
 rentalsMine(){return this.api.get<VenueRentalDto[]>('venue-rentals/mine');} rentalsIncoming(){return this.api.get<VenueRentalDto[]>('venue-rentals/incoming');} requestRental(body:CreateVenueRentalRequest){return this.api.post<VenueRentalDto>('venue-rentals',body);} rentalStatus(id:string,status:number,ownerMessage?:string){const body:UpdateVenueRentalStatusRequest={status,ownerMessage:ownerMessage?.trim()||null};return this.api.put<VenueRentalDto>(`venue-rentals/${id}/status`,body);}
 permanentDelete(id:string){return this.api.delete<void>(`venues/${id}/permanent`);}
 setFacilities(id:string,facilityIds:string[]){return this.api.put<void>(`venues/${id}/marketplace/facilities`,{facilityIds});}
 addMedia(id:string,body:AddVenueMediaRequest){return this.api.post<VenueMedia>(`venues/${id}/marketplace/media`,body);}
 addRate(id:string,body:AddVenueRateRequest){return this.api.post<VenueRate>(`venues/${id}/marketplace/rates`,body);}
 addAvailability(id:string,body:AddVenueAvailabilityRequest){return this.api.post<VenueAvailability>(`venues/${id}/marketplace/availability`,body);}
 addLayoutTemplate(id:string,body:AddVenueLayoutTemplateRequest){return this.api.post<VenueLayoutTemplate>(`venues/${id}/marketplace/layout-templates`,body);}
}

