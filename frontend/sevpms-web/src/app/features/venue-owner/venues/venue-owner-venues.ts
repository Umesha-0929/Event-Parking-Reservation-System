import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { VenueSummary } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';

@Component({
  selector: 'app-venue-owner-venues',
  imports: [RouterLink],
  templateUrl: './venue-owner-venues.html',
  styleUrl: './venue-owner-venues.scss',
})
export class VenueOwnerVenuesComponent implements OnInit {
  private readonly domain = inject(DomainApiService);
  readonly venues = signal<VenueSummary[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly actionId = signal<string | null>(null);

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    this.domain.myVenues().subscribe({
      next: (items) => { this.venues.set(items); this.loading.set(false); },
      error: (error) => { this.error.set(httpErrorMessage(error, 'Your venues could not be loaded.')); this.loading.set(false); },
    });
  }

  deactivate(venue: VenueSummary): void {
    const id = this.venueId(venue);
    if (!id || this.actionId()) return;
    if (typeof window !== 'undefined' && !window.confirm(`Deactivate “${venue.name}”? It will no longer appear as an active venue.`)) return;
    this.actionId.set(id);
    this.domain.deactivateVenue(id).subscribe({
      next: () => { this.actionId.set(null); this.load(); },
      error: (error) => { this.actionId.set(null); this.error.set(httpErrorMessage(error, 'The venue could not be deactivated.')); },
    });
  }

  venueId(venue: VenueSummary): string { return venue.venueId ?? venue.id ?? ''; }
}
