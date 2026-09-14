import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { catchError, forkJoin, of } from 'rxjs';
import { EventsService } from '../../core/services/events.service';
import { VenuesService } from '../../core/services/venues.service';
import { BookingsService } from '../../core/services/bookings.service';
import { AuthService } from '../../core/services/auth.service';
import {
  BookingDto,
  EventDto,
  EventRatingSummaryDto,
  EventReviewDto,
  EventWeatherDto,
  PublishedSeatingLayout,
  USER_ROLE,
  VenueDto,
  WaitlistEntryDto
} from '../../core/models/api.models';
import { PageStateComponent } from '../../shared/components/page-state.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge.component';

@Component({
  selector: 'app-event-detail-page',
  standalone: true,
  imports: [RouterLink, DatePipe, DecimalPipe, FormsModule, PageStateComponent, StatusBadgeComponent],
  template: `
  <div class="nv-page">
    @if(loading()){
      <div class="space-y-5">
        <div class="nv-skeleton h-72 sm:h-96"></div>
        <div class="grid xl:grid-cols-[1fr_360px] gap-6"><div class="nv-skeleton h-72"></div><div class="nv-skeleton h-72"></div></div>
      </div>
    }@else if(!event()){
      <app-page-state kind="error" title="Event unavailable" message="This event could not be loaded." actionLabel="Browse Events" [action]="browseEvents"/>
    }@else{
      <div class="grid xl:grid-cols-[1fr_360px] gap-6">
        <div class="space-y-6">
          <section class="nv-card overflow-hidden">
            <div class="relative h-[280px] sm:h-[390px]">
              @if(cover){<img [src]="cover" [alt]="event()!.title" class="w-full h-full object-cover" (error)="cover='';imgError=true">}
              @if(!cover||imgError){<div class="absolute inset-0 nv-image-fallback event" [style.backgroundImage]="eventFallbackBackground"></div>}
              <div class="absolute inset-0" style="background:linear-gradient(180deg,rgba(26,38,86,.04) 20%,rgba(15,23,56,.90) 100%)"></div>
              <div class="absolute bottom-0 left-0 right-0 p-5 sm:p-7 text-white">
                <div class="flex flex-wrap gap-2 mb-3">
                  <span class="nv-status border-white/20 bg-black/20 text-white">{{event()!.category}}</span>
                  @if(soldOut()){<span class="nv-status border-white/20 bg-black/30 text-white">Sold Out</span>}
                </div>
                <h1 class="text-3xl sm:text-5xl font-black tracking-[-.05em]">{{event()!.title}}</h1>
                <p class="mt-3 text-white/80">{{event()!.startAtUtc|date:'EEEE, MMMM d, y · h:mm a'}}</p>
              </div>
            </div>
          </section>

          <section class="grid sm:grid-cols-2 xl:grid-cols-4 gap-3">
            <div class="nv-card-soft p-4"><div class="text-xs font-extrabold nv-muted">Date & time</div><div class="font-black mt-1">{{event()!.startAtUtc|date:'MMM d · h:mm a'}}</div></div>
            <div class="nv-card-soft p-4"><div class="text-xs font-extrabold nv-muted">Venue</div><div class="font-black mt-1">{{venue()?.name||'Venue information'}}</div></div>
            <div class="nv-card-soft p-4"><div class="text-xs font-extrabold nv-muted">Seats available</div><div class="font-black mt-1">{{layout()?availableSeats():'Check seat map'}}</div></div>
            <div class="nv-card-soft p-4"><div class="text-xs font-extrabold nv-muted">Rating</div><div class="font-black mt-1">{{rating().reviewCount ? ((rating().averageRating|number:'1.1-1')+' / 5') : 'No reviews yet'}}</div></div>
          </section>

          <section class="nv-card p-6">
            <h2 class="text-xl font-black">About this event</h2>
            <p class="nv-muted mt-3 whitespace-pre-line">{{event()!.description||'Event details will appear here when provided by the organizer.'}}</p>
          </section>

          @if(weather()?.available){
            <section class="nv-card p-6">
              <div class="flex flex-wrap items-start justify-between gap-4">
                <div><p class="text-sm font-extrabold nv-muted">Event weather</p><h2 class="text-xl font-black mt-1">{{weather()!.condition||'Forecast'}}</h2><p class="nv-muted mt-1">{{weather()!.location}}</p></div>
                <div class="text-right"><div class="font-black text-xl">{{weather()!.minimumTemperatureC ?? '—'}}° – {{weather()!.maximumTemperatureC ?? '—'}}°C</div>@if(weather()!.precipitationProbabilityPercent!==null&&weather()!.precipitationProbabilityPercent!==undefined){<div class="text-sm nv-muted mt-1">Rain {{weather()!.precipitationProbabilityPercent}}%</div>}</div>
              </div>
              @if(weather()!.warning){<div class="mt-4 rounded-2xl p-3 text-sm" style="background:color-mix(in srgb,var(--warning) 12%,var(--surface));color:var(--warning)">{{weather()!.warning}}</div>}
            </section>
          }

          @if(venue()){
            <section class="nv-card p-6">
              <div class="flex flex-wrap items-start justify-between gap-4">
                <div><p class="text-sm font-extrabold nv-muted">Venue</p><h2 class="text-xl font-black mt-1">{{venue()!.name}}</h2><p class="nv-muted mt-2">{{venue()!.addressLine1}}, {{venue()!.city}}, {{venue()!.district}}</p></div>
                <a [routerLink]="['/app/venues',venue()!.venueId]" class="nv-btn nv-btn-secondary">View Venue</a>
              </div>
              <div class="grid sm:grid-cols-3 gap-3 mt-5">
                <div class="nv-card-soft p-4"><div class="text-sm nv-muted">Capacity</div><div class="font-black mt-1">{{venue()!.capacity}}</div></div>
                <div class="nv-card-soft p-4"><div class="text-sm nv-muted">Parking</div><div class="font-black mt-1">Check availability</div></div>
                <div class="nv-card-soft p-4"><div class="text-sm nv-muted">Location</div><div class="font-black mt-1">{{venue()!.city}}</div></div>
              </div>
            </section>
          }

          <section class="nv-card p-6">
            <div class="flex flex-wrap items-end justify-between gap-4">
              <div><p class="text-sm font-extrabold nv-muted">Attendee feedback</p><h2 class="text-xl font-black mt-1">Reviews</h2></div>
              @if(rating().reviewCount){<div class="text-right"><div class="text-2xl font-black">{{rating().averageRating|number:'1.1-1'}} / 5</div><div class="text-sm nv-muted">{{rating().reviewCount}} review{{rating().reviewCount===1?'':'s'}}</div></div>}
            </div>
            @if(reviews().length){
              <div class="grid md:grid-cols-2 gap-3 mt-5">
                @for(review of reviews().slice(0,4);track review.id){
                  <article class="nv-card-soft p-4"><div class="font-black">{{review.rating}} / 5</div><p class="nv-muted text-sm mt-2">{{review.comment||'Rating submitted without a written comment.'}}</p><time class="text-xs nv-muted block mt-3">{{review.createdAtUtc|date:'MMM d, y'}}</time></article>
                }
              </div>
            }@else{<p class="nv-muted text-sm mt-4">No attendee reviews have been submitted yet.</p>}

            @if(canReview()){
              <form class="mt-5 border-t pt-5" style="border-color:var(--border)" (ngSubmit)="submitReview()">
                <h3 class="font-black">Review your experience</h3>
                <p class="nv-muted text-sm mt-1">Reviews are available only to eligible attendees with a completed booking.</p>
                <div class="grid sm:grid-cols-[160px_1fr_auto] gap-3 mt-3 items-end">
                  <label><span class="nv-label">Rating</span><select class="nv-input" name="rating" [(ngModel)]="reviewRating" aria-label="Rating"><option [ngValue]="5">5 - Excellent</option><option [ngValue]="4">4 - Good</option><option [ngValue]="3">3 - Okay</option><option [ngValue]="2">2 - Poor</option><option [ngValue]="1">1 - Very poor</option></select></label>
                  <label><span class="nv-label">Comment</span><input class="nv-input" name="reviewComment" [(ngModel)]="reviewComment" placeholder="Share a short comment" aria-label="Share a short comment"></label>
                  <button type="submit" class="nv-btn nv-btn-primary" [disabled]="reviewBusy()">{{reviewBusy()?'Submitting...':'Submit Review'}}</button>
                </div>
                @if(reviewMessage()){<p class="text-sm mt-3" role="status">{{reviewMessage()}}</p>}
              </form>
            }
          </section>

          <section class="grid md:grid-cols-3 gap-4">
            <a [routerLink]="['/app/parking']" [queryParams]="{event:event()!.eventId,venue:event()!.venueId}" class="nv-card p-5"><div class="font-black">Parking</div><p class="nv-muted text-sm mt-1">Find zones and reserve a slot for this venue.</p></a>
            <a routerLink="/app/food" [queryParams]="{event:event()!.eventId}" class="nv-card p-5"><div class="font-black">Food & Drinks</div><p class="nv-muted text-sm mt-1">Browse stalls attached to this event.</p></a>
            <a routerLink="/app/places" [queryParams]="{venue:event()!.venueId}" class="nv-card p-5"><div class="font-black">Places nearby</div><p class="nv-muted text-sm mt-1">Explore recommendations around the venue.</p></a>
          </section>
        </div>

        <aside>
          <div class="nv-card p-5 xl:sticky xl:top-6">
            <app-status-badge [label]="soldOut()?'Sold Out':statusLabel" [tone]="soldOut()?'danger':statusTone"/>
            <h2 class="text-xl font-black mt-4">Booking</h2>
            <div class="space-y-3 mt-4 text-sm">
              <div class="flex justify-between gap-3"><span class="nv-muted">Date</span><strong>{{event()!.startAtUtc|date:'MMM d, y'}}</strong></div>
              <div class="flex justify-between gap-3"><span class="nv-muted">Time</span><strong>{{event()!.startAtUtc|date:'h:mm a'}}</strong></div>
              <div class="flex justify-between gap-3"><span class="nv-muted">Venue</span><strong class="text-right">{{venue()?.name||'Venue'}}</strong></div>
              @if(layout()){<div class="flex justify-between gap-3"><span class="nv-muted">Seat availability</span><strong>{{availableSeats()}} / {{layout()!.seats.length}}</strong></div>}
            </div>

            @if(event()!.status===1 && !soldOut()){
              <a [routerLink]="['/app/events',event()!.eventId,'seats']" class="nv-btn nv-btn-primary w-full mt-6">Select Seats</a>
            }@else if(event()!.status===1 && soldOut()){
              @if(isCustomer()){
                @if(waitlist()?.status===0 || waitlistStatus('waiting')){
                  <div class="nv-card-soft p-4 mt-6"><div class="font-black">You're on the waitlist</div><p class="nv-muted text-sm mt-1">@if(waitlist()?.position){Current position: {{waitlist()!.position}}. }You'll be notified if seats become available.</p><button type="button" class="nv-btn nv-btn-secondary w-full mt-3" (click)="leaveWaitlist()" [disabled]="waitlistBusy()">Leave Waitlist</button></div>
                }@else if(waitlist()?.status===1 || waitlistStatus('eligible')){
                  <div class="nv-card-soft p-4 mt-6"><div class="font-black" style="color:var(--success)">A seat may be available</div><p class="nv-muted text-sm mt-1">Your waitlist entry is eligible. Check the seat map now.</p><a [routerLink]="['/app/events',event()!.eventId,'seats']" class="nv-btn nv-btn-primary w-full mt-3">Check Seats</a></div>
                }@else{
                  <button type="button" class="nv-btn nv-btn-primary w-full mt-6" (click)="joinWaitlist()" [disabled]="waitlistBusy()">{{waitlistBusy()?'Joining...':'Join Waitlist'}}</button>
                }
                @if(waitlistMessage()){<p class="text-xs nv-muted mt-2" role="status">{{waitlistMessage()}}</p>}
              }@else{
                <button type="button" class="nv-btn nv-btn-primary w-full mt-6" (click)="signInForWaitlist()">Sign In for Waitlist</button>
              }
            }@else if(event()!.status===2){
              <button type="button" class="nv-btn nv-btn-secondary w-full mt-6" disabled>Event Cancelled</button>
            }@else{
              <button type="button" class="nv-btn nv-btn-secondary w-full mt-6" disabled>Booking Unavailable</button>
            }
          </div>
        </aside>
      </div>
    }
  </div>`
})
export class EventDetailPageComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private eventsApi = inject(EventsService);
  private venuesApi = inject(VenuesService);
  private bookingsApi = inject(BookingsService);
  private auth = inject(AuthService);

  event = signal<EventDto | null>(null);
  venue = signal<VenueDto | null>(null);
  layout = signal<PublishedSeatingLayout | null>(null);
  weather = signal<EventWeatherDto | null>(null);
  rating = signal<EventRatingSummaryDto>({ eventId: '', reviewCount: 0, averageRating: 0 });
  reviews = signal<EventReviewDto[]>([]);
  bookings = signal<BookingDto[]>([]);
  waitlist = signal<WaitlistEntryDto | null>(null);
  loading = signal(true);
  reviewBusy = signal(false);
  waitlistBusy = signal(false);
  reviewMessage = signal('');
  waitlistMessage = signal('');
  imgError = false;
  cover = '';
  reviewRating = 5;
  reviewComment = '';

  browseEvents = () => this.router.navigateByUrl('/app/events');

  get statusLabel() {
    const event=this.event();
    if(!event)return 'Event';
    if(event.status===0)return 'Draft';
    if(event.status===2)return 'Cancelled';
    if(event.status===3)return 'Completed';
    const now=Date.now(),start=Date.parse(event.startAtUtc),end=Date.parse(event.endAtUtc);
    if(Number.isFinite(start)&&Number.isFinite(end)&&start<=now&&now<end)return 'Ongoing';
    if(Number.isFinite(end)&&end<=now)return 'Past';
    return 'Upcoming';
  }
  get statusTone(){return this.statusLabel==='Cancelled'?'danger':this.statusLabel==='Ongoing'?'info':this.statusLabel==='Upcoming'?'success':'neutral';}
  get eventFallbackBackground(){const c=(this.event()?.category||'').toLowerCase();const file=c.includes('concert')?'concert':c.includes('festival')?'festival':c.includes('sport')?'sports':c.includes('conference')?'conference':c.includes('theatre')||c.includes('theater')?'theatre':c.includes('family')?'family-event':c.includes('wedding')?'wedding-event':'event-default';return `linear-gradient(135deg,rgba(15,23,56,.08),rgba(15,23,56,.60)),url('assets/nvent/images/events/${file}.webp'),linear-gradient(135deg,#92ABD3,#E49C95,#FAC098)`;}
  isCustomer() { return this.auth.isAuthenticated() && this.auth.role() === USER_ROLE.Customer; }
  waitlistStatus(value:string) { return String(this.waitlist()?.status ?? '').toLowerCase() === value.toLowerCase(); }
  availableSeats() { return this.layout()?.seats.filter(s => String(s.state).toLowerCase() === 'available').length ?? 0; }
  soldOut() { return !!this.layout()?.seats.length && this.availableSeats() === 0; }
  canReview() {
    if (!this.isCustomer()) return false;
    const eventId = this.event()?.eventId;
    if (!eventId || this.reviews().some(r => r.customerUserId === this.auth.session()?.userId)) return false;
    return this.bookings().some(b => b.eventId === eventId && b.status === 3);
  }
  reviewBookingId() {
    return this.bookings().find(b => b.eventId === this.event()?.eventId && b.status === 3)?.bookingId ?? '';
  }

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.eventsApi.get(id).subscribe({
      next: event => {
        this.event.set(event);
        this.eventsApi.cover(event.eventId).subscribe({ next: v => this.cover = v.url, error: () => this.cover = '' });

        const customer = this.isCustomer();
        forkJoin({
          venue: this.venuesApi.get(event.venueId).pipe(catchError(() => of(null))),
          layout: this.eventsApi.layout(event.eventId).pipe(catchError(() => of(null))),
          weather: this.eventsApi.weather(event.eventId).pipe(catchError(() => of(null))),
          rating: this.eventsApi.reviewsSummary(event.eventId).pipe(catchError(() => of({ eventId: event.eventId, reviewCount: 0, averageRating: 0 }))),
          reviews: this.eventsApi.reviews(event.eventId).pipe(catchError(() => of([]))),
          bookings: customer ? this.bookingsApi.list().pipe(catchError(() => of([]))) : of([] as BookingDto[]),
          waitlist: customer ? this.eventsApi.myWaitlist(event.eventId) : of(null)
        }).subscribe(result => {
          this.venue.set(result.venue);
          this.layout.set(result.layout);
          this.weather.set(result.weather);
          this.rating.set(result.rating);
          this.reviews.set(result.reviews);
          this.bookings.set(result.bookings);
          this.waitlist.set(result.waitlist);
          this.loading.set(false);
        });
      },
      error: () => this.loading.set(false)
    });
  }

  submitReview() {
    const eventId = this.event()?.eventId;
    const bookingId = this.reviewBookingId();
    if (!eventId || !bookingId) return;
    this.reviewBusy.set(true);
    this.reviewMessage.set('');
    this.eventsApi.createReview(eventId, bookingId, this.reviewRating, this.reviewComment).subscribe({
      next: review => {
        this.reviews.update(rows => [review, ...rows]);
        this.rating.update(r => {
          const count = r.reviewCount + 1;
          return { ...r, reviewCount: count, averageRating: ((r.averageRating * r.reviewCount) + review.rating) / count };
        });
        this.reviewComment = '';
        this.reviewBusy.set(false);
        this.reviewMessage.set('Review submitted.');
      },
      error: err => {
        this.reviewBusy.set(false);
        this.reviewMessage.set(err?.error?.error || err?.error?.message || 'Your review could not be submitted.');
      }
    });
  }

  joinWaitlist() {
    const id = this.event()?.eventId;
    if (!id) return;
    this.waitlistBusy.set(true);
    this.waitlistMessage.set('');
    this.eventsApi.joinWaitlist(id).subscribe({
      next: entry => { this.waitlist.set(entry); this.waitlistBusy.set(false); },
      error: err => { this.waitlistBusy.set(false); this.waitlistMessage.set(err?.error?.error || 'Waitlist could not be joined.'); }
    });
  }

  leaveWaitlist() {
    const id = this.event()?.eventId;
    if (!id) return;
    this.waitlistBusy.set(true);
    this.eventsApi.leaveWaitlist(id).subscribe({
      next: () => { this.waitlist.set(null); this.waitlistBusy.set(false); this.waitlistMessage.set('You left the waitlist.'); },
      error: err => { this.waitlistBusy.set(false); this.waitlistMessage.set(err?.error?.error || 'Waitlist could not be updated.'); }
    });
  }

  signInForWaitlist() {
    void this.router.navigate(['/auth/sign-in'], { queryParams: { returnUrl: this.router.url } });
  }
}
