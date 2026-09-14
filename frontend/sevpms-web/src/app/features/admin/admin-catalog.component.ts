import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../core/services/admin.service';
import { EventCategory, VenueFacility } from '../../core/models/api.models';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog.component';

@Component({
  selector:'app-admin-catalog',
  standalone:true,
  imports:[FormsModule,ConfirmDialogComponent],
  template:`
  <div class="nv-page space-y-6">
    <div><p class="font-extrabold text-sm nv-muted">Platform Configuration</p><h1 class="nv-page-title">Catalog</h1><p class="nv-muted mt-1">Manage event categories and venue facilities used by event setup and marketplace filters.</p></div>
    <div class="grid xl:grid-cols-2 gap-5">
      <section class="nv-card p-5 md:p-6">
        <div><h2 class="font-black text-xl">Event Categories</h2><p class="nv-muted text-sm mt-1">Create or activate/deactivate category options.</p></div>
        <form class="grid sm:grid-cols-[1fr_180px_auto] gap-2 mt-5" (ngSubmit)="saveCategory()"><input class="nv-input" name="catName" [(ngModel)]="categoryName" placeholder="Category name" required aria-label="Category name"><input class="nv-input" name="catCode" [(ngModel)]="categoryCode" placeholder="Code" required aria-label="Code"><button type="submit" class="nv-btn nv-btn-primary">{{categoryEditId?'Save':'Add'}}</button></form>
        <div class="space-y-2 mt-4">@for(c of categories();track c.eventCategoryId){<div class="nv-card-soft p-4 flex items-center gap-3"><div class="flex-1"><div class="font-extrabold">{{c.name}}</div><div class="nv-muted text-xs mt-1">{{c.code}}</div></div><span class="nv-status" [class.success]="c.isActive">{{c.isActive?'Active':'Inactive'}}</span><button type="button" class="text-sm font-bold" (click)="editCategory(c)">Edit</button><button type="button" class="text-sm nv-muted" (click)="toggleCategory(c)">{{c.isActive?'Deactivate':'Activate'}}</button></div>}@empty{<p class="nv-muted text-sm py-8 text-center">No event categories are configured.</p>}</div>
      </section>

      <section class="nv-card p-5 md:p-6">
        <div><h2 class="font-black text-xl">Venue Facilities</h2><p class="nv-muted text-sm mt-1">Reusable facility options for venue listings.</p></div>
        <form class="grid sm:grid-cols-[1fr_180px_auto] gap-2 mt-5" (ngSubmit)="saveFacility()"><input class="nv-input" name="facName" [(ngModel)]="facilityName" placeholder="Facility name" required aria-label="Facility name"><input class="nv-input" name="facCat" [(ngModel)]="facilityCategory" placeholder="Category" required aria-label="Category"><button type="submit" class="nv-btn nv-btn-primary">{{facilityEditId?'Save':'Add'}}</button></form>
        <div class="space-y-2 mt-4">@for(f of facilities();track f.facilityId){<div class="nv-card-soft p-4 flex items-center gap-3"><div class="flex-1"><div class="font-extrabold">{{f.name}}</div><div class="nv-muted text-xs mt-1">{{f.category}}</div></div><span class="nv-status" [class.success]="f.isActive">{{f.isActive?'Active':'Inactive'}}</span><button type="button" class="text-sm font-bold" (click)="editFacility(f)">Edit</button>@if(!f.isActive){<button type="button" class="text-sm nv-muted" (click)="deleteFacility(f)">Delete</button>}</div>}@empty{<p class="nv-muted text-sm py-8 text-center">No venue facilities are configured.</p>}</div>
      </section>
    </div>
    @if(confirmation()){<app-confirm-dialog [title]="confirmation()!.title" [message]="confirmation()!.message" [confirmLabel]="confirmation()!.label" tone="danger" (confirmed)="runConfirmation()" (cancelled)="confirmation.set(null)"/>}
    @if(message()){<div class="fixed right-4 bottom-4 nv-card px-4 py-3" role="status">{{message()}}</div>}
  </div>`
})
export class AdminCatalogComponent implements OnInit{
  private api=inject(AdminService);categories=signal<EventCategory[]>([]);facilities=signal<VenueFacility[]>([]);message=signal('');confirmation=signal<{title:string;message:string;label:string;action:()=>void}|null>(null);
  categoryName='';categoryCode='';categoryEditId='';facilityName='';facilityCategory='';facilityEditId='';
  ngOnInit(){this.load();}
  load(){this.api.categoriesAdmin().subscribe({next:v=>this.categories.set(v),error:()=>this.categories.set([])});this.api.facilitiesAdmin().subscribe({next:v=>this.facilities.set(v),error:()=>this.facilities.set([])});}
  saveCategory(){const body={name:this.categoryName.trim(),code:this.categoryCode.trim(),isActive:true};if(!body.name||!body.code)return;const req=this.categoryEditId?this.api.updateCategory(this.categoryEditId,body):this.api.createCategory(body);req.subscribe({next:()=>{this.message.set(this.categoryEditId?'Category updated.':'Category created.');this.categoryName='';this.categoryCode='';this.categoryEditId='';this.load();},error:e=>this.message.set(e?.error?.message||e?.error?.error||'Category could not be saved.')});}
  editCategory(c:EventCategory){this.categoryEditId=c.eventCategoryId;this.categoryName=c.name;this.categoryCode=c.code;}
  toggleCategory(c:EventCategory){if(c.isActive){this.api.deactivateCategory(c.eventCategoryId).subscribe({next:()=>this.load(),error:e=>this.message.set(e?.error?.message||'Category could not be deactivated.')});}else{this.api.updateCategory(c.eventCategoryId,{name:c.name,code:c.code,isActive:true}).subscribe({next:()=>this.load(),error:e=>this.message.set(e?.error?.message||'Category could not be activated.')});}}
  saveFacility(){const body={name:this.facilityName.trim(),category:this.facilityCategory.trim(),isActive:true};if(!body.name||!body.category)return;const req=this.facilityEditId?this.api.updateFacility(this.facilityEditId,body):this.api.createFacility(body);req.subscribe({next:()=>{this.message.set(this.facilityEditId?'Facility updated.':'Facility created.');this.facilityName='';this.facilityCategory='';this.facilityEditId='';this.load();},error:e=>this.message.set(e?.error?.message||e?.error?.error||'Facility could not be saved.')});}
  editFacility(f:VenueFacility){this.facilityEditId=f.facilityId;this.facilityName=f.name;this.facilityCategory=f.category;}
  deleteFacility(f:VenueFacility){this.confirmation.set({title:'Permanently delete facility?',message:`${f.name} will be removed permanently if it is not in use.`,label:'Delete Facility',action:()=>this.api.deleteFacility(f.facilityId).subscribe({next:()=>{this.message.set('Facility deleted.');this.load();},error:e=>this.message.set(e?.error?.message||e?.error?.error||'Facility could not be deleted.')})});}
  runConfirmation(){const current=this.confirmation();this.confirmation.set(null);current?.action();}
}
