import { Component, ElementRef, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { PreviewCard } from '../../../core/models/nvent.models';
import { SeoService } from '../../../core/services/seo.service';
import { RevealOnScrollDirective } from '../../../shared/directives/reveal-on-scroll.directive';
import { MagicCardDirective } from '../../../shared/directives/magic-card.directive';
import { EcosystemMotionComponent } from '../../../shared/components/ecosystem-motion/ecosystem-motion';
import { TubesBackgroundComponent } from '../../../shared/components/tubes-background/tubes-background';
import { EndFlowComponent } from '../../../shared/components/end-flow/end-flow';
import { LazyVideoDirective } from '../../../shared/directives/lazy-video.directive';

@Component({
  selector: 'app-home',
  imports: [
    RouterLink,
    RevealOnScrollDirective,
    MagicCardDirective,
    EcosystemMotionComponent,
    TubesBackgroundComponent,
    EndFlowComponent,
    LazyVideoDirective,
  ],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class HomeComponent implements OnInit {
  private readonly seo = inject(SeoService);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly domain = inject(DomainApiService);

  readonly categories = ['Conferences', 'Concerts', 'Weddings', 'Corporate Events', 'Exhibitions'];
  readonly eventsFromApi = signal(false);
  readonly eventCards = signal<PreviewCard[]>([
    { eyebrow: 'Design preview', title: 'Live performance experience', copy: 'Cinematic card treatment used until published event data is returned by the API.', image: '/assets/images/concert-poster.jpg', badge: 'Preview', meta: 'Ticket-ready UI', path: '/events' },
    { eyebrow: 'Design preview', title: 'Conference experience', copy: 'Professional event discovery surface prepared for event and venue API data.', image: '/assets/images/conference-poster.jpg', badge: 'Preview', meta: 'Venue + seating', path: '/events' },
    { eyebrow: 'Design preview', title: 'Premium venue arrival', copy: 'A venue-first card treatment that connects naturally to parking and arrival services.', image: '/assets/images/venue-night-poster.jpg', badge: 'Preview', meta: 'Arrival-ready UI', path: '/venues' },
  ]);

  readonly features = [
    { icon: '⌖', title: 'Discover Venues', copy: 'Find spaces that fit the moment, then continue into real availability and event context.', action: 'Explore now', path: '/venues' },
    { icon: '◇', title: 'Manage Events', copy: 'Create, publish and coordinate event journeys from a role-aware organizer workspace.', action: 'Host an event', path: '/organizer/create-event' },
    { icon: '▤', title: 'Sell Tickets', copy: 'Choose seats, receive tickets and use QR entry from one connected journey.', action: 'See tickets', path: '/customer/tickets' },
    { icon: '⌁', title: 'Real Insights', copy: 'Operational dashboards keep reports and performance metrics clear and easy to act on.', action: 'View workspace', path: '/organizer/dashboard' },
  ];

  ngOnInit(): void {
    this.seo.setPage(
      'Nvent | Extraordinary events, seamlessly managed',
      'Discover events and venues, book tickets, manage arrivals and host experiences with Nvent.',
    );
    this.domain.events().subscribe({
      next: (events) => {
        if (!Array.isArray(events) || !events.length) return;
        this.eventsFromApi.set(true);
        this.eventCards.set(events.slice(0, 3).map((event) => ({
          eyebrow: event.categoryName ?? 'Upcoming event',
          title: event.title ?? event.name ?? 'Event',
          copy: event.description ?? 'Open the event to view venue, timing and ticket information.',
          image: event.imageUrl || '/assets/images/conference-poster.jpg',
          badge: event.status ?? 'Upcoming',
          meta: event.venueName ?? event.city ?? event.location ?? 'View details',
          path: `/events/${event.eventId ?? event.id ?? ''}`,
        })));
      },
      error: () => { /* visual preview remains clearly labelled until the API returns event records */ },
    });
  }

  scrollToSearch(): void {
    this.host.nativeElement.querySelector('#event-search')?.scrollIntoView({ behavior: 'smooth', block: 'center' });
  }
}
