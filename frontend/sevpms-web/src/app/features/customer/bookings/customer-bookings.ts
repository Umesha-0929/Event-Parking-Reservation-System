import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin, map, of } from 'rxjs';
import { BookingSummary, EventSummary } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';

interface BookingView { booking: BookingSummary; event: EventSummary | null; }

@Component({
  selector: 'app-customer-bookings',
  imports: [CommonModule, RouterLink],
  templateUrl: './customer-bookings.html',
  styleUrl: './customer-bookings.scss',
})
export class CustomerBookingsComponent implements OnInit {
  private readonly domain = inject(DomainApiService);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly items = signal<BookingView[]>([]);
  readonly busyId = signal('');

  ngOnInit(): void { this.load(); }
  load(): void {
    this.loading.set(true); this.error.set('');
    this.domain.bookings().subscribe({
      next: (bookings) => {
        if (!bookings.length) { this.items.set([]); this.loading.set(false); return; }
        forkJoin(bookings.map((booking) => booking.eventId
          ? this.domain.event(booking.eventId).pipe(map((event) => ({ booking, event })))
          : of({ booking, event: null })))
          .subscribe({ next: (items) => { this.items.set(items); this.loading.set(false); }, error: () => { this.items.set(bookings.map((booking) => ({ booking, event: null }))); this.loading.set(false); } });
      },
      error: (error) => { this.error.set(httpErrorMessage(error, 'Your bookings could not be loaded.')); this.loading.set(false); },
    });
  }

  id(item: BookingView): string { return item.booking.bookingId ?? item.booking.id ?? ''; }
  status(value: string | number | undefined): string {
    if (typeof value === 'number') return ['Pending', 'Confirmed', 'Cancelled', 'Completed'][value] ?? String(value);
    return value || 'Pending';
  }
  canCancel(item: BookingView): boolean { return ['Pending', 'Confirmed'].includes(this.status(item.booking.status)); }
  cancel(item: BookingView): void {
    const id = this.id(item); if (!id || this.busyId() || !confirm('Cancel this booking?')) return;
    this.busyId.set(id); this.error.set('');
    this.domain.cancelBooking(id).subscribe({ next: () => { this.busyId.set(''); this.load(); }, error: (error) => { this.busyId.set(''); this.error.set(httpErrorMessage(error, 'The booking could not be cancelled.')); } });
  }
}
