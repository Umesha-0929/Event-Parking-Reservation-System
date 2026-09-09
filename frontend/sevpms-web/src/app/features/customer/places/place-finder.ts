import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { catchError, forkJoin, map, of } from 'rxjs';
import { EventSummary, NearbyPlaceDto, VenueSummary } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';
import { LocationMapComponent, PlatformMapPin } from '../../../shared/components/location-map/location-map';

@Component({
  selector: 'app-place-finder',
  imports: [CommonModule, FormsModule, LocationMapComponent],
  templateUrl: './place-finder.html',
  styleUrl: './place-finder.scss',
})
export class PlaceFinderComponent implements OnInit {
  private readonly domain = inject(DomainApiService);
  readonly venues = signal<VenueSummary[]>([]);
  readonly events = signal<EventSummary[]>([]);
  readonly places = signal<NearbyPlaceDto[]>([]);
  readonly foodPins = signal<PlatformMapPin[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');

  venueId = '';
  audienceMode = 'Friends';
  category = '';
  maxDistanceKm = 5;
  includeClosed = false;

  readonly mapPins = computed<PlatformMapPin[]>(() => {
    const result: PlatformMapPin[] = [];
    const venueMap = new Map<string, VenueSummary>();

    for (const venue of this.venues()) {
      const id = venue.venueId ?? venue.id ?? '';
      if (id) venueMap.set(id, venue);
      if (id && venue.latitude != null && venue.longitude != null) {
        result.push({
          id: `v-${id}`,
          name: venue.name,
          latitude: Number(venue.latitude),
          longitude: Number(venue.longitude),
          kind: 'venue',
          subtitle: venue.city,
        });
      }
    }

    for (const event of this.events()) {
      const eventId = event.eventId ?? event.id ?? '';
      const venue = venueMap.get(event.venueId ?? '');
      if (eventId && venue?.latitude != null && venue.longitude != null) {
        result.push({
          id: `e-${eventId}`,
          name: event.title ?? event.name ?? 'Event',
          latitude: Number(venue.latitude),
          longitude: Number(venue.longitude),
          kind: 'event',
          subtitle: venue.name,
          visualOffsetX: 12,
          visualOffsetY: -10,
        });
      }
    }

    result.push(...this.foodPins());
    for (const place of this.places()) {
      result.push({
        id: `p-${place.id}`,
        name: place.name,
        latitude: Number(place.latitude),
        longitude: Number(place.longitude),
        kind: 'place',
        subtitle: `${place.category} · ${place.distanceKm} km`,
      });
    }
    return result;
  });

  ngOnInit(): void {
    forkJoin({
      venues: this.domain.venues(),
      events: this.domain.events().pipe(catchError(() => of([] as EventSummary[]))),
    }).subscribe({
      next: ({ venues, events }) => {
        this.venues.set(venues);
        this.events.set(events);
        this.venueId = venues[0]?.venueId ?? venues[0]?.id ?? '';
        this.loading.set(false);
        if (this.venueId) this.search();
        this.loadFoodPins(events, venues);
      },
      error: (error) => {
        this.loading.set(false);
        this.error.set(httpErrorMessage(error, 'Venues could not be loaded.'));
      },
    });
  }

  search(): void {
    if (!this.venueId) return;
    this.loading.set(true);
    this.error.set('');
    this.domain.placeRecommendations(
      this.venueId,
      this.audienceMode,
      this.category,
      Number(this.maxDistanceKm) || undefined,
      this.includeClosed,
    ).subscribe({
      next: (places) => { this.places.set(places); this.loading.set(false); },
      error: (error) => {
        this.loading.set(false);
        this.error.set(httpErrorMessage(error, 'Nearby places could not be loaded.'));
      },
    });
  }

  pinChosen(pin: PlatformMapPin): void {
    if (pin.kind !== 'venue') return;
    const id = pin.id.replace(/^v-/, '');
    if (this.venues().some((venue) => (venue.venueId ?? venue.id) === id)) {
      this.venueId = id;
      this.search();
    }
  }

  private loadFoodPins(events: EventSummary[], venues: VenueSummary[]): void {
    const venueMap = new Map<string, VenueSummary>();
    for (const venue of venues) {
      const id = venue.venueId ?? venue.id ?? '';
      if (id) venueMap.set(id, venue);
    }

    const requests = events
      .map((event) => ({ event, id: event.eventId ?? event.id ?? '' }))
      .filter((item) => !!item.id)
      .map(({ event, id }) => this.domain.foodStalls(id).pipe(
        map((stalls) => {
          const pins: PlatformMapPin[] = [];
          const venue = venueMap.get(event.venueId ?? '');
          if (venue?.latitude == null || venue.longitude == null) return pins;
          stalls.forEach((stall, index) => {
            pins.push({
              id: `f-${stall.id}`,
              name: stall.stallName,
              latitude: Number(venue.latitude),
              longitude: Number(venue.longitude),
              kind: 'food',
              subtitle: `At ${venue.name} · ${event.title ?? event.name ?? 'Event'}`,
              visualOffsetX: 18 + (index % 3) * 14,
              visualOffsetY: 8 + Math.floor(index / 3) * 12,
            });
          });
          return pins;
        }),
        catchError(() => of([] as PlatformMapPin[])),
      ));

    if (!requests.length) { this.foodPins.set([]); return; }
    forkJoin(requests).subscribe((rows) => this.foodPins.set(rows.flat()));
  }
}
