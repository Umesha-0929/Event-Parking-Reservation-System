export type BackendRole = 'Customer' | 'EventOrganizer' | 'VenueOwner' | 'Admin';
export type BackendRoleValue = BackendRole | 0 | 1 | 2 | 3;

export interface ApiErrorShape {
  title?: string;
  detail?: string;
  message?: string;
  errors?: Record<string, string[]>;
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  expiresAtUtc?: string;
}

export interface AuthUser {
  id?: string;
  email: string;
  displayName?: string;
  firstName?: string;
  lastName?: string;
  role: BackendRole;
}

export interface LoginRequest { email: string; password: string; }
export interface RegisterRequest { firstName: string; lastName: string; email: string; password: string; phoneNumber?: string | null; role: 0 | 1 | 2; }
export interface AuthResponse extends Partial<AuthTokens> {
  token?: string;
  user?: AuthUser;
  role?: BackendRoleValue;
  userId?: string;
  email?: string;
  displayName?: string;
  firstName?: string;
  lastName?: string;
}

export interface EventSummary {
  id?: string;
  eventId?: string;
  organizerUserId?: string;
  categoryId?: string;
  category?: string;
  title?: string;
  name?: string;
  slug?: string;
  description?: string;
  categoryName?: string;
  venueName?: string;
  venueId?: string;
  city?: string;
  location?: string;
  startAtUtc?: string;
  startDateTime?: string;
  endAtUtc?: string;
  imageUrl?: string;
  status?: string;
  minimumPrice?: number;
}

export interface VenueSummary {
  id?: string;
  venueId?: string;
  ownerUserId?: string;
  name: string;
  description?: string;
  city?: string;
  address?: string;
  addressLine1?: string;
  addressLine2?: string | null;
  district?: string;
  country?: string;
  latitude?: number | null;
  longitude?: number | null;
  contactPhone?: string | null;
  contactEmail?: string | null;
  isActive?: boolean;
  capacity?: number;
  imageUrl?: string;
  status?: string;
  createdAtUtc?: string;
  updatedAtUtc?: string | null;
}

export interface UpsertVenueRequest {
  name: string;
  description: string;
  addressLine1: string;
  addressLine2?: string | null;
  city: string;
  district: string;
  country: string;
  latitude?: number | null;
  longitude?: number | null;
  capacity: number;
  contactPhone?: string | null;
  contactEmail?: string | null;
}

export interface BookingSummary {
  id?: string;
  bookingId?: string;
  bookingReference?: string;
  bookingNumber?: string;
  eventId?: string;
  eventName?: string;
  seatIds?: string[];
  totalAmount?: number;
  status?: string | number;
  createdAtUtc?: string;
}


export interface NotificationItem {
  id?: string;
  notificationId?: string;
  userId?: string;
  title?: string;
  message?: string;
  type?: string;
  isRead?: boolean;
  createdAtUtc?: string;
}

export interface SeatApiModel {
  id?: string;
  seatId?: string;
  row?: string;
  rowLabel?: string;
  number?: number;
  seatNumber?: string;
  category?: string;
  categoryName?: string;
  price?: number;
  status?: string;
  isAccessible?: boolean;
  has360?: boolean;
}

export interface SeatHoldResponse {
  holdToken: string;
  expiresAtUtc?: string;
  seats?: SeatApiModel[];
}

export interface TicketApiModel {
  id?: string;
  ticketId?: string;
  bookingId?: string;
  eventId?: string;
  seatId?: string | null;
  issuedAtUtc?: string;
  qrPayload?: string;
  ticketNo?: string;
  qrValue?: string;
  eventName?: string;
  venueName?: string;
  section?: string;
  row?: string;
  seat?: string;
  gate?: string;
  status?: string;
  checkedInAtUtc?: string;
}


export interface SeatSectionDto {
  id: string;
  seatingLayoutId: string;
  name: string;
  code: string;
  rowCount: number;
  columnCount: number;
  x: number;
  y: number;
  width: number;
  height: number;
  displayOrder: number;
  isAccessibleSection: boolean;
  isEnabled: boolean;
}

export interface SeatCategoryDto { id: string; name: string; code: string; price: number; displayOrder: number; isActive: boolean; }
export interface PublishedSeatDto {
  seatId: string; eventId: string; sectionId: string; seatCategoryId?: string | null; categoryName?: string | null; categoryCode?: string | null;
  price?: number | null; rowLabel: string; rowNumber: number; columnNumber: number; seatNumber: string; x: number; y: number; isAccessible: boolean; state: string; heldUntilUtc?: string | null;
}
export interface PublishedSeatingLayoutDto {
  layoutId: string; eventId: string; stageType: string | number; canvasWidth: number; canvasHeight: number;
  stageX: number; stageY: number; stageWidth: number; stageHeight: number;
  sections: SeatSectionDto[]; categories: SeatCategoryDto[]; seats: PublishedSeatDto[];
}
export interface SeatViewAssetDto {
  id: string; eventId: string; sectionId?: string | null; rowLabel?: string | null; seatId?: string | null;
  mediaUrl: string; viewerType: string; defaultYaw?: number | null; defaultPitch?: number | null; defaultFov?: number | null; isRepresentative: boolean;
}
export interface BackendSeatHoldDto { holdToken: string; eventId: string; seatIds: string[]; expiresAtUtc: string; }
export interface BackendSeatHoldResponse { succeeded: boolean; hold?: BackendSeatHoldDto | null; conflictingSeatIds: string[]; errorCode?: string | null; message?: string | null; }

export interface EventCategoryDto { eventCategoryId: string; name: string; code: string; isActive: boolean; }
export interface UpsertEventCategoryRequest { name: string; code: string; isActive: boolean; }

export interface SeatingLayoutDto {
  id: string; eventId: string; stageType: number; rowCount: number; columnCount: number; canvasWidth: number; canvasHeight: number;
  stageX: number; stageY: number; stageWidth: number; stageHeight: number; isPublished: boolean; publishedAtUtc?: string | null;
  sections: SeatSectionDto[]; categories: SeatCategoryDto[];
}

export interface PaymentResponseDto { paymentId:string; bookingId:string; customerUserId:string; amount:number; currency:string; provider:string; checkoutReference:string; qrPayload:string; status:number|string; paidAtUtc?:string|null; createdAtUtc:string; }
export interface PayHereCheckoutDto { paymentId:string; checkoutUrl:string; merchantId:string; orderId:string; amount:string; currency:string; hash:string; }

export interface UserProfileDto {
  userId?: string; id?: string; name?: string; firstName?: string; lastName?: string; email?: string; phoneNumber?: string | null; role?: BackendRoleValue; isActive?: boolean;
}

export interface AdminDashboardStatsDto {
  totalUsers?: number; totalEvents?: number; totalVenues?: number; totalBookings?: number; totalPayments?: number; totalRevenue?: number; activeEvents?: number;
  [key: string]: string | number | boolean | null | undefined;
}


export interface VenueFacilityDto {
  facilityId: string;
  name: string;
  category: string;
  isActive: boolean;
}

export interface VenueMediaDto {
  venueMediaId: string;
  url: string;
  type: string;
  sortOrder: number;
}

export interface VenueRateDto {
  venueRateId: string;
  rateType: string;
  amount: number;
  currency: string;
  validFromUtc?: string | null;
  validToUtc?: string | null;
}

export interface VenueAvailabilityDto {
  venueAvailabilityId: string;
  startAtUtc: string;
  endAtUtc: string;
  type: number | string;
  notes?: string | null;
}

export interface VenueLayoutTemplateDto {
  venueLayoutTemplateId: string;
  name: string;
  version: number;
  layoutJson: string;
  isActive: boolean;
}

export interface VenueMarketplaceDto {
  venueId: string;
  facilities: VenueFacilityDto[];
  media: VenueMediaDto[];
  rates: VenueRateDto[];
  availability: VenueAvailabilityDto[];
  layoutTemplates: VenueLayoutTemplateDto[];
}

export interface VenueRentalDto {
  rentalRequestId: string;
  organizerUserId: string;
  venueId: string;
  startAtUtc: string;
  endAtUtc: string;
  purpose: string;
  offeredAmount: number;
  status: number | string;
  ownerMessage?: string | null;
  createdAtUtc: string;
  updatedAtUtc?: string | null;
}

export interface ParkingZoneDto {
  id: string;
  venueId: string;
  eventId?: string | null;
  name: string;
  level: string;
  entranceName: string;
}

export interface ParkingSlotDto {
  id: string;
  parkingZoneId: string;
  eventId?: string | null;
  slotCode: string;
  x: number;
  y: number;
  isAccessible: boolean;
  status: 'Available' | 'Reserved' | 'Occupied' | 'Blocked' | string;
}

export interface UpsertParkingZoneRequest {
  venueId: string;
  eventId?: string | null;
  name: string;
  level: string;
  entranceName: string;
}

export interface UpsertParkingSlotRequest {
  parkingZoneId: string;
  eventId?: string | null;
  slotCode: string;
  x: number;
  y: number;
  isAccessible: boolean;
  status: string;
}


export interface SavedVehicleDto { id: string; userId?: string; nickname: string; registrationNo: string; vehicleType: string; isDefault: boolean; }
export interface ParkingReservationDto { id: string; bookingId: string; parkingSlotId: string; vehicleId?: string | null; vehicleRegSnapshot: string; status: string; reservedAtUtc: string; parkingPassCode?: string; }
export interface FoodStallDto { id: string; eventId: string; vendorId: string; stallName: string; isActive: boolean; opensAtUtc: string; closesAtUtc: string; }
export interface FoodMenuItemDto { id: string; eventFoodStallId: string; menuItemId: string; name: string; description: string; price: number; currency: string; isAvailable: boolean; imageUrl?: string; }
export interface FoodOrderItemDto { id?: string; menuItemId: string; itemNameSnapshot?: string; unitPrice?: number; quantity: number; lineTotal?: number; }
export interface FoodOrderDto { id: string; orderNo: string; customerUserId?: string; eventId: string; eventFoodStallId: string; bookingId?: string | null; status: string; fulfillmentType: string; seatLabelSnapshot?: string | null; total: number; createdAtUtc: string; items: FoodOrderItemDto[]; }
export interface NearbyPlaceDto { id: string; venueId: string; name: string; category: string; tags: string[]; audienceModes: string[]; address: string; distanceKm: number; latitude: number; longitude: number; isOpen: boolean; directionsUrl?: string | null; recommendationReason?: string; }
export interface AuditLogDto { id?: string; auditLogId?: string; actorUserId?: string | null; action?: string; entityType?: string; entityId?: string; beforeValue?: string | null; afterValue?: string | null; createdAtUtc?: string; ipAddress?: string | null; }
export interface EventReviewDto { id?: string; eventReviewId?: string; eventId?: string; rating: number; title?: string; comment?: string; createdAtUtc?: string; customerName?: string; }
export interface EventRatingSummaryDto { averageRating?: number; reviewCount?: number; }
export interface WaitlistEntryDto { id?: string; waitlistEntryId?: string; eventId?: string; status?: string; joinedAtUtc?: string; position?: number; }
