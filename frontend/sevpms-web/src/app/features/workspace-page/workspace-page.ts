import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DomainApiService } from '../../core/services/domain-api.service';
import { BookingSummary, EventSummary, NotificationItem, PaymentResponseDto, UserProfileDto, VenueSummary } from '../../core/models/api.models';
import { SessionService } from '../../core/services/session.service';
import { NotificationCenterService } from '../../core/services/notification-center.service';
import { httpErrorMessage } from '../../core/utils/http-error';

interface PageConfig {
  eyebrow: string;
  title: string;
  copy: string;
  action?: string;
  actionPath?: string;
  columns: string[];
  emptyTitle: string;
  emptyCopy: string;
}

@Component({
  selector: 'app-workspace-page',
  imports: [FormsModule, RouterLink],
  templateUrl: './workspace-page.html',
  styleUrl: './workspace-page.scss',
})
export class WorkspacePageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly domain = inject(DomainApiService);
  private readonly session = inject(SessionService);
  private readonly notificationCenter = inject(NotificationCenterService);

  readonly pageKey = (this.route.snapshot.data['page'] ?? 'bookings') as string;
  readonly loading = signal(false);
  readonly error = signal('');
  readonly rows = signal<string[][]>([]);
  readonly query = signal('');
  readonly statusFilter = signal('All');
  readonly filteredRows = computed(() => {
    const q = this.query().trim().toLowerCase();
    const status = this.statusFilter().toLowerCase();
    return this.rows().filter((row) => {
      const joined = row.join(' ').toLowerCase();
      return (!q || joined.includes(q)) && (status === 'all' || joined.includes(status));
    });
  });

  private readonly fallback: PageConfig = {
    eyebrow: 'Nvent workspace',
    title: 'Workspace',
    copy: 'Your information will appear here when available.',
    columns: ['Item', 'Status'],
    emptyTitle: 'Nothing to show yet',
    emptyCopy: 'Nothing is available here yet.',
  };

  readonly config: PageConfig = ({
    bookings: { eyebrow:'Bookings', title:'Bookings stay clear from reservation to arrival.', copy:'Track confirmed and pending bookings from reservation through arrival.', columns:['Booking','Event','Status','Total'], emptyTitle:'No bookings yet', emptyCopy:'Complete an event booking and it will appear here.' },
    food: { eyebrow:'Food', title:'Order food around the event experience.', copy:'Choose food options connected to your event and keep order status in one place.', columns:['Order','Status','Total'], emptyTitle:'No food orders yet', emptyCopy:'Your food orders will appear here.' },
    places: { eyebrow:'Place finder', title:'Useful places for the people you are travelling with.', copy:'Choose a venue and travel group to discover useful nearby places.', columns:['Place','Category','Distance'], emptyTitle:'No place recommendations loaded', emptyCopy:'Open an event or venue first so Nvent has a venue context for recommendations.' },
    notifications: { eyebrow:'Notifications', title:'Important updates without the noise.', copy:'See booking, ticket, parking and event updates in one place.', columns:['Notification','Type','State','Time'], emptyTitle:'You are all caught up', emptyCopy:'New booking, ticket, parking and event updates will appear here.' },
    profile: { eyebrow:'Profile', title:'Your Nvent account, clearly organized.', copy:'Review the account details used across Nvent.', columns:['Field','Value'], emptyTitle:'Profile unavailable', emptyCopy:'Sign in again if the account endpoint cannot be reached.' },
    settings: { eyebrow:'Settings', title:'Preferences that respect the experience.', copy:'Manage experience preferences and account security.', columns:['Preference','State'], emptyTitle:'No configurable server settings', emptyCopy:'Your account preferences and security options are available from Account Settings.' },
    organizerEvents: { eyebrow:'Organizer events', title:'Plan and manage every event from one place.', copy:'Create, publish and manage the events you organize.', action:'Create event', actionPath:'/organizer/create-event', columns:['Event','Venue','Status','Start'], emptyTitle:'No events yet', emptyCopy:'Create your first event draft to begin.' },
    categories: { eyebrow:'Seat categories', title:'Define categories and pricing in the seating builder.', copy:'Seat categories are configured inside each event seating layout.', action:'Open seating builder', actionPath:'/organizer/seating', columns:['Capability','Location'], emptyTitle:'Choose an event first', emptyCopy:'Open an event’s seating builder to configure categories, pricing and seat generation.' },
    organizerTickets: { eyebrow:'Tickets', title:'Manage issued tickets and entry readiness for your events.', copy:'Ticket issuance, QR validation and check-in remain linked to each booking and event.', columns:['Capability','Status'], emptyTitle:'Choose an event or booking', emptyCopy:'Ticket records become available through booking/event context.' },
    organizerReports: { eyebrow:'Organizer analytics', title:'Readable insights for your organizer activity.', copy:'Metrics appear as your events receive real activity.', columns:['Metric','Value'], emptyTitle:'No report data returned', emptyCopy:'Publish events and receive bookings before report values become available.' },
    finance: { eyebrow:'Finance', title:'Financial information without decorative distraction.', copy:'Review payment and receipt information available for your events.', columns:['Capability','Status'], emptyTitle:'No organizer finance list is exposed', emptyCopy:'Use Analytics & Finance to review organizer revenue and activity.' },
    adminEvents: { eyebrow:'Admin · events', title:'Operational event management.', copy:'Review and manage platform event records.', columns:['Event','Venue','Status','Start'], emptyTitle:'No events returned', emptyCopy:'No event records are available.' },
    adminVenues: { eyebrow:'Admin · venues', title:'Venue operations in a readable view.', copy:'Review registered venues and their active status.', columns:['Venue','City','Capacity','Status'], emptyTitle:'No venues returned', emptyCopy:'No venue records are available.' },
    payments: { eyebrow:'Admin · payments', title:'Payment and receipt management.', copy:'Review payment amounts, providers and current status.', columns:['Payment','Booking','Amount','Status'], emptyTitle:'No payments returned', emptyCopy:'No payment records are available yet.' },
    users: { eyebrow:'Admin · users', title:'User and role management.', copy:'Review users, roles and account status.', columns:['User','Email','Role','Status'], emptyTitle:'No users returned', emptyCopy:'No user records are available yet.' },
    seatingTickets: { eyebrow:'Admin · seating & tickets', title:'Operational visibility across seats and tickets.', copy:'Choose an event or booking to inspect seating and ticket operations.', columns:['Capability','Status'], emptyTitle:'Choose an event', emptyCopy:'Use an event context to inspect seating and a booking context to inspect tickets.' },
    adminReports: { eyebrow:'Admin · reports', title:'Platform reporting with current operational numbers.', copy:'Platform metrics are shown as they become available.', columns:['Metric','Value'], emptyTitle:'No report data returned', emptyCopy:'No platform report values are available yet.' },
    operations: { eyebrow:'Admin · operations', title:'Operational health where teams need it.', copy:'Review the current operational status of Nvent services.', columns:['Check','Value'], emptyTitle:'Health response unavailable', emptyCopy:'Operational status could not be loaded.' },
    venueOwnerDashboard: { eyebrow:'Venue owner', title:'Your venue portfolio at a glance.', copy:'Manage the venues registered under your account.', columns:['Venue','City','Capacity','Status'], emptyTitle:'No venues yet', emptyCopy:'Create or receive ownership of a venue to populate this workspace.' },
    venueOwnerVenues: { eyebrow:'My venues', title:'Manage the venues you own.', copy:'Keep each owned venue accurate and ready for organizers.', columns:['Venue','City','Capacity','Status'], emptyTitle:'No venues yet', emptyCopy:'No owned venues were returned.' },
    venueMarketplace: { eyebrow:'Venue marketplace', title:'Present venues with the details organizers need.', copy:'Select one of your venues to manage facilities, media, rates and availability.', columns:['Capability','Status'], emptyTitle:'Select a venue', emptyCopy:'Open an owned venue before editing its marketplace profile.' },
    venueRentals: { eyebrow:'Venue rentals', title:'Incoming venue rental requests.', copy:'Review and respond to incoming organizer rental requests.', columns:['Rental','Venue','Status','Start'], emptyTitle:'No incoming rentals', emptyCopy:'No rental requests are waiting for you.' },
    venueAvailability: { eyebrow:'Venue availability', title:'Availability belongs to each marketplace venue.', copy:'Select one of your venues to manage its availability windows.', columns:['Capability','Status'], emptyTitle:'Select a venue', emptyCopy:'Choose an owned venue before editing marketplace availability.' },
    venueParking: { eyebrow:'Venue parking', title:'Parking configuration for each venue.', copy:'Create zones and aligned parking slots for each owned venue.', columns:['Capability','Status'], emptyTitle:'Select a venue', emptyCopy:'Choose an owned venue to manage parking zones and slots.' },
    venueReports: { eyebrow:'Venue reports', title:'Venue performance using real report data.', copy:'Review venue activity and rental performance.', columns:['Metric','Value'], emptyTitle:'No report data returned', emptyCopy:'No venue report values are available yet.' },
  } as Record<string, PageConfig>)[this.pageKey] ?? this.fallback;

  ngOnInit(): void { this.load(); }

  retry(): void { this.load(); }

  markRead(rowIndex: number): void {
    if (this.pageKey !== 'notifications') return;
    const id = this.notificationIds[rowIndex];
    if (!id) return;
    this.notificationCenter.markRead(id).subscribe({ next: () => this.load(), error: (error) => this.error.set(httpErrorMessage(error)) });
  }

  private notificationIds: string[] = [];

  private load(): void {
    this.loading.set(true);
    this.error.set('');
    this.rows.set([]);
    this.notificationIds = [];

    switch (this.pageKey) {
      case 'bookings':
        this.domain.bookings().subscribe(this.handlers<BookingSummary[]>((items) => items.map((b) => [b.bookingNumber ?? b.bookingId ?? '—', b.eventName ?? b.eventId ?? '—', String(b.status ?? '—'), this.money(b.totalAmount)])));
        return;
      case 'notifications':
        this.notificationCenter.refresh().subscribe(this.handlers<NotificationItem[]>((items) => { this.notificationIds = items.map((n) => n.notificationId ?? n.id ?? ''); return items.map((n) => [n.title ?? n.message ?? 'Notification', n.type ?? 'System', n.isRead ? 'Read' : 'Unread', this.date(n.createdAtUtc)]); }));
        return;
      case 'profile':
        this.domain.me().subscribe(this.handlers<UserProfileDto>((u) => [['Name', [u.firstName, u.lastName].filter(Boolean).join(' ') || '—'], ['Email', u.email ?? '—'], ['Phone', u.phoneNumber ?? '—'], ['Role', this.session.normalizeRole(u.role) ?? this.session.role() ?? '—']]));
        return;
      case 'settings':
        this.finish([['Reduced motion','Uses operating-system preference'],['Account security','Available from your account security settings'],['Authentication','Secure session']]); return;
      case 'organizerEvents':
        this.domain.myEvents().subscribe(this.handlers<EventSummary[]>((items) => items.map((e) => [e.title ?? e.name ?? 'Event', e.venueName ?? e.venueId ?? '—', String(e.status ?? '—'), this.date(e.startAtUtc ?? e.startDateTime)]))); return;
      case 'organizerReports':
        this.domain.organizerReport().subscribe(this.handlers((value) => this.objectRows(value))); return;
      case 'adminEvents':
        this.domain.events().subscribe(this.handlers<EventSummary[]>((items) => items.map((e) => [e.title ?? e.name ?? 'Event', e.venueName ?? e.venueId ?? '—', String(e.status ?? '—'), this.date(e.startAtUtc ?? e.startDateTime)]))); return;
      case 'adminVenues':
        this.domain.venues().subscribe(this.handlers<VenueSummary[]>((items) => items.map((v) => [v.name, v.city ?? '—', String(v.capacity ?? '—'), v.isActive === false ? 'Inactive' : (v.status ?? 'Active')]))); return;
      case 'payments':
        this.domain.payments().subscribe(this.handlers<PaymentResponseDto[]>((items) => items.map((p) => [p.paymentId, p.bookingId, `${p.currency ?? 'LKR'} ${Number(p.amount ?? 0).toLocaleString()}`, String(p.status ?? '—')]))); return;
      case 'users':
        this.domain.adminUsers().subscribe(this.handlers<unknown[]>((items) => items.map((item) => this.adminUserRow(item)))); return;
      case 'adminReports':
        this.domain.platformReport().subscribe(this.handlers((value) => this.objectRows(value))); return;
      case 'operations':
        this.domain.health().subscribe(this.handlers((value) => this.objectRows(value))); return;
      case 'venueOwnerDashboard': case 'venueOwnerVenues':
        this.domain.myVenues().subscribe(this.handlers<VenueSummary[]>((items) => items.map((v) => [v.name, v.city ?? '—', String(v.capacity ?? '—'), v.isActive === false ? 'Inactive' : (v.status ?? 'Active')]))); return;
      case 'venueRentals':
        this.domain.venueRentalsIncoming().subscribe(this.handlers<unknown[]>((items) => items.map((item) => this.rentalRow(item)))); return;
      case 'venueReports':
        this.domain.venueOwnerReport().subscribe(this.handlers((value) => this.objectRows(value))); return;
      default:
        this.finish([]);
    }
  }

  private handlers<T>(map: (value: T) => string[][]) {
    return {
      next: (value: T) => this.finish(map(value)),
      error: (error: unknown) => { this.loading.set(false); this.error.set(httpErrorMessage(error, 'This information could not be loaded.')); },
    };
  }

  private finish(rows: string[][]): void { this.rows.set(rows); this.loading.set(false); }
  private money(value?: number): string { return value == null ? '—' : `LKR ${Number(value).toLocaleString()}`; }
  private date(value?: string): string { if (!value) return '—'; const date = new Date(value); return Number.isNaN(date.getTime()) ? value : date.toLocaleString(); }
  private objectRows(value: unknown): string[][] {
    if (Array.isArray(value)) return value.slice(0, 30).map((item, index) => [`Item ${index + 1}`, this.compact(item)]);
    if (value && typeof value === 'object') return Object.entries(value as Record<string, unknown>).map(([key, item]) => [this.label(key), this.compact(item)]);
    return value == null ? [] : [['Result', String(value)]];
  }
  private compact(value: unknown): string { return value && typeof value === 'object' ? JSON.stringify(value) : String(value ?? '—'); }
  private label(value: string): string { return value.replace(/([A-Z])/g, ' $1').replace(/^./, (x) => x.toUpperCase()); }
  private adminUserRow(item: unknown): string[] {
    const u = (item ?? {}) as Record<string, unknown>;
    return [String(u['displayName'] ?? [u['firstName'], u['lastName']].filter(Boolean).join(' ') ?? '—'), String(u['email'] ?? '—'), String(u['role'] ?? '—'), String(u['status'] ?? (u['isActive'] === false ? 'Inactive' : 'Active'))];
  }
  private rentalRow(item: unknown): string[] {
    const r = (item ?? {}) as Record<string, unknown>;
    return [String(r['venueRentalId'] ?? r['id'] ?? '—'), String(r['venueName'] ?? r['venueId'] ?? '—'), String(r['status'] ?? '—'), this.date(typeof r['startAtUtc'] === 'string' ? r['startAtUtc'] : undefined)];
  }
}
