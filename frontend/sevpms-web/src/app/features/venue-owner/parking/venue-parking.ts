
import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ParkingSlotDto, ParkingZoneDto, VenueSummary } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';

@Component({
 selector:'app-venue-parking', imports:[CommonModule,FormsModule,RouterLink], templateUrl:'./venue-parking.html', styleUrl:'./venue-parking.scss'
})
export class VenueParkingComponent implements OnInit{
 private readonly domain=inject(DomainApiService); private readonly route=inject(ActivatedRoute);
 readonly venues=signal<VenueSummary[]>([]); readonly zones=signal<ParkingZoneDto[]>([]); readonly slots=signal<ParkingSlotDto[]>([]);
 readonly loading=signal(true); readonly busy=signal(false); readonly error=signal(''); readonly success=signal('');
 selectedVenueId=''; selectedZoneId='';
 zoneName='Main Parking'; zoneLevel='Ground'; zoneEntrance='Main Entrance';
 slotCode='A1'; slotX=1; slotY=1; slotAccessible=false; slotStatus='Available';
 gridPrefix='P'; gridRows=5; gridColumns=10; gridXGap=1; gridYGap=1; accessibleEvery=10;

 ngOnInit():void{this.domain.myVenues().subscribe({next:v=>{this.venues.set(v.filter(x=>x.isActive!==false));const q=this.route.snapshot.queryParamMap.get('venueId');this.selectedVenueId=(q&&v.some(x=>this.id(x)===q))?q:this.id(v[0]);if(this.selectedVenueId)this.loadZones();else this.loading.set(false);},error:e=>{this.loading.set(false);this.error.set(httpErrorMessage(e,'Your venues could not be loaded.'));}});}
 id(v?:VenueSummary):string{return v?.venueId??v?.id??'';}
 loadZones():void{if(!this.selectedVenueId)return;this.loading.set(true);this.error.set('');this.domain.parkingZones(this.selectedVenueId).subscribe({next:z=>{this.zones.set(z);if(!z.some(x=>x.id===this.selectedZoneId))this.selectedZoneId=z[0]?.id??'';this.loading.set(false);if(this.selectedZoneId)this.loadSlots();else this.slots.set([]);},error:e=>{this.loading.set(false);this.error.set(httpErrorMessage(e,'Parking zones could not be loaded.'));}});}
 loadSlots():void{if(!this.selectedZoneId){this.slots.set([]);return;}this.domain.parkingSlots(this.selectedZoneId).subscribe({next:s=>this.slots.set(s),error:e=>this.error.set(httpErrorMessage(e,'Parking slots could not be loaded.'))});}
 createZone():void{if(!this.selectedVenueId||!this.zoneName.trim()||this.busy())return;this.busy.set(true);this.error.set('');this.domain.createParkingZone({venueId:this.selectedVenueId,eventId:null,name:this.zoneName.trim(),level:this.zoneLevel.trim(),entranceName:this.zoneEntrance.trim()}).subscribe({next:z=>{this.busy.set(false);this.success.set('Parking zone created.');this.selectedZoneId=z.id;this.loadZones();},error:e=>{this.busy.set(false);this.error.set(httpErrorMessage(e,'Parking zone could not be created.'));}});}
 deleteZone(z:ParkingZoneDto):void{if(this.busy()||!confirm(`Delete parking zone “${z.name}” and its slots?`))return;this.busy.set(true);this.domain.deleteParkingZone(z.id).subscribe({next:()=>{this.busy.set(false);this.selectedZoneId='';this.loadZones();},error:e=>{this.busy.set(false);this.error.set(httpErrorMessage(e,'Parking zone could not be deleted.'));}});}
 addSlot():void{if(!this.selectedZoneId||!this.slotCode.trim()||this.busy())return;this.busy.set(true);this.domain.createParkingSlot({parkingZoneId:this.selectedZoneId,eventId:null,slotCode:this.slotCode.trim(),x:Number(this.slotX),y:Number(this.slotY),isAccessible:this.slotAccessible,status:this.slotStatus}).subscribe({next:()=>{this.busy.set(false);this.success.set('Parking slot added.');this.loadSlots();},error:e=>{this.busy.set(false);this.error.set(httpErrorMessage(e,'Parking slot could not be added.'));}});}
 generateGrid():void{
   const rows=Math.min(26,Math.max(1,Number(this.gridRows)||1)), cols=Math.min(50,Math.max(1,Number(this.gridColumns)||1));
   if(!this.selectedZoneId||this.busy()||rows*cols>300)return;
   const existing=new Set(this.slots().map(x=>x.slotCode.toLowerCase()));
   const requests = [] as Array<ReturnType<DomainApiService['createParkingSlot']>>; let index=0;
   for(let r=0;r<rows;r++)for(let c=0;c<cols;c++){index++;const code=`${this.gridPrefix || 'P'}-${String.fromCharCode(65+r)}${c+1}`;if(existing.has(code.toLowerCase()))continue;requests.push(this.domain.createParkingSlot({parkingZoneId:this.selectedZoneId,eventId:null,slotCode:code,x:Number((c*this.gridXGap).toFixed(2)),y:Number((r*this.gridYGap).toFixed(2)),isAccessible:this.accessibleEvery>0&&index%this.accessibleEvery===0,status:'Available'}));}
   if(!requests.length){this.success.set('All slots in this alignment already exist.');return;}
   this.busy.set(true);this.error.set('');forkJoin(requests).subscribe({next:()=>{this.busy.set(false);this.success.set(`${requests.length} aligned parking slots created.`);this.loadSlots();},error:e=>{this.busy.set(false);this.error.set(httpErrorMessage(e,'The parking alignment could not be completed. Some slots may have been created; refresh before trying again.'));this.loadSlots();}});
 }
 updateSlotStatus(s:ParkingSlotDto,status:string):void{if(this.busy())return;this.busy.set(true);this.domain.updateParkingSlot(s.id,{parkingZoneId:s.parkingZoneId,eventId:s.eventId??null,slotCode:s.slotCode,x:s.x,y:s.y,isAccessible:s.isAccessible,status}).subscribe({next:()=>{this.busy.set(false);this.loadSlots();},error:e=>{this.busy.set(false);this.error.set(httpErrorMessage(e,'Parking slot could not be updated.'));}});}
 deleteSlot(s:ParkingSlotDto):void{if(this.busy())return;this.busy.set(true);this.domain.deleteParkingSlot(s.id).subscribe({next:()=>{this.busy.set(false);this.loadSlots();},error:e=>{this.busy.set(false);this.error.set(httpErrorMessage(e,'Parking slot could not be deleted.'));}});}
 slotStyle(s:ParkingSlotDto):Record<string,string>{return {left:`${Math.max(0,s.x)*34+16}px`,top:`${Math.max(0,s.y)*34+16}px`};}
}
