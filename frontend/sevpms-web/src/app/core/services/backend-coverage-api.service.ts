import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiService } from './api.service';

export interface CoverageEvent {
  id?: string;
  eventId?: string;
  title?: string;
  name?: string;
  description?: string;
  category?: string;
  categoryName?: string;
  venueId?: string;
  venueName?: string;
  startAtUtc?: string;
  startDateTime?: string;
  endAtUtc?: string;
  status?: string | number;
}

export interface CoverageBooking {
  id?: string;
  bookingId?: string;
  bookingReference?: string;
  bookingNumber?: string;
  eventId?: string;
  eventName?: string;
  totalAmount?: number;
  status?: string | number;
  createdAtUtc?: string;
}

export interface ReceiptDto {
  receiptId: string;
  receiptNumber: string;
  paymentId: string;
  bookingId: string;
  customerUserId: string;
  amount: number;
  currency: string;
  issuedAtUtc: string;
}

export interface ReceiptDeliveryDto {
  receiptDeliveryId: string;
  receiptId: string;
  channel: string;
  destinationMasked: string;
  status: string | number;
  attemptCount: number;
  lastAttemptAtUtc?: string | null;
  sentAtUtc?: string | null;
  lastError?: string | null;
}

export interface BookingCalendarDto {
  bookingId: string;
  eventId: string;
  eventTitle: string;
  venueName: string;
  location: string;
  startAtUtc: string;
  endAtUtc: string;
  googleCalendarUrl: string;
  icsDownloadPath: string;
}

export interface RefundDto {
  refundId: string;
  paymentId: string;
  bookingId: string;
  amount: number;
  currency: string;
  reason: string;
  refundReference: string;
  status: string | number;
  refundedAtUtc?: string | null;
}

export interface EventWeatherDto {
  eventId: string;
  eventTitle: string;
  venueId: string;
  venueName: string;
  eventStartUtc: string;
  location: string;
  available: boolean;
  message?: string | null;
  weatherCode?: number | null;
  condition?: string | null;
  minimumTemperatureC?: number | null;
  maximumTemperatureC?: number | null;
  precipitationProbabilityPercent?: number | null;
  precipitationMm?: number | null;
  maximumWindSpeedKmh?: number | null;
  warning?: string | null;
}

export interface EventReviewDto {
  id?: string;
  eventReviewId?: string;
  eventId?: string;
  rating: number;
  title?: string;
  comment?: string;
  createdAtUtc?: string;
  customerName?: string;
}

export interface RatingSummaryDto {
  averageRating?: number;
  reviewCount?: number;
}

export interface WaitlistDto {
  id?: string;
  waitlistEntryId?: string;
  eventId?: string;
  status?: string | number;
  joinedAtUtc?: string;
  position?: number;
}

export interface RecommendationDto {
  eventId: string;
  venueId: string;
  title: string;
  description: string;
  category: string;
  startAtUtc: string;
  endAtUtc: string;
  recommendationScore: number;
  reasons: string[];
}

export interface SavedVehicleDto {
  id: string;
  nickname: string;
  registrationNo: string;
  vehicleType: string;
  isDefault: boolean;
}

export interface VenueLiteDto {
  id?: string;
  venueId?: string;
  name: string;
  city?: string;
}

export interface ParkingRecommendationDto {
  parkingSlotId: string;
  parkingZoneId: string;
  slotCode: string;
  distanceCost: number;
  isAccessible: boolean;
  reason: string;
}

export interface ParkingNodeDto {
  id: string;
  venueId: string;
  layoutId?: string | null;
  nodeCode: string;
  x: number;
  y: number;
  nodeType: string;
}

export interface ParkingRouteDto {
  startNodeId: string;
  endNodeId: string;
  totalCost: number;
  nodes: ParkingNodeDto[];
}

@Injectable({ providedIn: 'root' })
export class BackendCoverageApiService {
  private readonly api = inject(ApiService);
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl.replace(/\/$/, '');

  events(): Observable<CoverageEvent[]> {
    return this.api.get<CoverageEvent[]>('events');
  }

  bookings(): Observable<CoverageBooking[]> {
    return this.api.get<CoverageBooking[]>('bookings');
  }

  venues(): Observable<VenueLiteDto[]> {
    return this.api.get<VenueLiteDto[]>('venues');
  }

  vehicles(): Observable<SavedVehicleDto[]> {
    return this.api.get<SavedVehicleDto[]>('vehicles');
  }

  receipts(): Observable<ReceiptDto[]> {
    return this.api.get<ReceiptDto[]>('receipts');
  }

  receipt(id: string): Observable<ReceiptDto> {
    return this.api.get<ReceiptDto>(`receipts/${id}`);
  }

  receiptDeliveries(id: string): Observable<ReceiptDeliveryDto[]> {
    return this.api.get<ReceiptDeliveryDto[]>(`receipts/${id}/deliveries`);
  }

  retryReceiptDelivery(id: string): Observable<ReceiptDeliveryDto[]> {
    return this.api.post<ReceiptDeliveryDto[]>(`receipts/${id}/deliveries/retry`, {});
  }

  adminReceipts(): Observable<ReceiptDto[]> {
    return this.api.get<ReceiptDto[]>('admin/receipts', { take: 100 });
  }

  adminReceiptDeliveries(id: string): Observable<ReceiptDeliveryDto[]> {
    return this.api.get<ReceiptDeliveryDto[]>(`admin/receipts/${id}/deliveries`);
  }

  retryAdminReceiptDelivery(id: string): Observable<ReceiptDeliveryDto[]> {
    return this.api.post<ReceiptDeliveryDto[]>(`admin/receipts/${id}/deliveries/retry`, {});
  }

  bookingCalendar(bookingId: string): Observable<BookingCalendarDto> {
    return this.api.get<BookingCalendarDto>(`bookings/${bookingId}/calendar`);
  }

  downloadCalendar(bookingId: string): Observable<Blob> {
    return this.http.get(
      `${this.baseUrl}/bookings/${encodeURIComponent(bookingId)}/calendar.ics`,
      { responseType: 'blob' }
    );
  }

  cancelConfirmedBooking(bookingId: string, reason: string): Observable<RefundDto> {
    return this.api.post<RefundDto>(`bookings/${bookingId}/cancel-confirmed`, { reason });
  }

  weather(eventId: string): Observable<EventWeatherDto> {
    return this.api.get<EventWeatherDto>(`events/${eventId}/weather`);
  }

  reviews(eventId: string): Observable<EventReviewDto[]> {
    return this.api.get<EventReviewDto[]>(`events/${eventId}/reviews`);
  }

  ratingSummary(eventId: string): Observable<RatingSummaryDto> {
    return this.api.get<RatingSummaryDto>(`events/${eventId}/reviews/summary`);
  }

  createReview(eventId: string, body: { rating: number; title?: string; comment?: string }): Observable<EventReviewDto> {
    return this.api.post<EventReviewDto>(`events/${eventId}/reviews`, body);
  }

  waitlistMine(eventId: string): Observable<WaitlistDto> {
    return this.api.get<WaitlistDto>(`waitlists/events/${eventId}/me`);
  }

  joinWaitlist(eventId: string): Observable<WaitlistDto> {
    return this.api.post<WaitlistDto>(`waitlists/events/${eventId}`, {});
  }

  leaveWaitlist(eventId: string): Observable<void> {
    return this.api.delete<void>(`waitlists/events/${eventId}`);
  }

  recommendations(preferredCategories: string[] = [], limit = 10): Observable<RecommendationDto[]> {
    return this.api.get<RecommendationDto[]>('recommendations/events', {
      preferredCategories: preferredCategories.join(','),
      limit,
    });
  }

  parkingNodes(venueId: string): Observable<ParkingNodeDto[]> {
    return this.api.get<ParkingNodeDto[]>(`parking/venues/${venueId}/nodes`);
  }

  parkingRecommendation(body: {
    venueId: string;
    eventId?: string | null;
    entranceNodeId: string;
    requiresAccessibleParking: boolean;
    savedVehicleId?: string | null;
  }): Observable<ParkingRecommendationDto> {
    return this.api.post<ParkingRecommendationDto>('parking/recommendations', body);
  }

  parkingRoute(
    venueId: string,
    startNodeId: string,
    endNodeId: string,
    accessibleOnly: boolean
  ): Observable<ParkingRouteDto> {
    return this.api.get<ParkingRouteDto>('parking/navigation/route', {
      venueId,
      startNodeId,
      endNodeId,
      accessibleOnly,
    });
  }

  requestPasswordReset(email: string): Observable<void> {
    return this.api.post<void>('auth/password-reset/request', { email });
  }

  confirmPasswordReset(token: string, newPassword: string): Observable<void> {
    return this.api.post<void>('auth/password-reset/confirm', { token, newPassword });
  }
}
