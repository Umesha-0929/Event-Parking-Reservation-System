import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { BookingsService } from '../../core/services/bookings.service';
import { BookingDto } from '../../core/models/api.models';
import { PageStateComponent } from '../../shared/components/page-state.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog.component';

@Component({
  selector: 'app-bookings-page',
  standalone: true,
  imports: [DatePipe, DecimalPipe, RouterLink, PageStateComponent, StatusBadgeComponent, ConfirmDialogComponent],
  template: `
  <div class="nv-page space-y-6">
    <section class="nv-hero min-h-[190px]"><div class="relative z-10"><p class="font-extrabold text-sm opacity-75">Account</p><h1 class="nv-page-title mt-1">My Bookings</h1><p class="mt-2 opacity-80">Review event bookings, complete payments and manage confirmed reservations.</p></div></section>

    @if(loading()){
      <div class="space-y-3">@for(i of [1,2,3];track i){<div class="nv-skeleton h-36"></div>}</div>
    }@else if(error()){
      <app-page-state kind="error" title="Bookings unavailable" message="We couldn't load your bookings." actionLabel="Retry" [action]="retry"/>
    }@else if(bookings().length){
      <div class="space-y-3">
        @for(b of bookings();track b.bookingId){
          <article class="nv-card p-5 flex flex-col lg:flex-row lg:items-center gap-4">
            <div class="flex-1 min-w-0">
              <div class="flex flex-wrap items-center gap-2"><h2 class="font-black">{{b.bookingNumber}}</h2><app-status-badge [label]="status(b.status)" [tone]="tone(b.status)"/></div>
              <p class="nv-muted text-sm mt-2">Created {{b.createdAtUtc|date:'MMM d, y · h:mm a'}} · {{b.seatIds.length}} seat{{b.seatIds.length===1?'':'s'}}</p>
              <p class="font-extrabold mt-2">LKR {{b.totalAmount|number:'1.0-2'}}</p>
              @if(b.confirmedAtUtc){<p class="text-xs nv-muted mt-1">Confirmed {{b.confirmedAtUtc|date:'medium'}}</p>}
              @if(b.cancelledAtUtc){<p class="text-xs mt-1" style="color:var(--danger)">Cancelled {{b.cancelledAtUtc|date:'medium'}}</p>}
            </div>
            <div class="flex flex-wrap gap-2 lg:justify-end">
              <a [routerLink]="['/app/events',b.eventId]" class="nv-btn nv-btn-secondary">Event Details</a>
              @if(b.status===0){
                <a [routerLink]="['/app/payments',b.bookingId]" class="nv-btn nv-btn-primary">Complete Payment</a>
                <button type="button" class="nv-btn nv-btn-secondary" (click)="cancelPending(b)">Cancel</button>
              }@else if(b.status===1){
                <a routerLink="/app/tickets" class="nv-btn nv-btn-primary">View Tickets</a>
                <button type="button" class="nv-btn nv-btn-secondary" (click)="openCalendar(b)">Add to Calendar</button>
                <button type="button" class="nv-btn nv-btn-secondary" (click)="downloadCalendar(b)">Download .ics</button>
                <button type="button" class="nv-btn nv-btn-secondary" (click)="cancelConfirmed(b)">Cancel & Refund</button>
              }@else if(b.status===3){
                <a routerLink="/app/tickets" class="nv-btn nv-btn-secondary">View Tickets</a>
              }
            </div>
          </article>
        }
      </div>
    }@else{
      <app-page-state title="No bookings yet" message="Your event bookings will appear here after you select seats." actionLabel="Explore Events" [action]="explore"/>
    }

    @if(confirmation()){<app-confirm-dialog [title]="confirmation()!.title" [message]="confirmation()!.message" [confirmLabel]="confirmation()!.label" tone="danger" (confirmed)="runConfirmation()" (cancelled)="confirmation.set(null)"/>}
    @if(message()){<div class="fixed right-4 bottom-24 lg:bottom-4 nv-card px-4 py-3 text-sm max-w-md" role="status">{{message()}}</div>}
  </div>`
})
export class BookingsPageComponent implements OnInit {
  private api = inject(BookingsService);
  private router = inject(Router);
  bookings = signal<BookingDto[]>([]);
  loading = signal(true);
  error = signal(false);
  message = signal('');
  confirmation = signal<{title:string;message:string;label:string;action:()=>void}|null>(null);
  retry = () => this.load();
  explore = () => this.router.navigateByUrl('/app/events');

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    this.error.set(false);
    this.api.list().subscribe({
      next: value => { this.bookings.set(value); this.loading.set(false); },
      error: () => { this.error.set(true); this.loading.set(false); }
    });
  }

  status(value: number) { return ['Pending', 'Confirmed', 'Cancelled', 'Completed'][value] ?? 'Unknown'; }
  tone(value: number): 'success' | 'warning' | 'danger' | 'neutral' { return value === 1 || value === 3 ? 'success' : value === 2 ? 'danger' : value === 0 ? 'warning' : 'neutral'; }

  cancelPending(booking: BookingDto) {
    this.confirmation.set({title:'Cancel pending booking?',message:'The selected seats will be released and this pending booking will be cancelled.',label:'Cancel Booking',action:()=>this.api.cancel(booking.bookingId).subscribe({
      next: value => this.bookings.update(rows => rows.map(row => row.bookingId === value.bookingId ? value : row)),
      error: err => this.message.set(err?.error?.message || err?.error?.error || 'Booking could not be cancelled.')
    })});
  }

  cancelConfirmed(booking: BookingDto) {
    this.confirmation.set({title:'Cancel confirmed booking?',message:'This will cancel the booking and start the available sandbox refund flow.',label:'Cancel & Refund',action:()=>this.api.cancelConfirmed(booking.bookingId).subscribe({
      next: refund => {
        this.message.set(`Booking cancelled. Refund ${refund?.refundReference ?? ''} completed.`.trim());
        this.load();
      },
      error: err => this.message.set(err?.error?.message || err?.error?.error || 'Confirmed booking could not be cancelled.')
    })});
  }

  runConfirmation(){const current=this.confirmation();this.confirmation.set(null);current?.action();}

  openCalendar(booking: BookingDto) {
    this.api.calendar(booking.bookingId).subscribe({
      next: info => {
        if (info.googleCalendarUrl) window.open(info.googleCalendarUrl, '_blank', 'noopener');
        else this.message.set('Calendar link is not available for this booking.');
      },
      error: err => this.message.set(err?.error?.message || err?.error?.error || 'Calendar export is not available for this booking.')
    });
  }

  downloadCalendar(booking: BookingDto) {
    this.api.calendarIcs(booking.bookingId).subscribe({
      next: blob => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `${booking.bookingNumber}.ics`;
        a.click();
        URL.revokeObjectURL(url);
      },
      error: err => this.message.set(err?.error?.message || 'Calendar file could not be downloaded.')
    });
  }
}
