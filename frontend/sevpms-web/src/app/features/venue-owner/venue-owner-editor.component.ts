import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { catchError, forkJoin, of } from 'rxjs';
import { VenuesService } from '../../core/services/venues.service';
import { MediaService } from '../../core/services/media.service';
import { VenueFacility, VenueMarketplace } from '../../core/models/api.models';

@Component({selector:'app-venue-owner-editor',standalone:true,imports:[FormsModule,DatePipe,DecimalPipe],template:`
<div class="nv-page space-y-6">
  <div><p class="font-extrabold text-sm nv-muted">Venue Management</p><h1 class="nv-page-title">{{id?'Edit Venue':'Add Venue'}}</h1><p class="nv-muted mt-1">Keep venue information and marketplace details accurate for organizers.</p></div>

  <div class="grid xl:grid-cols-[1fr_360px] gap-5">
    <div class="space-y-5">
      <form class="nv-card p-5 md:p-6 space-y-4" (ngSubmit)="save()">
        <h2 class="font-black text-xl">Venue information</h2>
        <div><label class="nv-label">Venue name</label><input class="nv-input" [(ngModel)]="name" name="name" required aria-label="Name"></div>
        <div><label class="nv-label">Description</label><textarea class="nv-input min-h-28" [(ngModel)]="description" name="description" aria-label="Description"></textarea></div>
        <div><label class="nv-label">Address</label><input class="nv-input" [(ngModel)]="addressLine1" name="address1" required aria-label="Address1"></div>
        <div class="grid md:grid-cols-3 gap-3"><div><label class="nv-label">City</label><input class="nv-input" [(ngModel)]="city" name="city" required aria-label="City"></div><div><label class="nv-label">District</label><input class="nv-input" [(ngModel)]="district" name="district" required aria-label="District"></div><div><label class="nv-label">Country</label><input class="nv-input" [(ngModel)]="country" name="country" required aria-label="Country"></div></div>
        <div class="grid md:grid-cols-3 gap-3"><div><label class="nv-label">Capacity</label><input class="nv-input" type="number" min="1" [(ngModel)]="capacity" name="capacity" required aria-label="Capacity"></div><div><label class="nv-label">Latitude</label><input class="nv-input" type="number" step="any" [(ngModel)]="latitude" name="lat" aria-label="Latitude"></div><div><label class="nv-label">Longitude</label><input class="nv-input" type="number" step="any" [(ngModel)]="longitude" name="lon" aria-label="Lon"></div></div>
        <div class="grid md:grid-cols-2 gap-3"><div><label class="nv-label">Contact phone</label><input class="nv-input" [(ngModel)]="contactPhone" name="phone" aria-label="Phone"></div><div><label class="nv-label">Contact email</label><input class="nv-input" type="email" [(ngModel)]="contactEmail" name="email" aria-label="Email"></div></div>
        <button type="submit" class="nv-btn nv-btn-primary" [disabled]="busy()">{{busy()?'Saving...':id?'Save Venue':'Create Venue'}}</button>
      </form>

      @if(id){
        <section class="nv-card p-5 md:p-6">
          <div class="flex flex-wrap justify-between gap-3 items-center"><div><h2 class="font-black text-xl">Facilities</h2><p class="nv-muted text-sm mt-1">Select the facilities organizers should see.</p></div><button type="button" class="nv-btn nv-btn-secondary" (click)="saveFacilities()">Save Facilities</button></div>
          <div class="grid sm:grid-cols-2 lg:grid-cols-3 gap-2 mt-4">@for(f of facilities();track f.facilityId){<label class="nv-card-soft p-3 flex items-center gap-3 text-sm font-bold"><input type="checkbox" [checked]="facilitySelected(f.facilityId)" (change)="toggleFacility(f.facilityId,$event)" [attr.aria-label]="'Toggle '+f.name">{{f.name}}<span class="ml-auto text-xs nv-muted">{{f.category}}</span></label>}</div>
        </section>

        <section class="nv-card p-5 md:p-6">
          <h2 class="font-black text-xl">Venue media</h2><p class="nv-muted text-sm mt-1">Upload real venue photos or short videos. The first photo is used as the customer-facing cover.</p>
          <div class="mt-4 flex flex-wrap items-center gap-3"><input type="file" accept="image/*,video/mp4,video/webm,video/quicktime" (change)="mediaSelected($event)" aria-label="Choose file"><span class="text-xs nv-muted">Use a clear venue image or supported short video</span></div>
          @if(marketplace()?.media?.length){<div class="grid sm:grid-cols-2 lg:grid-cols-3 gap-3 mt-5">@for(m of marketplace()!.media;track m.venueMediaId){<div class="nv-card-soft overflow-hidden">@if(m.type.toLowerCase()==='photo'){<img [src]="m.url" alt="Venue media" class="w-full aspect-[4/3] object-cover" loading="lazy">}@else{<video [src]="m.url" class="w-full aspect-[4/3] object-cover" controls muted playsinline preload="metadata"></video>}<div class="p-3 text-xs font-bold">{{m.type}} · order {{m.sortOrder}}</div></div>}</div>}
        </section>

        <div class="grid lg:grid-cols-2 gap-5">
          <section class="nv-card p-5">
            <h2 class="font-black text-xl">Rates</h2><form class="space-y-3 mt-4" (ngSubmit)="addRate()"><div><label class="nv-label">Rate type</label><select class="nv-input" [(ngModel)]="rateType" name="rateType" aria-label="Rate type"><option>Hourly</option><option>Daily</option><option>Event</option></select></div><div class="grid grid-cols-[1fr_110px] gap-2"><input class="nv-input" type="number" min="0" step="0.01" [(ngModel)]="rateAmount" name="rateAmount" placeholder="Amount" required aria-label="Amount"><input class="nv-input" [(ngModel)]="rateCurrency" name="currency" placeholder="LKR" aria-label="LKR"></div><button type="submit" class="nv-btn nv-btn-secondary w-full">Add Rate</button></form>
            <div class="space-y-2 mt-4">@for(r of marketplace()?.rates??[];track r.venueRateId){<div class="nv-card-soft p-3 flex justify-between text-sm"><span>{{r.rateType}}</span><strong>{{r.currency}} {{r.amount|number:'1.0-2'}}</strong></div>}</div>
          </section>

          <section class="nv-card p-5">
            <h2 class="font-black text-xl">Availability</h2><form class="space-y-3 mt-4" (ngSubmit)="addAvailability()"><div><label class="nv-label">From</label><input class="nv-input" type="datetime-local" [(ngModel)]="availabilityStart" name="availStart" required aria-label="Availability start"></div><div><label class="nv-label">To</label><input class="nv-input" type="datetime-local" [(ngModel)]="availabilityEnd" name="availEnd" required aria-label="Availability end"></div><select class="nv-input" [(ngModel)]="availabilityType" name="availType" aria-label="Availability type"><option [ngValue]="0">Available</option><option [ngValue]="1">Blocked</option><option [ngValue]="2">Maintenance</option></select><input class="nv-input" [(ngModel)]="availabilityNotes" name="availNotes" placeholder="Optional notes" aria-label="Optional notes"><button type="submit" class="nv-btn nv-btn-secondary w-full">Add Availability</button></form>
            <div class="space-y-2 mt-4">@for(a of marketplace()?.availability??[];track a.venueAvailabilityId){<div class="nv-card-soft p-3 text-sm"><div class="font-extrabold">{{availabilityLabel(a.type)}}</div><div class="nv-muted text-xs mt-1">{{a.startAtUtc|date:'MMM d, y · h:mm a'}} – {{a.endAtUtc|date:'MMM d, y · h:mm a'}}</div></div>}</div>
          </section>
        </div>
      }
    </div>

    <aside class="space-y-4">
      <section class="nv-card p-5"><h2 class="font-black">Marketplace readiness</h2><p class="nv-muted text-sm mt-2">Venue information, facilities, media, rates and availability feed the public venue experience and organizer rental decisions.</p>@if(id){<dl class="space-y-2 mt-4 text-sm"><div class="flex justify-between"><dt class="nv-muted">Facilities</dt><dd class="font-extrabold">{{marketplace()?.facilities?.length??0}}</dd></div><div class="flex justify-between"><dt class="nv-muted">Media</dt><dd class="font-extrabold">{{marketplace()?.media?.length??0}}</dd></div><div class="flex justify-between"><dt class="nv-muted">Rates</dt><dd class="font-extrabold">{{marketplace()?.rates?.length??0}}</dd></div></dl>}</section>
      @if(id){<section class="nv-card p-5"><h2 class="font-black">Venue actions</h2><button type="button" class="nv-btn nv-btn-secondary w-full mt-4" (click)="deactivate()">Deactivate Venue</button></section>}
    </aside>
  </div>
  @if(message()){<div class="fixed right-4 bottom-4 nv-card px-4 py-3" role="status">{{message()}}</div>}
</div>`})
export class VenueOwnerEditorComponent implements OnInit{
  private route=inject(ActivatedRoute);private router=inject(Router);private api=inject(VenuesService);private media=inject(MediaService);
  id='';name='';description='';addressLine1='';city='';district='';country='Sri Lanka';capacity=1;latitude:number|null=null;longitude:number|null=null;contactPhone='';contactEmail='';busy=signal(false);message=signal('');
  facilities=signal<VenueFacility[]>([]);marketplace=signal<VenueMarketplace|null>(null);selectedFacilityIds=new Set<string>();rateType='Hourly';rateAmount=0;rateCurrency='LKR';availabilityStart='';availabilityEnd='';availabilityType=0;availabilityNotes='';
  ngOnInit(){
    this.id=this.route.snapshot.paramMap.get('id')??'';
    this.api.facilities().subscribe({next:v=>this.facilities.set(v as VenueFacility[]),error:()=>void 0});
    if(this.id){forkJoin({venue:this.api.get(this.id),market:this.api.marketplace(this.id).pipe(catchError(()=>of(null)))}).subscribe(v=>{const x=v.venue;Object.assign(this,{name:x.name,description:x.description,addressLine1:x.addressLine1,city:x.city,district:x.district,country:x.country,capacity:x.capacity,latitude:x.latitude??null,longitude:x.longitude??null,contactPhone:x.contactPhone??'',contactEmail:x.contactEmail??''});this.applyMarketplace(v.market);});}
  }
  private applyMarketplace(v:VenueMarketplace|null){this.marketplace.set(v);this.selectedFacilityIds=new Set((v?.facilities??[]).map(x=>x.facilityId));}
  private refreshMarketplace(){if(!this.id)return;this.api.marketplace(this.id).subscribe({next:v=>this.applyMarketplace(v)});}
  save(){if(!this.name||!this.addressLine1||!this.city||!this.district)return;this.busy.set(true);const body={name:this.name,description:this.description,addressLine1:this.addressLine1,addressLine2:null,city:this.city,district:this.district,country:this.country,latitude:this.latitude,longitude:this.longitude,capacity:Number(this.capacity),contactPhone:this.contactPhone||null,contactEmail:this.contactEmail||null};const req=this.id?this.api.update(this.id,body):this.api.create(body);req.subscribe({next:v=>{this.busy.set(false);this.message.set(this.id?'Venue saved.':'Venue created.');if(!this.id){this.id=v.venueId;void this.router.navigate(['/venue-owner/venues',v.venueId,'edit'],{replaceUrl:true});this.refreshMarketplace();}},error:e=>{this.busy.set(false);this.message.set(e?.error?.message||e?.error?.error||'Venue could not be saved.');}});}
  facilitySelected(id:string){return this.selectedFacilityIds.has(id);}
  toggleFacility(id:string,event:Event){const checked=(event.target as HTMLInputElement).checked;if(checked)this.selectedFacilityIds.add(id);else this.selectedFacilityIds.delete(id);}
  saveFacilities(){if(!this.id)return;this.api.setFacilities(this.id,[...this.selectedFacilityIds]).subscribe({next:()=>{this.message.set('Facilities updated.');this.refreshMarketplace();},error:e=>this.message.set(e?.error?.message||'Facilities could not be updated.')});}
  mediaSelected(event:Event){const file=(event.target as HTMLInputElement).files?.[0];if(!file||!this.id)return;const category=`venue-${this.id}`;this.media.upload(file,category).subscribe({next:u=>this.api.addMedia(this.id,{url:u.url,type:u.type,sortOrder:(this.marketplace()?.media?.length??0)}).subscribe({next:()=>{this.message.set('Venue media added.');this.refreshMarketplace();},error:e=>this.message.set(e?.error?.message||'Media could not be attached to the venue.')}),error:e=>this.message.set(e?.error?.message||'Media upload failed.')});}
  addRate(){if(!this.id||Number(this.rateAmount)<0)return;this.api.addRate(this.id,{rateType:this.rateType,amount:Number(this.rateAmount),currency:this.rateCurrency||'LKR',validFromUtc:null,validToUtc:null}).subscribe({next:()=>{this.rateAmount=0;this.message.set('Venue rate added.');this.refreshMarketplace();},error:e=>this.message.set(e?.error?.message||'Rate could not be added.')});}
  addAvailability(){if(!this.id||!this.availabilityStart||!this.availabilityEnd)return;this.api.addAvailability(this.id,{startAtUtc:new Date(this.availabilityStart).toISOString(),endAtUtc:new Date(this.availabilityEnd).toISOString(),type:Number(this.availabilityType),notes:this.availabilityNotes||null}).subscribe({next:()=>{this.availabilityStart='';this.availabilityEnd='';this.availabilityNotes='';this.message.set('Availability added.');this.refreshMarketplace();},error:e=>this.message.set(e?.error?.message||'Availability could not be added.')});}
  availabilityLabel(v:number|string){return ['Available','Blocked','Maintenance'][Number(v)]??String(v);}
  deactivate(){if(!this.id)return;this.api.deactivate(this.id).subscribe({next:()=>{this.message.set('Venue deactivated.');void this.router.navigate(['/venue-owner/venues']);},error:e=>this.message.set(e?.error?.message||'Venue could not be deactivated.')});}
}
