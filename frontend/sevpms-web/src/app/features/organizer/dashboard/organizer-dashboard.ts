import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { catchError, forkJoin, of } from 'rxjs';
import { EventSummary, VenueSummary } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';
import { NavIconComponent } from '../../../shared/components/nav-icon/nav-icon';

@Component({
  selector: 'app-organizer-dashboard',
  imports: [RouterLink, NavIconComponent],
  templateUrl: './organizer-dashboard.html',
  styleUrl: './organizer-dashboard.scss',
})
export class OrganizerDashboardComponent implements OnInit {
  private readonly domain = inject(DomainApiService);
  readonly events = signal<EventSummary[]>([]);
  readonly venues = signal<VenueSummary[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    forkJoin({
      events: this.domain.myEvents(),
      venues: this.domain.venues().pipe(catchError(() => of([] as VenueSummary[]))),
    }).subscribe({
      next: ({ events, venues }) => {
        this.events.set(events);
        this.venues.set(venues);
        this.loading.set(false);
      },
      error: (error) => {
        this.error.set(httpErrorMessage(error, 'Organizer events could not be loaded.'));
        this.loading.set(false);
      },
    });
  }

  published(): number { return this.events().filter((event) => this.status(event) === 'Published').length; }
  drafts(): number { return this.events().filter((event) => this.status(event) === 'Draft').length; }
  recentEvents(): EventSummary[] { return this.events().slice(0, 4); }
  id(event: EventSummary): string { return event.eventId ?? event.id ?? ''; }

  status(event: EventSummary): string {
    const raw = event.status;
    const numeric = Number(raw);
    if (!Number.isNaN(numeric)) return ['Draft', 'Published', 'Cancelled', 'Completed'][numeric] ?? String(raw ?? 'Draft');
    const text = String(raw ?? 'Draft').toLowerCase();
    if (text.includes('publish')) return 'Published';
    if (text.includes('cancel')) return 'Cancelled';
    if (text.includes('complete')) return 'Completed';
    return 'Draft';
  }

  statusClass(event: EventSummary): string {
    return `status-pill status-pill--${this.status(event).toLowerCase()}`;
  }

  venueName(event: EventSummary): string {
    if (event.venueName?.trim()) return event.venueName.trim();
    const venueId = event.venueId;
    if (!venueId) return 'Venue not assigned';
    return this.venues().find((venue) => (venue.venueId ?? venue.id) === venueId)?.name ?? 'Venue details unavailable';
  }

  when(value?: string): string {
    if (!value) return 'Date not set';
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return 'Date not set';
    return new Intl.DateTimeFormat(undefined, {
      month: 'short', day: 'numeric', year: 'numeric', hour: 'numeric', minute: '2-digit',
    }).format(date);
  }

  actionLabel(event: EventSummary): string {
    const status = this.status(event);
    if (status === 'Published') return 'Review event';
    if (status === 'Cancelled' || status === 'Completed') return 'View setup';
    return 'Continue setup';
  }

  actionRoute(event: EventSummary): string[] {
    const id = this.id(event);
    const status = this.status(event);
    return ['/organizer/events', id, status === 'Draft' ? 'stage' : 'preview'];
  }
}
