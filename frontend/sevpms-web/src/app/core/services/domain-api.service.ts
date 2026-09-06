import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { AdminDashboardStatsDto, AuditLogDto, BackendSeatHoldResponse, BookingSummary, EventCategoryDto, EventRatingSummaryDto, EventReviewDto, EventSummary, FoodMenuItemDto, FoodOrderDto, FoodStallDto, NearbyPlaceDto, NotificationItem, ParkingReservationDto, ParkingSlotDto, ParkingZoneDto, PayHereCheckoutDto, PaymentResponseDto, PublishedSeatingLayoutDto, SavedVehicleDto, SeatApiModel, SeatCategoryDto, SeatSectionDto, SeatViewAssetDto, SeatingLayoutDto, TicketApiModel, UpsertEventCategoryRequest, UpsertParkingSlotRequest, UpsertParkingZoneRequest, UpsertVenueRequest, UserProfileDto, VenueAvailabilityDto, VenueFacilityDto, VenueMarketplaceDto, VenueMediaDto, VenueRateDto, VenueRentalDto, VenueSummary, VenueLayoutTemplateDto, WaitlistEntryDto } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class DomainApiService {
  private readonly api = inject(ApiService);

  eventCategories(): Observable<EventCategoryDto[]> { return this.api.get<EventCategoryDto[]>('event-categories'); }
  adminEventCategories(): Observable<EventCategoryDto[]> { return this.api.get<EventCategoryDto[]>('event-categories/admin'); }
  createEventCategory(body: UpsertEventCategoryRequest): Observable<EventCategoryDto> { return this.api.post<EventCategoryDto>('event-categories', body); }
  updateEventCategory(id: string, body: UpsertEventCategoryRequest): Observable<EventCategoryDto> { return this.api.put<EventCategoryDto>(`event-categories/${id}`, body); }
  deactivateEventCategory(id: string): Observable<void> { return this.api.delete<void>(`event-categories/${id}`); }
  events(params?: Record<string, string | number | boolean | undefined>): Observable<EventSummary[]> { return this.api.get<EventSummary[]>('events', params); }
  event(id: string): Observable<EventSummary> { return this.api.get<EventSummary>(`events/${id}`); }
  myEvents(): Observable<EventSummary[]> { return this.api.get<EventSummary[]>('events/mine'); }
  createEvent(body: unknown): Observable<EventSummary> { return this.api.post<EventSummary>('events', body); }
  updateEvent(id: string, body: unknown): Observable<EventSummary> { return this.api.put<EventSummary>(`events/${id}`, body); }
  publishEvent(id: string): Observable<unknown> { return this.api.put(`events/${id}/publish`, {}); }
  cancelEvent(id: string): Observable<EventSummary> { return this.api.put<EventSummary>(`events/${id}/cancel`, {}); }

  venues(params?: Record<string, string | number | boolean | undefined>): Observable<VenueSummary[]> { return this.api.get<VenueSummary[]>('venues', params); }
  venue(id: string): Observable<VenueSummary> { return this.api.get<VenueSummary>(`venues/${id}`); }
  myVenues(): Observable<VenueSummary[]> { return this.api.get<VenueSummary[]>('venues/mine'); }
  createVenue(body: UpsertVenueRequest): Observable<VenueSummary> { return this.api.post<VenueSummary>('venues', body); }
  updateVenue(id: string, body: UpsertVenueRequest): Observable<VenueSummary> { return this.api.put<VenueSummary>(`venues/${id}`, body); }
  deactivateVenue(id: string): Observable<void> { return this.api.delete<void>(`venues/${id}`); }


  venueFacilities(): Observable<VenueFacilityDto[]> { return this.api.get<VenueFacilityDto[]>('venue-facilities'); }
  createVenueFacility(body: { name: string; category: string; isActive: boolean }): Observable<VenueFacilityDto> { return this.api.post<VenueFacilityDto>('venue-facilities', body); }
  updateVenueFacility(id: string, body: { name: string; category: string; isActive: boolean }): Observable<VenueFacilityDto> { return this.api.put<VenueFacilityDto>(`venue-facilities/${id}`, body); }
  venueMarketplace(venueId: string): Observable<VenueMarketplaceDto> { return this.api.get<VenueMarketplaceDto>(`venues/${venueId}/marketplace`); }
  setVenueFacilities(venueId: string, facilityIds: string[]): Observable<void> { return this.api.put<void>(`venues/${venueId}/marketplace/facilities`, { facilityIds }); }
  addVenueMedia(venueId: string, body: { url: string; type: string; sortOrder: number }): Observable<VenueMediaDto> { return this.api.post<VenueMediaDto>(`venues/${venueId}/marketplace/media`, body); }
  addVenueRate(venueId: string, body: { rateType: string; amount: number; currency: string; validFromUtc?: string | null; validToUtc?: string | null }): Observable<VenueRateDto> { return this.api.post<VenueRateDto>(`venues/${venueId}/marketplace/rates`, body); }
  addVenueAvailability(venueId: string, body: { startAtUtc: string; endAtUtc: string; type: number; notes?: string | null }): Observable<VenueAvailabilityDto> { return this.api.post<VenueAvailabilityDto>(`venues/${venueId}/marketplace/availability`, body); }
  addVenueLayoutTemplate(venueId: string, body: { name: string; version: number; layoutJson: string }): Observable<VenueLayoutTemplateDto> { return this.api.post<VenueLayoutTemplateDto>(`venues/${venueId}/marketplace/layout-templates`, body); }

  bookings(): Observable<BookingSummary[]> { return this.api.get<BookingSummary[]>('bookings'); }
  booking(id: string): Observable<BookingSummary> { return this.api.get<BookingSummary>(`bookings/${id}`); }
  createBooking(body: unknown): Observable<BookingSummary> { return this.api.post<BookingSummary>('bookings', body); }
  cancelBooking(id: string): Observable<BookingSummary> { return this.api.delete<BookingSummary>(`bookings/${id}`); }


  me(): Observable<UserProfileDto> { return this.api.get<UserProfileDto>('users/me'); }
  updateMe(body: unknown): Observable<UserProfileDto> { return this.api.put<UserProfileDto>('users/me', body); }
  changePassword(body: { currentPassword: string; newPassword: string }): Observable<unknown> { return this.api.put('users/me/password', body); }
  health(): Observable<unknown> { return this.api.get('health'); }
  venueRentalsMine(): Observable<VenueRentalDto[]> { return this.api.get<VenueRentalDto[]>('venue-rentals/mine'); }
  createVenueRental(body: { venueId: string; startAtUtc: string; endAtUtc: string; purpose: string; offeredAmount: number }): Observable<VenueRentalDto> { return this.api.post<VenueRentalDto>('venue-rentals', body); }
  venueRentalsIncoming(): Observable<VenueRentalDto[]> { return this.api.get<VenueRentalDto[]>('venue-rentals/incoming'); }
  updateVenueRentalStatus(id: string, status: 1 | 2 | 3, ownerMessage?: string | null): Observable<VenueRentalDto> { return this.api.put<VenueRentalDto>(`venue-rentals/${id}/status`, { status, ownerMessage: ownerMessage || null }); }

  notifications(): Observable<NotificationItem[]> { return this.api.get<NotificationItem[]>('notifications'); }
  markNotificationRead(id: string): Observable<unknown> { return this.api.put(`notifications/${id}/read`, {}); }

  seats(eventId: string): Observable<SeatApiModel[]> { return this.api.get<SeatApiModel[]>(`events/${eventId}/seats`); }
  holdSeats(eventId: string, body: { seatIds: string[]; existingHoldToken?: string | null }): Observable<BackendSeatHoldResponse> { return this.api.post<BackendSeatHoldResponse>(`events/${eventId}/seat-holds`, body); }
  releaseHold(token: string): Observable<unknown> { return this.api.delete(`seat-holds/${encodeURIComponent(token)}`); }
  commitHold(token: string, body: unknown): Observable<unknown> { return this.api.post(`seat-holds/${encodeURIComponent(token)}/commit`, body); }
  publishedLayout(eventId: string): Observable<PublishedSeatingLayoutDto> { return this.api.get<PublishedSeatingLayoutDto>(`events/${eventId}/seating-layout/published`); }
  organizerLayout(eventId: string): Observable<SeatingLayoutDto> { return this.api.get<SeatingLayoutDto>(`events/${eventId}/seating-layout/organizer`); }
  saveLayout(eventId: string, body: unknown): Observable<SeatingLayoutDto> { return this.api.put<SeatingLayoutDto>(`events/${eventId}/seating-layout`, body); }
  saveSections(eventId: string, body: unknown): Observable<SeatSectionDto> { return this.api.put<SeatSectionDto>(`events/${eventId}/seating-layout/sections`, body); }
  saveCategories(eventId: string, body: unknown): Observable<SeatCategoryDto> { return this.api.put<SeatCategoryDto>(`events/${eventId}/seating-layout/categories`, body); }
  generateSeats(eventId: string, body: unknown): Observable<SeatApiModel[]> { return this.api.post<SeatApiModel[]>(`events/${eventId}/seating-layout/generate-seats`, body); }
  publishLayout(eventId: string, body: unknown = {}): Observable<SeatingLayoutDto> { return this.api.put<SeatingLayoutDto>(`events/${eventId}/seating-layout/publish`, body); }
  seatView(eventId: string, seatId: string): Observable<SeatViewAssetDto> { return this.api.get<SeatViewAssetDto>(`events/${eventId}/seats/${seatId}/view`); }

  ticketsForBooking(bookingId: string): Observable<TicketApiModel[]> { return this.api.get<TicketApiModel[]>(`bookings/${bookingId}/tickets`); }
  ticket(ticketNo: string): Observable<TicketApiModel> { return this.api.get<TicketApiModel>(`tickets/${encodeURIComponent(ticketNo)}`); }
  issueTickets(bookingId: string, body: unknown = {}): Observable<TicketApiModel[]> { return this.api.post<TicketApiModel[]>(`bookings/${bookingId}/tickets/issue`, body); }
  scanTicket(eventId: string, body: unknown): Observable<unknown> { return this.api.post(`events/${eventId}/check-ins/scan`, body); }

  payments(): Observable<PaymentResponseDto[]> { return this.api.get<PaymentResponseDto[]>('payments'); }
  createPayment(body: { bookingId: string }): Observable<PaymentResponseDto> { return this.api.post<PaymentResponseDto>('payments', body); }
  payHereCheckout(id: string): Observable<PayHereCheckoutDto> { return this.api.post<PayHereCheckoutDto>(`payments/${id}/payhere-checkout`, {}); }
  completePayment(id: string, body: unknown = {}): Observable<unknown> { return this.api.post(`payments/${id}/complete`, body); }

  vehicles(): Observable<SavedVehicleDto[]> { return this.api.get<SavedVehicleDto[]>('vehicles'); }
  createVehicle(body: { nickname: string; registrationNo: string; vehicleType: string; isDefault: boolean }): Observable<SavedVehicleDto> { return this.api.post<SavedVehicleDto>('vehicles', body); }
  updateVehicle(id: string, body: { nickname: string; registrationNo: string; vehicleType: string; isDefault: boolean }): Observable<SavedVehicleDto> { return this.api.put<SavedVehicleDto>(`vehicles/${id}`, body); }
  deleteVehicle(id: string): Observable<void> { return this.api.delete<void>(`vehicles/${id}`); }
  parkingZones(venueId: string): Observable<ParkingZoneDto[]> { return this.api.get<ParkingZoneDto[]>(`parking/venues/${venueId}/zones`); }
  parkingSlots(zoneId: string): Observable<ParkingSlotDto[]> { return this.api.get<ParkingSlotDto[]>(`parking/zones/${zoneId}/slots`); }
  createParkingZone(body: UpsertParkingZoneRequest): Observable<ParkingZoneDto> { return this.api.post<ParkingZoneDto>('parking/zones', body); }
  updateParkingZone(id: string, body: UpsertParkingZoneRequest): Observable<ParkingZoneDto> { return this.api.put<ParkingZoneDto>(`parking/zones/${id}`, body); }
  deleteParkingZone(id: string): Observable<void> { return this.api.delete<void>(`parking/zones/${id}`); }
  createParkingSlot(body: UpsertParkingSlotRequest): Observable<ParkingSlotDto> { return this.api.post<ParkingSlotDto>('parking/slots', body); }
  updateParkingSlot(id: string, body: UpsertParkingSlotRequest): Observable<ParkingSlotDto> { return this.api.put<ParkingSlotDto>(`parking/slots/${id}`, body); }
  deleteParkingSlot(id: string): Observable<void> { return this.api.delete<void>(`parking/slots/${id}`); }
  parkingRecommendation(body: unknown): Observable<unknown> { return this.api.post('parking/recommendations', body); }
  createParkingReservation(body: { bookingId: string; parkingSlotId: string; vehicleId?: string | null; vehicleRegistration: string }): Observable<ParkingReservationDto> { return this.api.post<ParkingReservationDto>('parking/reservations', body); }
  parkingReservation(id: string): Observable<ParkingReservationDto> { return this.api.get<ParkingReservationDto>(`parking/reservations/${id}`); }
  parkingEnter(id: string): Observable<ParkingReservationDto> { return this.api.post<ParkingReservationDto>(`parking/reservations/${id}/enter`, {}); }
  parkingPark(id: string): Observable<ParkingReservationDto> { return this.api.post<ParkingReservationDto>(`parking/reservations/${id}/park`, {}); }
  parkingExit(id: string): Observable<ParkingReservationDto> { return this.api.post<ParkingReservationDto>(`parking/reservations/${id}/exit`, {}); }
  cancelParkingReservation(id: string): Observable<void> { return this.api.delete<void>(`parking/reservations/${id}`); }

  foodStalls(eventId: string): Observable<FoodStallDto[]> { return this.api.get<FoodStallDto[]>(`food/events/${eventId}/stalls`); }
  foodMenu(stallId: string): Observable<FoodMenuItemDto[]> { return this.api.get<FoodMenuItemDto[]>(`food/stalls/${stallId}/menu`); }
  foodOrders(): Observable<FoodOrderDto[]> { return this.api.get<FoodOrderDto[]>('food/orders'); }
  createFoodOrder(body: { eventId: string; eventFoodStallId: string; bookingId?: string | null; fulfillmentType: string; seatLabelSnapshot?: string | null; items: Array<{ menuItemId: string; quantity: number }> }): Observable<FoodOrderDto> { return this.api.post<FoodOrderDto>('food/orders', body); }

  places(venueId: string, params?: Record<string, string | number | boolean | undefined>): Observable<NearbyPlaceDto[]> { return this.api.get<NearbyPlaceDto[]>(`places/venues/${venueId}`, params); }
  placeRecommendations(venueId: string, audienceMode?: string, category?: string, maxDistanceKm?: number, includeClosed = false): Observable<NearbyPlaceDto[]> { return this.api.get<NearbyPlaceDto[]>(`places/venues/${venueId}/recommendations`, { audienceMode: audienceMode || undefined, category: category || undefined, maxDistanceKm, includeClosed }); }
  createPlace(body: unknown): Observable<NearbyPlaceDto> { return this.api.post<NearbyPlaceDto>('places', body); }
  updatePlace(id: string, body: unknown): Observable<NearbyPlaceDto> { return this.api.put<NearbyPlaceDto>(`places/${id}`, body); }
  deletePlace(id: string): Observable<void> { return this.api.delete<void>(`places/${id}`); }


  eventReviews(eventId: string): Observable<EventReviewDto[]> { return this.api.get<EventReviewDto[]>(`events/${eventId}/reviews`); }
  eventReviewSummary(eventId: string): Observable<EventRatingSummaryDto> { return this.api.get<EventRatingSummaryDto>(`events/${eventId}/reviews/summary`); }
  createEventReview(eventId: string, body: { rating: number; title?: string; comment?: string }): Observable<EventReviewDto> { return this.api.post<EventReviewDto>(`events/${eventId}/reviews`, body); }
  waitlistMine(eventId: string): Observable<WaitlistEntryDto> { return this.api.get<WaitlistEntryDto>(`waitlists/events/${eventId}/me`); }
  joinWaitlist(eventId: string): Observable<WaitlistEntryDto> { return this.api.post<WaitlistEntryDto>(`waitlists/events/${eventId}`, {}); }
  leaveWaitlist(eventId: string): Observable<void> { return this.api.delete<void>(`waitlists/events/${eventId}`); }
  eventRecommendations(preferredCategories: string[] = [], limit = 10): Observable<unknown[]> { return this.api.get<unknown[]>('recommendations/events', { preferredCategories: preferredCategories.join(','), limit }); }

  adminStats(): Observable<AdminDashboardStatsDto> { return this.api.get<AdminDashboardStatsDto>('admin/dashboard/stats'); }
  adminUsers(params?: Record<string, string | number | boolean | undefined>): Observable<unknown[]> { return this.api.get<unknown[]>('admin/users', params); }
  auditLogs(params?: Record<string, string | number | boolean | undefined>): Observable<AuditLogDto[]> { return this.api.get<AuditLogDto[]>('admin/audit-logs', params); }
  updateAdminUserStatus(id: string, status: 0 | 1 | 2): Observable<unknown> { return this.api.put(`admin/users/${id}/status`, { status }); }
  platformReport(): Observable<unknown> { return this.api.get('reports/platform'); }
  organizerReport(): Observable<unknown> { return this.api.get('reports/organizer'); }
  venueOwnerReport(): Observable<unknown> { return this.api.get('reports/venue-owner'); }
}
