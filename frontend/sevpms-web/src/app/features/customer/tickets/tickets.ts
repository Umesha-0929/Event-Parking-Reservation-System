import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { CustomerTicketSummary } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';
import { QrCodeComponent } from '../../../shared/components/qr-code/qr-code';

@Component({
  selector: 'app-tickets',
  imports: [QrCodeComponent, DatePipe],
  templateUrl: './tickets.html',
  styleUrl: './tickets.scss',
})
export class TicketsComponent {
  private readonly domain = inject(DomainApiService);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly items = signal<CustomerTicketSummary[]>([]);
  readonly selected = signal<CustomerTicketSummary | null>(null);

  constructor() { this.load(); }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    this.domain.myTickets().subscribe({
      next: (items) => {
        this.items.set(items);
        this.loading.set(false);
      },
      error: (error) => {
        this.error.set(httpErrorMessage(error, 'Tickets could not be loaded.'));
        this.loading.set(false);
      },
    });
  }

  qr(item: CustomerTicketSummary): string { return item.qrPayload ?? ''; }
  eventTitle(item: CustomerTicketSummary): string { return item.eventName || 'Event ticket'; }
  venueLabel(item: CustomerTicketSummary): string { return item.venueName || 'Venue details in booking'; }

  seatLabel(item: CustomerTicketSummary): string {
    if (item.rowLabel && item.seatNumber) return `${item.rowLabel}${item.seatNumber}`;
    return item.seatId ? 'Assigned seat' : 'General admission';
  }

  statusKey(item: CustomerTicketSummary): string {
    return String(item.status ?? '').trim().toLowerCase();
  }
}
