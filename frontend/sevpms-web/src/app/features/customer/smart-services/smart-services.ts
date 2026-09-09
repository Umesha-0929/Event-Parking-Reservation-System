import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import {
  BackendCoverageApiService,
  BookingCalendarDto,
  CoverageBooking,
  CoverageEvent,
  EventReviewDto,
  EventWeatherDto,
  ParkingRecommendationDto,
  ParkingNodeDto,
  ParkingRouteDto,
  RatingSummaryDto,
  ReceiptDeliveryDto,
  ReceiptDto,
  RecommendationDto,
  RefundDto,
  SavedVehicleDto,
  VenueLiteDto,
  WaitlistDto,
} from '../../../core/services/backend-coverage-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';

type SmartTab = 'events' | 'bookings' | 'receipts' | 'parking';

@Component({
  selector: 'app-smart-services',
  imports: [FormsModule],
  templateUrl: './smart-services.html',
  styleUrl: './smart-services.scss',
})
export class SmartServicesComponent implements OnInit {
  private readonly api = inject(BackendCoverageApiService);
  private readonly router = inject(Router);

  readonly tab = signal<SmartTab>('events');
  readonly busy = signal(false);
  readonly message = signal('');
  readonly error = signal('');

  readonly events = signal<CoverageEvent[]>([]);
  readonly recommendations = signal<RecommendationDto[]>([]);
  readonly weather = signal<EventWeatherDto | null>(null);
  readonly reviews = signal<EventReviewDto[]>([]);
  readonly rating = signal<RatingSummaryDto | null>(null);
  readonly waitlist = signal<WaitlistDto | null>(null);

  readonly bookings = signal<CoverageBooking[]>([]);
  readonly calendar = signal<BookingCalendarDto | null>(null);
  readonly refund = signal<RefundDto | null>(null);

  readonly receipts = signal<ReceiptDto[]>([]);
  readonly selectedReceipt = signal<ReceiptDto | null>(null);
  readonly deliveries = signal<ReceiptDeliveryDto[]>([]);

  readonly venues = signal<VenueLiteDto[]>([]);
  readonly vehicles = signal<SavedVehicleDto[]>([]);
  readonly parkingRecommendation = signal<ParkingRecommendationDto | null>(null);
  readonly parkingRoute = signal<ParkingRouteDto | null>(null);
  readonly parkingNodes = signal<ParkingNodeDto[]>([]);
  readonly routeNodes = signal<ParkingNodeDto[]>([]);

  selectedEventId = '';
  preferredCategories = '';
  reviewRating = 5;
  reviewTitle = '';
  reviewComment = '';

  selectedBookingId = '';
  cancellationReason = '';

  selectedReceiptId = '';

  parkingVenueId = '';
  parkingEventId = '';
  entranceNodeId = '';
  savedVehicleId = '';
  accessibleParking = false;

  routeVenueId = '';
  routeStartNodeId = '';
  routeEndNodeId = '';
  routeAccessibleOnly = false;

  ngOnInit(): void {
    this.loadReferenceData();
    this.loadRecommendations();
  }

  setTab(tab: SmartTab): void {
    this.tab.set(tab);
    this.clearFeedback();
  }

  loadReferenceData(): void {
    this.api.events().subscribe({
      next: (items) => {
        this.events.set(items ?? []);
        if (!this.selectedEventId && items?.length) {
          this.selectedEventId = this.eventId(items[0]);
        }
      },
      error: () => this.events.set([]),
    });

    this.api.bookings().subscribe({
      next: (items) => {
        this.bookings.set(items ?? []);
        if (!this.selectedBookingId && items?.length) {
          this.selectedBookingId = this.bookingId(items[0]);
        }
      },
      error: () => this.bookings.set([]),
    });

    this.api.receipts().subscribe({
      next: (items) => this.receipts.set(items ?? []),
      error: () => this.receipts.set([]),
    });

    this.api.venues().subscribe({
      next: (items) => {
        this.venues.set(items ?? []);
        if (!this.parkingVenueId && items?.length) {
          this.parkingVenueId = this.venueId(items[0]);
          this.routeVenueId = this.parkingVenueId;
          this.loadParkingNodes(this.parkingVenueId, 'recommend');
          this.loadParkingNodes(this.routeVenueId, 'route');
        }
      },
      error: () => this.venues.set([]),
    });

    this.api.vehicles().subscribe({
      next: (items) => {
        this.vehicles.set(items ?? []);
        const preferred = items?.find((x) => x.isDefault) ?? items?.[0];
        if (preferred) this.savedVehicleId = preferred.id;
      },
      error: () => this.vehicles.set([]),
    });
  }

  loadRecommendations(): void {
    this.clearFeedback();
    const categories = this.preferredCategories
      .split(',')
      .map((x) => x.trim())
      .filter(Boolean);

    this.api.recommendations(categories, 10).subscribe({
      next: (items) => this.recommendations.set(items ?? []),
      error: (e) => this.error.set(httpErrorMessage(e, 'Recommendations could not be loaded.')),
    });
  }

  openRecommendedEvent(eventId: string): void {
    void this.router.navigate(['/customer/browse-events', eventId]);
  }

  loadEventTools(): void {
    this.clearFeedback();
    const eventId = this.selectedEventId.trim();
    if (!eventId) {
      this.error.set('Choose an event first.');
      return;
    }

    this.busy.set(true);
    this.weather.set(null);
    this.reviews.set([]);
    this.rating.set(null);
    this.waitlist.set(null);

    let pending = 4;
    const done = () => {
      pending -= 1;
      if (pending <= 0) this.busy.set(false);
    };

    this.api.weather(eventId).subscribe({
      next: (value) => { this.weather.set(value); done(); },
      error: (e) => { this.error.set(httpErrorMessage(e, 'Weather could not be loaded.')); done(); },
    });

    this.api.reviews(eventId).subscribe({
      next: (value) => { this.reviews.set(value ?? []); done(); },
      error: (e) => { this.error.set(httpErrorMessage(e, 'Reviews could not be loaded.')); done(); },
    });

    this.api.ratingSummary(eventId).subscribe({
      next: (value) => { this.rating.set(value); done(); },
      error: () => done(),
    });

    this.api.waitlistMine(eventId).subscribe({
      next: (value) => { this.waitlist.set(value); done(); },
      error: (e) => {
        if (e?.status !== 404) this.error.set(httpErrorMessage(e, 'Waitlist status could not be loaded.'));
        done();
      },
    });
  }

  joinWaitlist(): void {
    const eventId = this.selectedEventId.trim();
    if (!eventId) return;
    this.clearFeedback();
    this.api.joinWaitlist(eventId).subscribe({
      next: (value) => {
        this.waitlist.set(value);
        this.message.set('You joined the waitlist.');
      },
      error: (e) => this.error.set(httpErrorMessage(e, 'Could not join the waitlist.')),
    });
  }

  leaveWaitlist(): void {
    const eventId = this.selectedEventId.trim();
    if (!eventId) return;
    if (!confirm('Leave this event waitlist?')) return;

    this.clearFeedback();
    this.api.leaveWaitlist(eventId).subscribe({
      next: () => {
        this.waitlist.set(null);
        this.message.set('You left the waitlist.');
      },
      error: (e) => this.error.set(httpErrorMessage(e, 'Could not leave the waitlist.')),
    });
  }

  submitReview(): void {
    const eventId = this.selectedEventId.trim();
    if (!eventId) return;

    const rating = Math.min(5, Math.max(1, Number(this.reviewRating) || 0));
    if (!rating) {
      this.error.set('Choose a rating from 1 to 5.');
      return;
    }

    this.clearFeedback();
    this.api.createReview(eventId, {
      rating,
      title: this.reviewTitle.trim() || undefined,
      comment: this.reviewComment.trim() || undefined,
    }).subscribe({
      next: (review) => {
        this.reviews.set([review, ...this.reviews()]);
        this.reviewTitle = '';
        this.reviewComment = '';
        this.message.set('Review submitted.');
        this.api.ratingSummary(eventId).subscribe({ next: (value) => this.rating.set(value) });
      },
      error: (e) => this.error.set(httpErrorMessage(e, 'The review could not be submitted.')),
    });
  }

  loadBookingCalendar(): void {
    this.clearFeedback();
    this.calendar.set(null);
    this.refund.set(null);
    const bookingId = this.selectedBookingId.trim();
    if (!bookingId) {
      this.error.set('Choose a booking first.');
      return;
    }

    this.api.bookingCalendar(bookingId).subscribe({
      next: (value) => this.calendar.set(value),
      error: (e) => this.error.set(httpErrorMessage(e, 'Calendar information could not be loaded.')),
    });
  }

  downloadIcs(): void {
    const bookingId = this.selectedBookingId.trim();
    if (!bookingId) return;

    this.clearFeedback();
    this.api.downloadCalendar(bookingId).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `nvent-booking-${bookingId}.ics`;
        a.click();
        URL.revokeObjectURL(url);
        this.message.set('Calendar file downloaded.');
      },
      error: (e) => this.error.set(httpErrorMessage(e, 'Calendar download failed.')),
    });
  }

  cancelConfirmedBooking(): void {
    const booking = this.selectedBooking();
    if (!booking) {
      this.error.set('Choose a booking first.');
      return;
    }

    if (!this.isConfirmedBooking(booking)) {
      this.error.set('Only a confirmed booking can use the cancellation + refund flow.');
      return;
    }

    const reason = this.cancellationReason.trim();
    if (reason.length < 3) {
      this.error.set('Enter a short cancellation reason.');
      return;
    }

    if (!confirm('Cancel this confirmed booking and start the refund flow?')) return;

    this.clearFeedback();
    this.api.cancelConfirmedBooking(this.bookingId(booking), reason).subscribe({
      next: (value) => {
        this.refund.set(value);
        this.message.set('Booking cancelled. Refund information is shown below.');
        this.loadReferenceData();
      },
      error: (e) => this.error.set(httpErrorMessage(e, 'Cancellation/refund could not be completed.')),
    });
  }

  chooseReceipt(receiptId: string): void {
    this.selectedReceiptId = receiptId;
    this.clearFeedback();
    this.selectedReceipt.set(null);
    this.deliveries.set([]);

    this.api.receipt(receiptId).subscribe({
      next: (value) => this.selectedReceipt.set(value),
      error: (e) => this.error.set(httpErrorMessage(e, 'Receipt details could not be loaded.')),
    });

    this.api.receiptDeliveries(receiptId).subscribe({
      next: (value) => this.deliveries.set(value ?? []),
      error: (e) => this.error.set(httpErrorMessage(e, 'Receipt delivery history could not be loaded.')),
    });
  }

  retryReceipt(): void {
    const id = this.selectedReceiptId.trim();
    if (!id) return;
    this.clearFeedback();

    this.api.retryReceiptDelivery(id).subscribe({
      next: (value) => {
        this.deliveries.set(value ?? []);
        this.message.set('Receipt delivery retry requested.');
      },
      error: (e) => this.error.set(httpErrorMessage(e, 'Receipt delivery could not be retried.')),
    });
  }

  loadParkingNodes(venueId: string, target: 'recommend' | 'route'): void {
    if (!venueId) {
      if (target === 'recommend') { this.parkingNodes.set([]); this.entranceNodeId = ''; }
      else { this.routeNodes.set([]); this.routeStartNodeId = ''; this.routeEndNodeId = ''; }
      return;
    }
    this.api.parkingNodes(venueId).subscribe({
      next: (nodes) => {
        const items = nodes ?? [];
        if (target === 'recommend') {
          this.parkingNodes.set(items);
          const entrance = items.find((x) => x.nodeType.toLowerCase().includes('entrance')) ?? items[0];
          this.entranceNodeId = entrance?.id ?? '';
        } else {
          this.routeNodes.set(items);
          this.routeStartNodeId = items[0]?.id ?? '';
          this.routeEndNodeId = items.length > 1 ? items[items.length - 1].id : '';
        }
      },
      error: () => { if (target === 'recommend') this.parkingNodes.set([]); else this.routeNodes.set([]); },
    });
  }

  recommendParking(): void {
    this.clearFeedback();
    this.parkingRecommendation.set(null);

    if (!this.parkingVenueId || !this.entranceNodeId.trim()) {
      this.error.set('Choose a venue and entrance point.');
      return;
    }

    this.api.parkingRecommendation({
      venueId: this.parkingVenueId,
      eventId: this.parkingEventId || null,
      entranceNodeId: this.entranceNodeId.trim(),
      requiresAccessibleParking: this.accessibleParking,
      savedVehicleId: this.savedVehicleId || null,
    }).subscribe({
      next: (value) => this.parkingRecommendation.set(value),
      error: (e) => this.error.set(httpErrorMessage(e, 'No parking recommendation could be returned.')),
    });
  }

  findParkingRoute(): void {
    this.clearFeedback();
    this.parkingRoute.set(null);

    if (!this.routeVenueId || !this.routeStartNodeId.trim() || !this.routeEndNodeId.trim()) {
      this.error.set('Choose a venue, start point and destination.');
      return;
    }

    this.api.parkingRoute(
      this.routeVenueId,
      this.routeStartNodeId.trim(),
      this.routeEndNodeId.trim(),
      this.routeAccessibleOnly
    ).subscribe({
      next: (value) => this.parkingRoute.set(value),
      error: (e) => this.error.set(httpErrorMessage(e, 'A parking route could not be found.')),
    });
  }

  selectedBooking(): CoverageBooking | undefined {
    return this.bookings().find((x) => this.bookingId(x) === this.selectedBookingId);
  }

  eventId(event: CoverageEvent): string {
    return event.eventId ?? event.id ?? '';
  }

  eventName(event: CoverageEvent): string {
    return event.title ?? event.name ?? 'Untitled event';
  }

  bookingId(booking: CoverageBooking): string {
    return booking.bookingId ?? booking.id ?? '';
  }

  bookingLabel(booking: CoverageBooking): string {
    return booking.bookingReference ?? booking.bookingNumber ?? this.bookingId(booking);
  }

  venueId(venue: VenueLiteDto): string {
    return venue.venueId ?? venue.id ?? '';
  }

  isConfirmedBooking(booking: CoverageBooking): boolean {
    if (typeof booking.status === 'number') return booking.status === 1;
    return String(booking.status ?? '').toLowerCase() === 'confirmed';
  }

  bookingStatus(booking: CoverageBooking): string {
    if (typeof booking.status === 'number') {
      return ['Pending', 'Confirmed', 'Cancelled', 'Completed'][booking.status] ?? `Status ${booking.status}`;
    }
    return String(booking.status ?? 'Unknown');
  }

  deliveryStatus(value: string | number): string {
    if (typeof value === 'number') return ['Pending', 'Sent', 'Failed'][value] ?? `Status ${value}`;
    return String(value);
  }

  waitlistStatus(value: string | number | undefined): string {
    if (typeof value === 'number') return ['Waiting', 'Eligible', 'Left', 'Converted'][value] ?? `Status ${value}`;
    return String(value ?? 'Waiting');
  }

  refundStatus(value: string | number): string {
    if (typeof value === 'number') return ['Pending', 'Completed', 'Failed'][value] ?? `Status ${value}`;
    return String(value);
  }

  formatDate(value?: string | null): string {
    if (!value) return '—';
    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? value : date.toLocaleString();
  }

  money(value?: number, currency = 'LKR'): string {
    if (value === undefined || value === null) return '—';
    return `${currency} ${Number(value).toLocaleString(undefined, { maximumFractionDigits: 2 })}`;
  }

  private clearFeedback(): void {
    this.error.set('');
    this.message.set('');
  }
}
