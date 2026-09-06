
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { VenueFacilityDto } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';

@Component({selector:'app-venue-facilities',imports:[FormsModule],templateUrl:'./venue-facilities.html',styleUrl:'./venue-facilities.scss'})
export class VenueFacilitiesComponent implements OnInit{
 private readonly domain=inject(DomainApiService);
 readonly facilities=signal<VenueFacilityDto[]>([]); readonly loading=signal(true); readonly error=signal(''); readonly busy=signal(false);
 name='';category='General';editing:VenueFacilityDto|null=null;active=true;
 ngOnInit():void{this.load();}
 load():void{this.loading.set(true);this.domain.venueFacilities().subscribe({next:x=>{this.facilities.set(x);this.loading.set(false);},error:e=>{this.loading.set(false);this.error.set(httpErrorMessage(e,'Facilities could not be loaded.'));}});}
 edit(f:VenueFacilityDto):void{this.editing=f;this.name=f.name;this.category=f.category;this.active=f.isActive;}
 reset():void{this.editing=null;this.name='';this.category='General';this.active=true;}
 save():void{if(!this.name.trim()||!this.category.trim()||this.busy())return;this.busy.set(true);const body={name:this.name.trim(),category:this.category.trim(),isActive:this.active};const req=this.editing?this.domain.updateVenueFacility(this.editing.facilityId,body):this.domain.createVenueFacility(body);req.subscribe({next:()=>{this.busy.set(false);this.reset();this.load();},error:e=>{this.busy.set(false);this.error.set(httpErrorMessage(e,'Facility could not be saved.'));}});}
}
