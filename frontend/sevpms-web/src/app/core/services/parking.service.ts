import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import {
  ParkingNodeDto,
  ParkingRecommendationDto,
  ParkingReservation,
  ParkingRouteDto,
  ParkingSlot,
  ParkingZone,
  VehicleDto,
  BulkCreateParkingSlotsRequest,
  CreateParkingReservationRequest,
  CreateVehicleRequest,
  ParkingRecommendationRequest,
  UpdateVehicleRequest,
  UpsertParkingSlotRequest,
  UpsertParkingZoneRequest
} from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class ParkingService {
  private api = inject(ApiService);

  nodes(venueId: string) { return this.api.get<ParkingNodeDto[]>(`parking/venues/${venueId}/nodes`); }
  zones(venueId: string) { return this.api.get<ParkingZone[]>(`parking/venues/${venueId}/zones`); }
  slots(zoneId: string) { return this.api.get<ParkingSlot[]>(`parking/zones/${zoneId}/slots`); }
  recommend(body: ParkingRecommendationRequest) { return this.api.post<ParkingRecommendationDto>('parking/recommendations', body); }
  reserve(body: CreateParkingReservationRequest) { return this.api.post<ParkingReservation>('parking/reservations', body); }
  reservation(id: string) { return this.api.get<ParkingReservation>(`parking/reservations/${id}`); }
  enterReservation(id: string) { return this.api.post<ParkingReservation>(`parking/reservations/${id}/enter`, {}); }
  parkReservation(id: string) { return this.api.post<ParkingReservation>(`parking/reservations/${id}/park`, {}); }
  exitReservation(id: string) { return this.api.post<ParkingReservation>(`parking/reservations/${id}/exit`, {}); }
  cancelReservation(id: string) { return this.api.delete<void>(`parking/reservations/${id}`); }
  route(params: Record<string, unknown>) { return this.api.get<ParkingRouteDto>('parking/navigation/route', params); }

  vehicles() { return this.api.get<VehicleDto[]>('vehicles'); }
  addVehicle(body: CreateVehicleRequest) { return this.api.post<VehicleDto>('vehicles', body); }
  updateVehicle(id: string, body: UpdateVehicleRequest) { return this.api.put<VehicleDto>(`vehicles/${id}`, body); }
  removeVehicle(id: string) { return this.api.delete<void>(`vehicles/${id}`); }

  zonesCreate(body: UpsertParkingZoneRequest) { return this.api.post<ParkingZone>('parking/zones', body); }
  zoneUpdate(id: string, body: UpsertParkingZoneRequest) { return this.api.put<ParkingZone>(`parking/zones/${id}`, body); }
  zoneDelete(id: string) { return this.api.delete<void>(`parking/zones/${id}`); }
  slotsCreate(body: UpsertParkingSlotRequest) { return this.api.post<ParkingSlot>('parking/slots', body); }
  slotsBulk(body: BulkCreateParkingSlotsRequest) { return this.api.post<ParkingSlot[]>('parking/slots/bulk', body); }
  slotUpdate(id: string, body: UpsertParkingSlotRequest) { return this.api.put<ParkingSlot>(`parking/slots/${id}`, body); }
  slotDelete(id: string) { return this.api.delete<void>(`parking/slots/${id}`); }
}
