import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { EventSummary, VenueMarketplaceDto, VenueSummary } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { SeoService } from '../../../core/services/seo.service';
import { httpErrorMessage } from '../../../core/utils/http-error';
import { RevealOnScrollDirective } from '../../../shared/directives/reveal-on-scroll.directive';

@Component({ selector:'app-detail', imports:[CommonModule, RouterLink, RevealOnScrollDirective], templateUrl:'./detail.html', styleUrl:'./detail.scss' })
export class DetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly seo = inject(SeoService);
  private readonly domain = inject(DomainApiService);
  readonly kind = (this.route.snapshot.data['kind'] ?? 'event') as 'event'|'venue';
  readonly backLink = (this.route.snapshot.data['backLink'] as string | undefined) ?? (this.kind === 'event' ? '/events' : '/venues');
  readonly eventsLink = (this.route.snapshot.data['eventsLink'] as string | undefined) ?? '/events';
  readonly id = this.route.snapshot.paramMap.get('slug') ?? '';
  readonly loading = signal(true);
  readonly error = signal('');
  readonly event = signal<EventSummary | null>(null);
  readonly venue = signal<VenueSummary | null>(null);
  readonly marketplace = signal<VenueMarketplaceDto | null>(null);

  ngOnInit(): void {
    if (!this.id) { this.error.set('No item identifier was supplied.'); this.loading.set(false); return; }
    if (this.kind === 'event') {
      this.domain.event(this.id).subscribe({
        next: (event) => { this.event.set(event); this.loading.set(false); this.seo.setPage(`${event.title ?? event.name ?? 'Event'} | Nvent`, event.description ?? 'Nvent event details', event.imageUrl); },
        error: (error) => { this.error.set(httpErrorMessage(error, 'Event details could not be loaded.')); this.loading.set(false); },
      });
    } else {
      this.domain.venue(this.id).subscribe({
        next: (venue) => {
          this.venue.set(venue);
          this.loading.set(false);
          this.seo.setPage(`${venue.name} | Nvent`, venue.description ?? 'Nvent venue details', venue.imageUrl);
          this.domain.venueMarketplace(this.id).subscribe({ next: (marketplace) => this.marketplace.set(marketplace), error: () => this.marketplace.set(null) });
        },
        error: (error) => { this.error.set(httpErrorMessage(error, 'Venue details could not be loaded.')); this.loading.set(false); },
      });
    }
  }

  get title(): string { return this.kind === 'event' ? (this.event()?.title ?? this.event()?.name ?? 'Event') : (this.venue()?.name ?? 'Venue'); }
  get description(): string { return this.kind === 'event' ? (this.event()?.description ?? 'View the event information and continue to ticket selection when available.') : (this.venue()?.description ?? 'View the venue information and upcoming event context.'); }
  get image(): string { return this.kind === 'event' ? (this.event()?.imageUrl || '/assets/images/concert-poster.jpg') : (this.venue()?.imageUrl || '/assets/images/venue-night-poster.jpg'); }
}
