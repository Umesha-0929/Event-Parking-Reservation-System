import { Component, ElementRef, OnDestroy, OnInit, ViewChild, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { EventsService } from '../../core/services/events.service';
import { OrganizerService } from '../../core/services/organizer.service';
import { CheckInTicketResponse, EventDto } from '../../core/models/api.models';

@Component({selector:'app-organizer-checkin',standalone:true,imports:[FormsModule],template:`
<div class="nv-page space-y-6"><div><p class="font-extrabold text-sm nv-muted">Event Operations</p><h1 class="nv-page-title">Tickets & Check-In</h1><p class="nv-muted mt-1">Validate attendee QR tickets against an event you manage.</p></div>
<div class="grid xl:grid-cols-[1fr_380px] gap-5">
<section class="nv-card p-5 md:p-6"><div class="grid md:grid-cols-2 gap-4"><div><label class="nv-label">Event</label><select class="nv-input" [(ngModel)]="eventId" aria-label="Event  id"><option value="">Choose event</option>@for(e of events();track e.eventId){<option [value]="e.eventId">{{e.title}}</option>}</select></div><div><label class="nv-label">Gate</label><input class="nv-input" [(ngModel)]="gate" placeholder="Main Gate" aria-label="Main Gate"></div></div>
<div class="mt-5 rounded-[28px] overflow-hidden min-h-72 grid place-items-center relative" style="background:#0a1029"><video #video class="w-full h-full min-h-72 object-cover" playsinline muted></video>@if(!cameraActive()){<div class="absolute inset-0 grid place-items-center text-center p-8 text-white"><div><div class="text-5xl">▣</div><h2 class="font-black text-xl mt-4">Camera scanner</h2><p class="text-white/70 text-sm mt-2">Choose an event, then start the camera or paste a QR payload manually.</p></div></div>}</div>
<div class="flex flex-wrap gap-2 mt-4"><button type="button" class="nv-btn nv-btn-primary" [disabled]="!eventId" (click)="toggleCamera()">{{cameraActive()?'Stop Camera':'Start Camera'}}</button></div>
</section>
<aside class="space-y-4"><section class="nv-card p-5"><h2 class="font-black text-lg">Manual validation</h2><label class="nv-label mt-4">QR payload</label><textarea class="nv-input min-h-28" [(ngModel)]="payload" placeholder="Paste QR ticket payload" aria-label="Paste QR ticket payload"></textarea><button type="button" class="nv-btn nv-btn-primary w-full mt-3" [disabled]="!eventId||!payload.trim()||busy()" (click)="scan(payload)">{{busy()?'Validating...':'Validate Ticket'}}</button></section>
@if(result(); as check){<section class="nv-card p-5" [attr.role]="check.succeeded?'status':'alert'"><span class="nv-status" [class.success]="check.succeeded" [class.error]="!check.succeeded">{{check.succeeded?'Valid':'Not accepted'}}</span><h2 class="font-black text-xl mt-3">{{check.result}}</h2><p class="nv-muted mt-2">{{check.message}}</p>@if(check.ticketNo){<div class="nv-card-soft p-3 mt-4"><div class="text-xs nv-muted">Ticket</div><div class="font-black">{{check.ticketNo}}</div></div>}</section>}</aside></div>
</div>`})
export class OrganizerCheckinComponent implements OnInit,OnDestroy{
 private eventsApi=inject(EventsService);private ops=inject(OrganizerService);@ViewChild('video') video?:ElementRef<HTMLVideoElement>;
 events=signal<EventDto[]>([]);eventId='';gate='Main Gate';payload='';busy=signal(false);result=signal<CheckInTicketResponse|null>(null);cameraActive=signal(false);private controls:{stop:()=>void}|null=null;
 ngOnInit(){this.eventsApi.mine().subscribe({next:v=>this.events.set(v)});} async toggleCamera(){if(this.cameraActive()){this.stopCamera();return;}if(!this.eventId)return;try{const {BrowserQRCodeReader}=await import('@zxing/browser');const reader=new BrowserQRCodeReader();this.controls=await reader.decodeFromVideoDevice(undefined,this.video!.nativeElement,(r:{getText:()=>string}|undefined)=>{if(r && this.cameraActive()){const text=r.getText();this.payload=text;this.stopCamera();this.scan(text);}});this.cameraActive.set(true);}catch{this.result.set({succeeded:false,result:'Camera unavailable',message:'Use manual QR validation on this device.'});}}
 scan(value:string){if(!this.eventId||!value.trim())return;this.busy.set(true);this.ops.checkIn(this.eventId,value.trim(),this.gate||'Main Gate').subscribe({next:r=>{this.busy.set(false);this.result.set(r);this.payload='';},error:e=>{this.busy.set(false);this.result.set(e?.error??{succeeded:false,result:'Validation failed',message:'The ticket could not be validated.'});}})}
 stopCamera(){try{this.controls?.stop();}catch{}this.controls=null;this.cameraActive.set(false);}ngOnDestroy(){this.stopCamera();}
}
