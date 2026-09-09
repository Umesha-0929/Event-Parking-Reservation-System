import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { EventSummary } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';
import { NavIconComponent } from '../../../shared/components/nav-icon/nav-icon';

const ARCHIVE_KEY = 'nvent.organizer.archived-event-ids';

@Component({
  selector: 'app-organizer-events',
  imports: [RouterLink, FormsModule, NavIconComponent],
  templateUrl: './organizer-events.html',
  styleUrl: './organizer-events.scss',
})
export class OrganizerEventsComponent implements OnInit {
  private readonly domain = inject(DomainApiService);
  readonly events = signal<EventSummary[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly success = signal('');
  readonly busyId = signal('');
  readonly archivedIds = signal<Set<string>>(this.readArchived());
  query = '';
  filter = 'active';

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    this.domain.myEvents().subscribe({
      next: (items) => { this.events.set(items); this.loading.set(false); },
      error: (error) => { this.error.set(httpErrorMessage(error, 'Your events could not be loaded.')); this.loading.set(false); },
    });
  }

  id(event: EventSummary): string { return event.eventId ?? event.id ?? ''; }
  status(event: EventSummary): string {
    const value = Number(event.status);
    if (value === 1) return 'Published';
    if (value === 2) return 'Cancelled';
    if (value === 3) return 'Completed';
    const text = String(event.status ?? '').toLowerCase();
    if (text.includes('publish')) return 'Published';
    if (text.includes('cancel')) return 'Cancelled';
    if (text.includes('complete')) return 'Completed';
    return 'Draft';
  }

  filteredEvents(): EventSummary[] {
    const q = this.query.trim().toLowerCase();
    return this.events().filter((event) => {
      const id = this.id(event);
      const archived = this.archivedIds().has(id);
      if (this.filter === 'archived' && !archived) return false;
      if (this.filter !== 'archived' && archived) return false;
      if (this.filter !== 'active' && this.filter !== 'archived' && this.status(event).toLowerCase() !== this.filter) return false;
      if (!q) return true;
      return `${event.title ?? event.name ?? ''} ${event.categoryName ?? event.category ?? ''} ${this.status(event)}`.toLowerCase().includes(q);
    });
  }

  cancel(event: EventSummary): void {
    const id = this.id(event);
    if (!id || this.busyId() || !confirm('Cancel this event? Customers will no longer be able to book it.')) return;
    this.busyId.set(id); this.error.set(''); this.success.set('');
    this.domain.cancelEvent(id).subscribe({
      next: () => { this.busyId.set(''); this.success.set('Event cancelled. You can archive it after review.'); this.load(); },
      error: (error) => { this.busyId.set(''); this.error.set(httpErrorMessage(error, 'The event could not be cancelled.')); },
    });
  }

  archive(event: EventSummary): void {
    const id = this.id(event);
    if (!id || !['Cancelled', 'Completed'].includes(this.status(event))) return;
    if (!confirm('Archive this event from your normal event list? The event record and history will not be deleted.')) return;
    const next = new Set(this.archivedIds());
    next.add(id);
    this.archivedIds.set(next);
    this.writeArchived(next);
    this.success.set('Event archived from the normal list.');
  }

  clearCancelled(): void {
    const cancelled=this.events().filter((event)=>this.status(event)==='Cancelled'&&!this.archivedIds().has(this.id(event)));
    if(!cancelled.length){this.success.set('No cancelled events to clear.');return;}
    if(!confirm(`Clear ${cancelled.length} cancelled event${cancelled.length===1?'':'s'} from the normal list? They will move to Archived and history will be preserved.`))return;
    const next=new Set(this.archivedIds()); for(const event of cancelled){const id=this.id(event);if(id)next.add(id);}
    this.archivedIds.set(next);this.writeArchived(next);this.success.set(`${cancelled.length} cancelled event${cancelled.length===1?'':'s'} moved to Archived.`);
  }

  restore(event: EventSummary): void {
    const id = this.id(event);
    const next = new Set(this.archivedIds());
    next.delete(id);
    this.archivedIds.set(next);
    this.writeArchived(next);
    this.success.set('Event restored to your list.');
  }

  when(value?: string): string {
    if (!value) return 'Date not set';
    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? 'Date not set' : new Intl.DateTimeFormat(undefined, { month:'short', day:'numeric', year:'numeric', hour:'numeric', minute:'2-digit' }).format(date);
  }

  private readArchived(): Set<string> {
    if (typeof localStorage === 'undefined') return new Set();
    try { return new Set(JSON.parse(localStorage.getItem(ARCHIVE_KEY) ?? '[]') as string[]); } catch { return new Set(); }
  }
  private writeArchived(ids: Set<string>): void {
    if (typeof localStorage !== 'undefined') localStorage.setItem(ARCHIVE_KEY, JSON.stringify([...ids]));
  }
}
