import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { catchError, of } from 'rxjs';
import { EventsService } from '../../core/services/events.service';
import { SeatCategory, SeatSection, SeatingLayoutDto } from '../../core/models/api.models';

@Component({
  selector:'app-organizer-seating', standalone:true, imports:[FormsModule,RouterLink],
  template:`
  <div class="nv-page space-y-6">
    <div class="flex flex-wrap items-start justify-between gap-3">
      <div><p class="font-extrabold text-sm nv-muted">Host Event</p><h1 class="nv-page-title">Stage & Seating</h1><p class="nv-muted mt-1">Configure the published 2D layout customers will book against.</p></div>
      <a routerLink="/organizer/events" class="nv-btn nv-btn-secondary">My Events</a>
    </div>

    @if(!eventId){<div class="nv-card p-8"><h2 class="font-black text-xl">Choose an event first</h2><p class="nv-muted mt-2">Open an event from My Events and continue to Stage & Seating.</p></div>}
    @else{
    <div class="grid xl:grid-cols-[1fr_360px] gap-5">
      <section class="nv-card p-5 md:p-6 space-y-6">
        <div class="flex flex-wrap gap-2">
          @for(s of steps; track s){<span class="nv-chip" [class.active]="s==='Stage & Seating' || (s==='Pricing' && categories().length)">{{s}}</span>}
        </div>

        <div class="grid lg:grid-cols-2 gap-4">
          <div><label class="nv-label" for="stage">Stage type</label><select id="stage" class="nv-input" [(ngModel)]="stageType" aria-label="Stage type">
            @for(s of stages;track s.value){<option [ngValue]="s.value">{{s.label}}</option>}
          </select></div>
          <div class="grid grid-cols-2 gap-3"><div><label class="nv-label" for="rows">Rows</label><input id="rows" class="nv-input" type="number" min="1" [(ngModel)]="rowCount" aria-label="Row count"></div><div><label class="nv-label" for="cols">Columns</label><input id="cols" class="nv-input" type="number" min="1" [(ngModel)]="columnCount" aria-label="Column count"></div></div>
        </div>

        <div class="rounded-[28px] border p-4 min-h-[430px] relative overflow-hidden" style="border-color:var(--border);background:linear-gradient(180deg,color-mix(in srgb,var(--surface-soft) 80%,transparent),var(--surface))">
          <div class="absolute rounded-2xl border flex items-center justify-center font-black text-xs tracking-[.2em] uppercase" [style.left.%]="stageX" [style.top.%]="stageY" [style.width.%]="stageWidth" [style.height.%]="stageHeight" style="background:var(--surface-elevated);border-color:var(--accent)">Stage</div>
          @for(section of sections();track section.id){<div class="absolute rounded-3xl border p-3 overflow-hidden" [style.left.%]="section.x" [style.top.%]="section.y" [style.width.%]="section.width" [style.height.%]="section.height" style="border-color:var(--border);background:color-mix(in srgb,var(--accent) 8%,var(--surface))"><div class="font-black text-xs">{{section.code}} · {{section.name}}</div><div class="text-[11px] nv-muted mt-1">{{section.rowCount}} rows × {{section.columnCount}} seats</div><div class="grid gap-1 mt-3" [style.grid-template-columns]="'repeat('+previewCols(section.columnCount)+',minmax(0,1fr))'">@for(x of previewSeats(section);track $index){<span class="aspect-square rounded-full border" style="border-color:var(--border);background:var(--surface-elevated)"></span>}</div></div>}
          @if(!sections().length){<div class="absolute inset-0 grid place-items-center"><div class="text-center max-w-xs"><div class="text-4xl">◫</div><h3 class="font-black mt-3">Layout canvas ready</h3><p class="nv-muted text-sm mt-1">Save the stage settings, then add seat sections and pricing categories.</p></div></div>}
        </div>
        <div class="flex flex-wrap gap-2"><button type="button" class="nv-btn nv-btn-primary" [disabled]="busy()" (click)="saveLayout()">Save Layout</button><button type="button" class="nv-btn nv-btn-secondary" [disabled]="busy() || !sections().length || !categories().length" (click)="publishLayout()">Publish Customer Layout</button></div>
      </section>

      <aside class="space-y-4">
        <section class="nv-card p-5"><h2 class="font-black text-lg">Section</h2><p class="nv-muted text-sm mt-1">Create or update a seating block.</p>
          <div class="grid gap-3 mt-4"><input class="nv-input" placeholder="Section name" [(ngModel)]="sectionName" aria-label="Section name"><input class="nv-input" placeholder="Code, e.g. A" [(ngModel)]="sectionCode" aria-label="Code, e.g. A"><div class="grid grid-cols-2 gap-2"><input class="nv-input" type="number" min="1" placeholder="Rows" [(ngModel)]="sectionRows" aria-label="Rows"><input class="nv-input" type="number" min="1" placeholder="Columns" [(ngModel)]="sectionCols" aria-label="Columns"></div><div class="grid grid-cols-2 gap-2"><input class="nv-input" type="number" placeholder="X %" [(ngModel)]="sectionX" aria-label="X %"><input class="nv-input" type="number" placeholder="Y %" [(ngModel)]="sectionY" aria-label="Y %"></div><div class="grid grid-cols-2 gap-2"><input class="nv-input" type="number" placeholder="Width %" [(ngModel)]="sectionWidth" aria-label="Width %"><input class="nv-input" type="number" placeholder="Height %" [(ngModel)]="sectionHeight" aria-label="Height %"></div><label class="flex items-center gap-2 text-sm font-bold"><input type="checkbox" [(ngModel)]="sectionAccessible" aria-label="Section accessible"> Accessible section</label><button type="button" class="nv-btn nv-btn-secondary" (click)="saveSection()">Add Section</button></div>
        </section>

        <section class="nv-card p-5"><h2 class="font-black text-lg">Pricing category</h2><div class="grid gap-3 mt-4"><input class="nv-input" placeholder="Category name" [(ngModel)]="categoryName" aria-label="Category name"><input class="nv-input" placeholder="Code" [(ngModel)]="categoryCode" aria-label="Code"><input class="nv-input" type="number" min="0" placeholder="Price (LKR)" [(ngModel)]="categoryPrice" aria-label="Price (LKR)"><button type="button" class="nv-btn nv-btn-secondary" (click)="saveCategory()">Add Category</button></div>
          @if(categories().length){<div class="grid gap-2 mt-4">@for(c of categories();track c.id){<div class="nv-card-soft p-3 flex justify-between gap-2"><span class="font-extrabold">{{c.name}}</span><span class="nv-muted">LKR {{c.price}}</span></div>}</div>}
        </section>

        @if(sections().length && categories().length){<section class="nv-card p-5"><h2 class="font-black text-lg">Generate seats</h2><select class="nv-input mt-4" [(ngModel)]="generateSectionId" aria-label="Generate section  id">@for(s of sections();track s.id){<option [value]="s.id">{{s.name}}</option>}</select><select class="nv-input mt-2" [(ngModel)]="generateCategoryId" aria-label="Generate category  id">@for(c of categories();track c.id){<option [value]="c.id">{{c.name}}</option>}</select><button type="button" class="nv-btn nv-btn-primary w-full mt-3" (click)="generateSeats()">Generate Section Seats</button></section>}
      </aside>
    </div>}
    @if(message()){<div class="fixed bottom-4 right-4 nv-card px-4 py-3 max-w-sm" role="status">{{message()}}</div>}
  </div>`
})
export class OrganizerSeatingComponent implements OnInit{
  private route=inject(ActivatedRoute); private events=inject(EventsService);
  eventId=''; busy=signal(false); message=signal(''); sections=signal<SeatSection[]>([]); categories=signal<SeatCategory[]>([]);
  steps=['Event Details','Venue','Stage & Seating','Pricing','Parking','Food','Review'];
  stages=[{value:1,label:'Arena Stage'},{value:2,label:'Proscenium Theatre Stage'},{value:3,label:'End-On Stage'},{value:4,label:'Thrust Stage'},{value:5,label:'Traverse Stage'},{value:6,label:'In-the-Round Stage'}];
  stageType=2; rowCount=10; columnCount=16; stageX=25; stageY=4; stageWidth=50; stageHeight=12;
  sectionName='Section A'; sectionCode='A'; sectionRows=8; sectionCols=12; sectionX=12; sectionY=28; sectionWidth=76; sectionHeight=55; sectionAccessible=false;
  categoryName='Standard'; categoryCode='STD'; categoryPrice=2500; generateSectionId=''; generateCategoryId='';
  ngOnInit(){this.route.queryParamMap.subscribe(params=>{const id=params.get('event')??'';if(id===this.eventId)return;this.eventId=id;if(this.eventId)this.load();});}
  load(){this.events.organizerLayout(this.eventId).pipe(catchError(()=>of(null))).subscribe((l:SeatingLayoutDto|null)=>{if(!l)return;this.stageType=l.stageType;this.rowCount=l.rowCount;this.columnCount=l.columnCount;this.stageX=Number(l.stageX);this.stageY=Number(l.stageY);this.stageWidth=Number(l.stageWidth);this.stageHeight=Number(l.stageHeight);this.sections.set(l.sections??[]);this.categories.set(l.categories??[]);this.generateSectionId=l.sections?.[0]?.id??'';this.generateCategoryId=l.categories?.[0]?.id??'';});}
  saveLayout(){this.busy.set(true);this.events.configureLayout(this.eventId,{stageType:Number(this.stageType),rowCount:Number(this.rowCount),columnCount:Number(this.columnCount),canvasWidth:100,canvasHeight:100,stageX:Number(this.stageX),stageY:Number(this.stageY),stageWidth:Number(this.stageWidth),stageHeight:Number(this.stageHeight)}).subscribe({next:(l:SeatingLayoutDto)=>{this.busy.set(false);this.sections.set(l.sections??[]);this.categories.set(l.categories??[]);this.message.set('Stage layout saved.');},error:e=>{this.busy.set(false);this.message.set(e?.error?.message||'Layout could not be saved.');}});}
  saveSection(){if(!this.sectionName.trim()||!this.sectionCode.trim())return;this.events.setSections(this.eventId,{id:null,name:this.sectionName.trim(),code:this.sectionCode.trim().toUpperCase(),rowCount:Number(this.sectionRows),columnCount:Number(this.sectionCols),x:Number(this.sectionX),y:Number(this.sectionY),width:Number(this.sectionWidth),height:Number(this.sectionHeight),displayOrder:this.sections().length+1,isAccessibleSection:this.sectionAccessible,isEnabled:true}).subscribe({next:(s:SeatSection)=>{this.sections.update(v=>[...v,s]);this.generateSectionId ||= s.id;this.message.set('Section added.');},error:e=>this.message.set(e?.error?.message||'Section could not be saved.')});}
  saveCategory(){if(!this.categoryName.trim()||!this.categoryCode.trim())return;this.events.setCategories(this.eventId,{id:null,name:this.categoryName.trim(),code:this.categoryCode.trim().toUpperCase(),price:Number(this.categoryPrice),displayOrder:this.categories().length+1,isActive:true}).subscribe({next:(c:SeatCategory)=>{this.categories.update(v=>[...v,c]);this.generateCategoryId ||= c.id;this.message.set('Pricing category added.');},error:e=>this.message.set(e?.error?.message||'Category could not be saved.')});}
  generateSeats(){const s=this.sections().find(x=>x.id===this.generateSectionId);if(!s)return;this.events.generateSeats(this.eventId,{sectionId:s.id,seatCategoryId:this.generateCategoryId||null,rowCount:s.rowCount,columnCount:s.columnCount,startingRowLabel:'A',startingSeatNumber:1,startX:Number(s.x)+2,startY:Number(s.y)+5,horizontalSpacing:Math.max(1,Number(s.width)/(s.columnCount+1)),verticalSpacing:Math.max(1,Number(s.height)/(s.rowCount+1)),unavailablePositions:[],accessiblePositions:[],gaps:[]}).subscribe({next:()=>this.message.set('Seats generated for this section.'),error:e=>this.message.set(e?.error?.message||'Seats could not be generated.')});}
  publishLayout(){this.events.publishLayout(this.eventId,true).subscribe({next:()=>this.message.set('Customer seating layout published.'),error:e=>this.message.set(e?.error?.message||'Layout could not be published.')});}
  previewCols(n:number){return Math.min(Math.max(2,n),10)} previewSeats(section:SeatSection){return Array.from({length:Math.min(Number(section.rowCount)*this.previewCols(Number(section.columnCount)),60)});}
}
