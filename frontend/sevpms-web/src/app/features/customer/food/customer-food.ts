import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { BookingSummary, FoodMenuItemDto, FoodOrderDto, FoodStallDto } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';

@Component({selector:'app-customer-food',imports:[CommonModule,FormsModule,RouterLink],templateUrl:'./customer-food.html',styleUrl:'./customer-food.scss'})
export class CustomerFoodComponent implements OnInit{
 private readonly domain=inject(DomainApiService); private readonly route=inject(ActivatedRoute);
 readonly loading=signal(true); readonly error=signal(''); readonly success=signal(''); readonly bookings=signal<BookingSummary[]>([]); readonly stalls=signal<FoodStallDto[]>([]); readonly menu=signal<FoodMenuItemDto[]>([]); readonly orders=signal<FoodOrderDto[]>([]); readonly busy=signal(false);
 selectedBookingId=''; selectedEventId=''; selectedStallId=''; fulfillmentType='Pickup'; seatLabel=''; readonly quantities=signal<Record<string,number>>({});
 readonly cartTotal=computed(()=>this.menu().reduce((sum,item)=>sum+Number(item.price||0)*Number(this.quantities()[item.menuItemId]||0),0));
 ngOnInit():void{this.loadBase();}
 loadBase():void{this.loading.set(true);this.error.set('');this.domain.bookings().subscribe({next:b=>{this.bookings.set(b.filter(x=>this.bookingStatus(x.status)==='Confirmed'||this.bookingStatus(x.status)==='Completed'));const q=this.route.snapshot.queryParamMap.get('bookingId');const chosen=(q&&this.bookings().some(x=>this.bid(x)===q))?q:this.bid(this.bookings()[0]);this.selectedBookingId=chosen;this.syncEvent();this.domain.foodOrders().subscribe({next:o=>{this.orders.set(o);this.loading.set(false);},error:()=>this.loading.set(false)});},error:e=>{this.loading.set(false);this.error.set(httpErrorMessage(e,'Bookings could not be loaded.'));}});}
 bid(b?:BookingSummary):string{return b?.bookingId??b?.id??'';}
 bookingStatus(v:string|number|undefined):string{return typeof v==='number'?(['Pending','Confirmed','Cancelled','Completed'][v]??String(v)):(v||'');}
 syncEvent():void{const b=this.bookings().find(x=>this.bid(x)===this.selectedBookingId);this.selectedEventId=b?.eventId??'';this.stalls.set([]);this.menu.set([]);this.selectedStallId='';if(!this.selectedEventId)return;this.domain.foodStalls(this.selectedEventId).subscribe({next:s=>{this.stalls.set(s.filter(x=>x.isActive));this.selectedStallId=this.stalls()[0]?.id??'';if(this.selectedStallId)this.loadMenu();},error:e=>this.error.set(httpErrorMessage(e,'Food stalls could not be loaded.'))});}
 loadMenu():void{this.menu.set([]);this.quantities.set({});if(!this.selectedStallId)return;this.domain.foodMenu(this.selectedStallId).subscribe({next:m=>this.menu.set(m.filter(x=>x.isAvailable)),error:e=>this.error.set(httpErrorMessage(e,'The menu could not be loaded.'))});}
 setQty(item:FoodMenuItemDto,delta:number):void{this.quantities.update((current)=>({...current,[item.menuItemId]:Math.max(0,Number(current[item.menuItemId]||0)+delta)}));}
 placeOrder():void{const quantities=this.quantities();const items=this.menu().filter(x=>(quantities[x.menuItemId]||0)>0).map(x=>({menuItemId:x.menuItemId,quantity:quantities[x.menuItemId]}));if(!this.selectedEventId||!this.selectedStallId||!items.length||this.busy())return;this.busy.set(true);this.error.set('');this.success.set('');this.domain.createFoodOrder({eventId:this.selectedEventId,eventFoodStallId:this.selectedStallId,bookingId:this.selectedBookingId||null,fulfillmentType:this.fulfillmentType,seatLabelSnapshot:this.fulfillmentType==='Seat Delivery'?(this.seatLabel||null):null,items}).subscribe({next:o=>{this.busy.set(false);this.success.set(`Order ${o.orderNo} placed successfully.`);this.quantities.set({});this.domain.foodOrders().subscribe({next:v=>this.orders.set(v)});},error:e=>{this.busy.set(false);this.error.set(httpErrorMessage(e,'The food order could not be placed.'));}});}
}
