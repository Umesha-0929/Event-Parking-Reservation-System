import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { SeatingLayoutDto } from '../models/api.models';
import {
  KlegarSeatAvailabilityDto,
  KlegarSeatViewAssetDto,
  KlegarUpsertSeatRequest,
  KlegarUpsertSeatViewAssetRequest,
} from '../models/klegar-seat-ticket.models';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class KlegarSeatTicketApiService {
  private readonly api = inject(ApiService);

  organizerLayout(eventId: string): Observable<SeatingLayoutDto> {
    return this.api.get<SeatingLayoutDto>(`events/${eventId}/seating-layout/organizer`);
  }

  saveLayout(eventId: string, body: unknown): Observable<SeatingLayoutDto> {
    return this.api.put<SeatingLayoutDto>(`events/${eventId}/seating-layout`, body);
  }

  seats(eventId: string, sectionId?: string): Observable<KlegarSeatAvailabilityDto[]> {
    return this.api.get<KlegarSeatAvailabilityDto[]>(`events/${eventId}/seats`, {
      sectionId: sectionId || undefined,
    });
  }

  saveSeat(eventId: string, body: KlegarUpsertSeatRequest): Observable<KlegarSeatAvailabilityDto> {
    return this.api.put<KlegarSeatAvailabilityDto>(`events/${eventId}/seats`, body);
  }

  saveSeatViewAsset(
    eventId: string,
    body: KlegarUpsertSeatViewAssetRequest,
  ): Observable<KlegarSeatViewAssetDto> {
    return this.api.put<KlegarSeatViewAssetDto>(`events/${eventId}/seat-view-assets`, body);
  }

  seatView(eventId: string, seatId: string): Observable<KlegarSeatViewAssetDto> {
    return this.api.get<KlegarSeatViewAssetDto>(`events/${eventId}/seats/${seatId}/view`);
  }
}
