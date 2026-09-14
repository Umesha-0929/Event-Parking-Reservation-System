import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { ReportsService } from '../../core/services/reports.service';
import { OrganizerService } from '../../core/services/organizer.service';
import { EventsService } from '../../core/services/events.service';
import { BookingDto, EventDto, ManualPaymentReviewDto, OrganizerReportDto } from '../../core/models/api.models';
import { PageStateComponent } from '../../shared/components/page-state.component';

@Component({
  selector:'app-organizer-bookings',
  standalone:true,
  imports:[DatePipe,DecimalPipe,FormsModule,PageStateComponent],
  template:`
  <div class="nv-page space-y-6">
    <div>
      <p class="font-extrabold text-sm nv-muted">Sales & Finance</p>
      <h1 class="nv-page-title">Bookings & Payments</h1>
      <p class="nv-muted mt-1">Monitor event bookings and review customer payment proofs.</p>
    </div>

    @if(report()){
      <div class="grid sm:grid-cols-2 xl:grid-cols-4 gap-4">
        <article class="nv-card p-5"><div class="nv-muted text-sm">Confirmed bookings</div><div class="text-3xl font-black mt-2">{{report()!.confirmedBookings}}</div></article>
        <article class="nv-card p-5"><div class="nv-muted text-sm">Attendance</div><div class="text-3xl font-black mt-2">{{report()!.attendance}}</div></article>
        <article class="nv-card p-5"><div class="nv-muted text-sm">Revenue</div><div class="text-3xl font-black mt-2">LKR {{report()!.revenue|number:'1.0-0'}}</div></article>
        <article class="nv-card p-5"><div class="nv-muted text-sm">Parking reservations</div><div class="text-3xl font-black mt-2">{{report()!.parkingReservations}}</div></article>
      </div>
    }

    <section class="nv-card p-5 md:p-6">
      <div class="flex flex-col md:flex-row md:items-end gap-3">
        <div class="flex-1">
          <label class="nv-label" for="booking-search">Search bookings</label>
          <input id="booking-search" class="nv-input" [(ngModel)]="query" name="bookingQuery" placeholder="Booking number or event" />
        </div>
        <div class="md:w-52">
          <label class="nv-label" for="booking-status">Status</label>
          <select id="booking-status" class="nv-input" [(ngModel)]="status" name="bookingStatus">
            <option value="All">All</option>
            <option value="0">Pending</option>
            <option value="1">Confirmed</option>
            <option value="2">Cancelled</option>
            <option value="3">Completed</option>
          </select>
        </div>
        <button type="button" class="nv-btn nv-btn-secondary" (click)="load()">Refresh</button>
      </div>

      @if(loading()){
        <div class="space-y-3 mt-5">@for(i of [1,2,3];track i){<div class="nv-skeleton h-20"></div>}</div>
      } @else if(filteredBookings.length){
        <div class="overflow-x-auto mt-5">
          <table class="w-full text-sm">
            <caption class="sr-only">Bookings for organizer-owned events</caption>
            <thead><tr class="text-left nv-muted"><th class="py-3 pr-4">Booking</th><th class="py-3 pr-4">Event</th><th class="py-3 pr-4">Customer</th><th class="py-3 pr-4">Seats</th><th class="py-3 pr-4">Amount</th><th class="py-3 pr-4">Status</th><th class="py-3">Created</th></tr></thead>
            <tbody>
              @for(b of filteredBookings;track b.bookingId){
                <tr class="border-t" style="border-color:var(--border)">
                  <td class="py-4 pr-4"><div class="font-extrabold">{{b.bookingNumber}}</div><div class="text-xs nv-muted mt-1">{{short(b.bookingId)}}</div></td>
                  <td class="py-4 pr-4">{{eventName(b.eventId)}}</td>
                  <td class="py-4 pr-4">{{short(b.customerUserId)}}</td>
                  <td class="py-4 pr-4">{{b.seatIds.length}}</td>
                  <td class="py-4 pr-4">LKR {{b.totalAmount|number:'1.0-2'}}</td>
                  <td class="py-4 pr-4"><span class="nv-status" [class.success]="b.status===1||b.status===3">{{bookingStatusLabel(b.status)}}</span></td>
                  <td class="py-4">{{b.createdAtUtc|date:'medium'}}</td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      } @else {
        <div class="mt-5"><app-page-state title="No matching bookings" message="Bookings for your events will appear here as customers complete reservations." /></div>
      }
    </section>

    <section class="nv-card p-5 md:p-6">
      <div class="flex items-center justify-between gap-3">
        <div><h2 class="font-black text-xl">Manual payment review</h2><p class="nv-muted text-sm mt-1">Review payment proofs that are waiting for organizer approval.</p></div>
        <button type="button" class="nv-btn nv-btn-secondary" (click)="loadPayments()">Refresh</button>
      </div>
      <div class="overflow-x-auto mt-5">
        <table class="w-full text-sm">
          <caption class="sr-only">Manual payment proofs for organizer events</caption>
          <thead><tr class="text-left nv-muted"><th class="py-3 pr-4">Payment</th><th class="py-3 pr-4">Event / Booking</th><th class="py-3 pr-4">Amount</th><th class="py-3 pr-4">Proof</th><th class="py-3">Actions</th></tr></thead>
          <tbody>
            @for(p of payments();track p.paymentId){
              <tr class="border-t" style="border-color:var(--border)">
                <td class="py-4 pr-4"><div class="font-bold">{{short(p.paymentId)}}</div><div class="text-xs nv-muted mt-1">Submitted {{p.submittedAtUtc|date:'medium'}}</div></td>
                <td class="py-4 pr-4"><div class="font-bold">{{eventName(p.eventId)}}</div><div class="text-xs nv-muted mt-1">Booking {{short(p.bookingId)}}</div></td>
                <td class="py-4 pr-4">{{p.currency}} {{p.amount|number:'1.0-2'}}</td>
                <td class="py-4 pr-4">@if(p.proofUrl){<a class="font-extrabold underline underline-offset-4" [href]="p.proofUrl" target="_blank" rel="noopener noreferrer">View Proof</a>}@else{<span class="nv-muted">No proof URL</span>}</td>
                <td class="py-4"><div class="flex gap-2"><button type="button" class="nv-btn nv-btn-primary" (click)="approve(p.paymentId)">Approve</button><button type="button" class="nv-btn nv-btn-secondary" (click)="reject(p.paymentId)">Reject</button></div></td>
              </tr>
            } @empty {<tr><td colspan="5" class="py-12 text-center nv-muted">No payment proofs are waiting for review.</td></tr>}
          </tbody>
        </table>
      </div>
    </section>

    @if(message()){<div class="fixed right-4 bottom-24 lg:bottom-4 nv-card px-4 py-3" role="status">{{message()}}</div>}
  </div>`
})
export class OrganizerBookingsComponent implements OnInit {
  private reports=inject(ReportsService);
  private ops=inject(OrganizerService);
  private eventsApi=inject(EventsService);

  report=signal<OrganizerReportDto|null>(null);
  payments=signal<ManualPaymentReviewDto[]>([]);
  bookings=signal<BookingDto[]>([]);
  events=signal<EventDto[]>([]);
  loading=signal(true);
  message=signal('');
  query='';
  status='All';

  get filteredBookings(){
    const q=this.query.trim().toLowerCase();
    return this.bookings().filter(b=>{
      if(this.status!=='All'&&String(b.status)!==this.status)return false;
      if(!q)return true;
      return [b.bookingNumber,this.eventName(b.eventId),b.customerUserId].some(v=>v.toLowerCase().includes(q));
    });
  }

  ngOnInit(){this.load();}

  load(){
    this.loading.set(true);
    forkJoin({report:this.reports.organizer(),bookings:this.ops.bookings(),events:this.eventsApi.mine(),payments:this.ops.manualPayments()}).subscribe({
      next:v=>{this.report.set(v.report);this.bookings.set(v.bookings);this.events.set(v.events);this.payments.set(v.payments);this.loading.set(false);},
      error:()=>{this.loading.set(false);this.message.set('Organizer booking data could not be loaded.');}
    });
  }

  loadPayments(){this.ops.manualPayments().subscribe({next:v=>this.payments.set(v),error:()=>this.payments.set([])});}
  approve(id:string){this.ops.approvePayment(id).subscribe({next:()=>{this.message.set('Payment approved.');this.loadPayments();this.loadBookingsOnly();},error:e=>this.message.set(e?.error?.message||e?.error?.error||'Payment could not be approved.')});}
  reject(id:string){this.ops.rejectPayment(id).subscribe({next:()=>{this.message.set('Payment rejected.');this.loadPayments();},error:e=>this.message.set(e?.error?.message||e?.error?.error||'Payment could not be rejected.')});}
  loadBookingsOnly(){this.ops.bookings().subscribe({next:v=>this.bookings.set(v)});}
  eventName(eventId:string){return this.events().find(e=>e.eventId===eventId)?.title??`Event ${this.short(eventId)}`;}
  bookingStatusLabel(v:number){return ['Pending','Confirmed','Cancelled','Completed'][Number(v)]??String(v);}
  short(value:unknown){const s=String(value??'');return s.length>12?`${s.slice(0,8)}…${s.slice(-4)}`:(s||'—');}
}
