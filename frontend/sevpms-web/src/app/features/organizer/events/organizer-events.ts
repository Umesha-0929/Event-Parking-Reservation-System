import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { EventSummary } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';

@Component({
  selector: 'app-organizer-events',
  imports: [RouterLink],
  templateUrl: './organizer-events.html',
  styleUrl: './organizer-events.scss'
})
export class OrganizerEventsComponent implements OnInit {
  private readonly domain = inject(DomainApiService);
  readonly events = signal<EventSummary[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly busyId = signal('');

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    this.domain.myEvents().subscribe({
      next: (items) => { this.events.set(items); this.loading.set(false); },
      error: (error) => { this.error.set(httpErrorMessage(error, 'Your events could not be loaded.')); this.loading.set(false); }
    });
  }

  id(event: EventSummary): string { return event.eventId ?? event.id ?? ''; }
  status(event: EventSummary): string {
    const value = Number(event.status);
    if (value === 1) return 'Published';
    if (value === 2) return 'Cancelled';
    if (value === 3) return 'Completed';
    return typeof event.status === 'string' && Number.isNaN(value) ? event.status : 'Draft';
  }
  statusClass(event: EventSummary): string {
    const value = this.status(event).toLowerCase();
    return value === 'published' ? 'badge--success' : value === 'cancelled' ? 'badge--danger' : value === 'completed' ? 'badge--muted' : 'badge--warning';
  }
  cancel(event: EventSummary): void {
    const id = this.id(event);
    if (!id || this.busyId() || !confirm('Cancel this event? Customers will no longer be able to book it.')) return;
    this.busyId.set(id); this.error.set('');
    this.domain.cancelEvent(id).subscribe({
      next: () => { this.busyId.set(''); this.load(); },
      error: (error) => { this.busyId.set(''); this.error.set(httpErrorMessage(error, 'The event could not be cancelled.')); },
    });
  }
  when(value?: string): string {
    if (!value) return 'Date not set';
    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? value : date.toLocaleString();
  }
}
