import { Injectable, inject } from '@angular/core';
import { catchError, of } from 'rxjs';
import { ApiService } from './api.service';
import {
  EventCategory,
  EventDto,
  EventRatingSummaryDto,
  EventRecommendationDto,
  EventReviewDto,
  EventWeatherDto,
  PublishedSeatingLayout,
  SeatingLayoutDto,
  SeatAvailabilityDto,
  SeatCategory,
  SeatSection,
  SeatHoldResponse,
  SeatViewAsset,
  WaitlistEntryDto,
  EventUpsertRequest,
  ConfigureSeatingLayoutRequest,
  UpsertSeatSectionRequest,
  UpsertSeatCategoryRequest,
  GenerateSeatsRequest,
  UpsertSeatViewAssetRequest
} from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class EventsService {
  private api = inject(ApiService);

  list(filters: Record<string, unknown> = {}) { return this.api.get<EventDto[]>('events', filters); }
  get(id: string) { return this.api.get<EventDto>(`events/${id}`); }
  mine() { return this.api.get<EventDto[]>('events/mine'); }
  categories() { return this.api.get<EventCategory[]>('event-categories'); }
  recommendations(limit = 6) { return this.api.get<EventRecommendationDto[]>('recommendations/events', { limit }); }

  create(body: EventUpsertRequest) { return this.api.post<EventDto>('events', body); }
  update(id: string, body: EventUpsertRequest) { return this.api.put<EventDto>(`events/${id}`, body); }
  publish(id: string) { return this.api.put<EventDto>(`events/${id}/publish`, {}); }
  cancel(id: string) { return this.api.put<EventDto>(`events/${id}/cancel`, {}); }

  layout(id: string) { return this.api.get<PublishedSeatingLayout>(`events/${id}/seating-layout/published`); }
  organizerLayout(id: string) { return this.api.get<SeatingLayoutDto>(`events/${id}/seating-layout/organizer`); }
  configureLayout(id: string, body: ConfigureSeatingLayoutRequest) { return this.api.put<SeatingLayoutDto>(`events/${id}/seating-layout`, body); }
  setSections(id: string, body: UpsertSeatSectionRequest) { return this.api.put<SeatSection>(`events/${id}/seating-layout/sections`, body); }
  setCategories(id: string, body: UpsertSeatCategoryRequest) { return this.api.put<SeatCategory>(`events/${id}/seating-layout/categories`, body); }
  generateSeats(id: string, body: GenerateSeatsRequest) { return this.api.post<SeatAvailabilityDto[]>(`events/${id}/seating-layout/generate-seats`, body); }
  publishLayout(id: string, publish = true) { return this.api.put<SeatingLayoutDto>(`events/${id}/seating-layout/publish`, { publish }); }

  holdSeats(eventId: string, seatIds: string[], existingHoldToken?: string) {
    return this.api.post<SeatHoldResponse>(`events/${eventId}/seat-holds`, { seatIds, existingHoldToken });
  }
  releaseHold(token: string) { return this.api.delete<void>(`seat-holds/${encodeURIComponent(token)}`); }
  seatView(eventId: string, seatId: string) { return this.api.get<SeatViewAsset>(`events/${eventId}/seats/${seatId}/view`); }
  upsertSeatView(eventId: string, body: UpsertSeatViewAssetRequest) { return this.api.put<SeatViewAsset>(`events/${eventId}/seat-view-assets`, body); }

  weather(eventId: string) { return this.api.get<EventWeatherDto>(`events/${eventId}/weather`); }
  reviews(eventId: string) { return this.api.get<EventReviewDto[]>(`events/${eventId}/reviews`); }
  reviewsSummary(eventId: string) { return this.api.get<EventRatingSummaryDto>(`events/${eventId}/reviews/summary`); }
  createReview(eventId: string, bookingId: string, rating: number, comment: string) {
    return this.api.post<EventReviewDto>(`events/${eventId}/reviews`, { bookingId, rating, comment: comment.trim() || null });
  }

  myWaitlist(eventId: string) {
    return this.api.get<WaitlistEntryDto>(`waitlists/events/${eventId}/me`).pipe(catchError(() => of(null)));
  }
  joinWaitlist(eventId: string) { return this.api.post<WaitlistEntryDto>(`waitlists/events/${eventId}`, {}); }
  leaveWaitlist(eventId: string) { return this.api.delete<void>(`waitlists/events/${eventId}`); }

  cover(eventId: string) { return this.api.get<{ url: string }>(`events/${eventId}/cover`); }
}
