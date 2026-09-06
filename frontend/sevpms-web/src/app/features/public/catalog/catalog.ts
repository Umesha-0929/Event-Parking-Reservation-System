import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { EventSummary, VenueSummary } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { SeoService } from '../../../core/services/seo.service';
import { httpErrorMessage } from '../../../core/utils/http-error';
import { RevealOnScrollDirective } from '../../../shared/directives/reveal-on-scroll.directive';
import { MagicCardDirective } from '../../../shared/directives/magic-card.directive';

@Component({ selector: 'app-catalog', imports: [RouterLink, RevealOnScrollDirective, MagicCardDirective], templateUrl: './catalog.html', styleUrl: './catalog.scss' })
export class CatalogComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly seo = inject(SeoService);
  private readonly domain = inject(DomainApiService);
  readonly kind = (this.route.snapshot.data['kind'] ?? 'events') as 'events' | 'venues';
  readonly detailBase = (this.route.snapshot.data['detailBase'] as string | undefined) ?? (this.kind === 'events' ? '/events' : '/venues');
  readonly backLink = (this.route.snapshot.data['backLink'] as string | undefined) ?? '/';
  readonly inWorkspace = !!this.route.snapshot.data['workspace'];
  readonly title = this.kind === 'events' ? 'Discover events made for the moment.' : 'Find a venue that fits the experience.';
  readonly copy = this.kind === 'events' ? 'Search published events, then continue to tickets, seats and arrival services.' : 'Explore venue information from the connected venue API.';
  readonly loading = signal(true);
  readonly error = signal('');
  readonly events = signal<EventSummary[]>([]);
  readonly venues = signal<VenueSummary[]>([]);

  ngOnInit(): void {
    this.seo.setPage(this.kind === 'events' ? 'Events | Nvent' : 'Venues | Nvent', this.copy);
    if (this.kind === 'events') {
      this.domain.events().subscribe({ next: (items) => { this.events.set(Array.isArray(items) ? items : []); this.loading.set(false); }, error: (error) => { this.error.set(httpErrorMessage(error, 'Events could not be loaded.')); this.loading.set(false); } });
    } else {
      this.domain.venues().subscribe({ next: (items) => { this.venues.set(Array.isArray(items) ? items : []); this.loading.set(false); }, error: (error) => { this.error.set(httpErrorMessage(error, 'Venues could not be loaded.')); this.loading.set(false); } });
    }
  }

  retry(): void { this.loading.set(true); this.error.set(''); this.ngOnInit(); }
  eventTitle(item: EventSummary): string { return item.title ?? item.name ?? 'Event'; }
}
