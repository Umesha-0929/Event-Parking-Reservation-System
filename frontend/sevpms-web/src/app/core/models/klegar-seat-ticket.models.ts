export interface KlegarSeatAvailabilityDto {
  seatId: string;
  eventId: string;
  sectionId: string;
  rowLabel: string;
  seatNumber: string;
  x: number;
  y: number;
  ticketTypeId?: string | null;
  isAccessible: boolean;
  state: string;
  heldUntilUtc?: string | null;
}

export interface KlegarUpsertSeatRequest {
  seatId?: string | null;
  sectionId: string;
  rowLabel: string;
  seatNumber: string;
  x: number;
  y: number;
  ticketTypeId?: string | null;
  isAccessible: boolean;
  status: 'Available' | 'Blocked';
  seatViewAssetId?: string | null;
}

export interface KlegarSeatViewAssetDto {
  id: string;
  eventId: string;
  sectionId?: string | null;
  rowLabel?: string | null;
  seatId?: string | null;
  mediaUrl: string;
  viewerType: string;
  defaultYaw?: number | null;
  defaultPitch?: number | null;
  defaultFov?: number | null;
  isRepresentative: boolean;
}

export interface KlegarUpsertSeatViewAssetRequest {
  id?: string | null;
  sectionId?: string | null;
  rowLabel?: string | null;
  seatId?: string | null;
  mediaUrl: string;
  viewerType: string;
  defaultYaw?: number | null;
  defaultPitch?: number | null;
  defaultFov?: number | null;
  isRepresentative: boolean;
}

export type SeatViewTargetLevel = 'section' | 'row' | 'seat';
