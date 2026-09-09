import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { catchError, forkJoin, map, of } from 'rxjs';
import { EventSummary, VenueSummary } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { SeoService } from '../../../core/services/seo.service';
import { httpErrorMessage } from '../../../core/utils/http-error';
import { RevealOnScrollDirective } from '../../../shared/directives/reveal-on-scroll.directive';
import { MagicCardDirective } from '../../../shared/directives/magic-card.directive';
import { NavIconComponent } from '../../../shared/components/nav-icon/nav-icon';
import { LocalMediaService } from '../../../core/services/local-media.service';

@Component({
  selector: 'app-catalog',
  imports: [RouterLink, RevealOnScrollDirective, MagicCardDirective, NavIconComponent],
  templateUrl: './catalog.html',
  styleUrl: './catalog.scss',
})
export class CatalogComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly seo = inject(SeoService);
  private readonly domain = inject(DomainApiService);
  private readonly media = inject(LocalMediaService);
  readonly kind = (this.route.snapshot.data['kind'] ?? 'events') as 'events' | 'venues';
  readonly detailBase = (this.route.snapshot.data['detailBase'] as string | undefined) ?? (this.kind === 'events' ? '/events' : '/venues');
  readonly backLink = (this.route.snapshot.data['backLink'] as string | undefined) ?? '/';
  readonly inWorkspace = !!this.route.snapshot.data['workspace'];
  readonly title = this.kind === 'events' ? 'Discover events made for the moment.' : 'Find a venue that fits the experience.';
  readonly copy = this.kind === 'events' ? 'Search published events, then continue to tickets, seats and arrival services.' : 'Explore real venue information and media supplied by venue owners.';
  readonly loading = signal(true);
  readonly error = signal('');
  readonly events = signal<EventSummary[]>([]);
  readonly venues = signal<VenueSummary[]>([]);
  readonly venueImages = signal<Record<string, string>>({});
  readonly eventImages = signal<Record<string, string>>({});

  ngOnInit(): void {
    this.seo.setPage(this.kind === 'events' ? 'Events | Nvent' : 'Venues | Nvent', this.copy);
    if (this.kind === 'events') {
      this.domain.events().subscribe({
        next: (items) => { const events=Array.isArray(items)?items:[]; this.events.set(events); this.loading.set(false); this.loadEventImages(events); },
        error: (error) => { this.error.set(httpErrorMessage(error, 'Events could not be loaded.')); this.loading.set(false); },
      });
    } else {
      this.domain.venues().subscribe({
        next: (items) => {
          const venues = Array.isArray(items) ? items : [];
          this.venues.set(venues);
          this.loading.set(false);
          this.loadVenueImages(venues);
        },
        error: (error) => { this.error.set(httpErrorMessage(error, 'Venues could not be loaded.')); this.loading.set(false); },
      });
    }
  }

  retry(): void { this.loading.set(true); this.error.set(''); this.ngOnInit(); }
  eventTitle(item: EventSummary): string { return item.title ?? item.name ?? 'Event'; }
  venueId(item: VenueSummary): string { return item.venueId ?? item.id ?? ''; }
  venueImage(item: VenueSummary): string { return this.venueImages()[this.venueId(item)] ?? ''; }
  eventId(item: EventSummary): string { return item.eventId ?? item.id ?? ''; }
  eventImage(item: EventSummary): string { return this.eventImages()[this.eventId(item)] ?? item.imageUrl ?? ''; }

  private loadEventImages(events: EventSummary[]): void {
    const requests=events.map((event)=>{const id=this.eventId(event);if(!id)return of({id:'',image:''});return this.media.eventCover(id).pipe(map(x=>({id,image:x.url??''})),catchError(()=>of({id,image:''})));});
    if(!requests.length)return;forkJoin(requests).subscribe(results=>{const next:Record<string,string>={};for(const r of results)if(r.id&&r.image)next[r.id]=r.image;this.eventImages.set(next);});
  }

  private loadVenueImages(venues: VenueSummary[]): void {
    const requests = venues.map((venue) => {
      const id = this.venueId(venue);
      if (!id) return of({ id: '', image: '' });
      return this.domain.venueMarketplace(id).pipe(
        map((marketplace) => {
          const media = [...(marketplace.media ?? [])]
            .filter((item) => item.type?.toLowerCase() === 'photo' || /\.(png|jpe?g|webp|gif)(\?|$)/i.test(item.url ?? ''))
            .sort((a, b) => Number(a.sortOrder ?? 0) - Number(b.sortOrder ?? 0));
          return { id, image: media[0]?.url ?? '' };
        }),
        catchError(() => of({ id, image: '' })),
      );
    });
    if (!requests.length) return;
    forkJoin(requests).subscribe((results) => {
      const mapValue: Record<string, string> = {};
      for (const result of results) if (result.id && result.image) mapValue[result.id] = result.image;
      this.venueImages.set(mapValue);
    });
  }
}
