import { DecimalPipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MenuItem } from '../../core/models/api.models';

@Component({selector:'app-food-card',standalone:true,imports:[DecimalPipe],template:`
<article class="nv-card overflow-hidden h-full flex flex-col group">
  <div class="aspect-[16/9] relative nv-media-shell">
    <div class="nv-image-fallback food"></div>
    @if(item.imageUrl&&!imageError){<img [src]="item.imageUrl" [alt]="item.name" loading="lazy" decoding="async" class="nv-img relative z-[1]" (error)="imageError=true">}
    <div class="absolute inset-x-0 bottom-0 h-16 z-[2]" style="background:linear-gradient(transparent,rgba(15,23,56,.45))"></div>
    <span class="absolute right-3 top-3 z-[3] px-2.5 py-1 rounded-full bg-white/85 text-[#1d2944] text-xs font-black backdrop-blur-md">LKR {{item.price|number:'1.0-2'}}</span>
  </div>
  <div class="p-4 flex-1 flex flex-col">
    <h3 class="font-black tracking-[-.01em]">{{item.name}}</h3>
    <p class="nv-muted text-sm mt-2 line-clamp-2 flex-1">{{item.description}}</p>
    <button type="button" class="nv-btn nv-btn-secondary w-full mt-4" [disabled]="!item.isAvailable" (click)="added.emit(item)">{{item.isAvailable?'Add to order':'Sold Out'}}</button>
  </div>
</article>`})
export class FoodCardComponent{
  @Input({required:true}) item!:MenuItem;
  @Output() added=new EventEmitter<MenuItem>();
  imageError=false;
}
