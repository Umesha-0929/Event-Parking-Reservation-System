import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { VenueSummary } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';

@Component({
  selector: 'app-venue-owner-dashboard',
  imports: [RouterLink],
  templateUrl: './venue-owner-dashboard.html',
  styleUrl: './venue-owner-dashboard.scss',
})
export class VenueOwnerDashboardComponent implements OnInit {
  private readonly domain = inject(DomainApiService);
  readonly venues = signal<VenueSummary[]>([]);
  readonly rentalCount = signal(0);
  readonly loading = signal(true);
  readonly error = signal('');

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    forkJoin({ venues: this.domain.myVenues(), rentals: this.domain.venueRentalsIncoming() }).subscribe({
      next: ({ venues, rentals }) => {
        this.venues.set(venues);
        this.rentalCount.set(rentals.length);
        this.loading.set(false);
      },
      error: (error) => {
        this.error.set(httpErrorMessage(error, 'Venue Owner dashboard data could not be loaded.'));
        this.loading.set(false);
      },
    });
  }

  activeCount(): number { return this.venues().filter((venue) => venue.isActive !== false).length; }
  totalCapacity(): number { return this.venues().reduce((sum, venue) => sum + Number(venue.capacity ?? 0), 0); }
  venueId(venue: VenueSummary): string { return venue.venueId ?? venue.id ?? ''; }
}
