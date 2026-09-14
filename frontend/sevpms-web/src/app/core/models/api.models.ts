export type UserRole = 0 | 1 | 2 | 3;
export const USER_ROLE = { Customer: 0 as UserRole, EventOrganizer: 1 as UserRole, VenueOwner: 2 as UserRole, Admin: 3 as UserRole };
export const roleLabel = (role: UserRole | number | null | undefined) => ['Customer','Event Organizer','Venue Owner','Admin'][Number(role ?? 0)] ?? 'User';

export interface AuthUser { userId:string; firstName:string; lastName:string; email:string; role:UserRole; tokenType:string; accessToken:string; accessTokenExpiresAtUtc:string; refreshTokenExpiresAtUtc:string; }
export interface LoginRequest { email:string; password:string; }
export interface RegisterRequest { firstName:string; lastName:string; email:string; password:string; phoneNumber?:string|null; role:UserRole; }
export interface RegistrationPending { email:string; emailVerificationRequired:boolean; otpExpiresAtUtc:string; message:string; }
export interface UserProfile { userId:string; email:string; name:string; phoneNumber?:string|null; role:UserRole; }
export interface UpdateProfileRequest { firstName:string; lastName:string; phoneNumber?:string|null; }
export interface ChangePasswordRequest { currentPassword:string; newPassword:string; }

export interface EventDto { eventId:string; organizerUserId:string; venueId:string; categoryId:string; category:string; title:string; description:string; startAtUtc:string; endAtUtc:string; status:number; createdAtUtc:string; updatedAtUtc?:string|null; }
export interface EventCategory { eventCategoryId:string; name:string; code:string; isActive:boolean; }
export interface VenueDto { venueId:string; ownerUserId:string; name:string; description:string; addressLine1:string; addressLine2?:string|null; city:string; district:string; country:string; latitude?:number|null; longitude?:number|null; capacity:number; contactPhone?:string|null; contactEmail?:string|null; isActive:boolean; createdAtUtc:string; updatedAtUtc?:string|null; }
export interface VenueFacility { facilityId:string; name:string; category:string; isActive:boolean; }
export interface VenueMedia { venueMediaId:string; url:string; type:string; sortOrder:number; }
export interface VenueRate { venueRateId:string; rateType:string; amount:number; currency:string; validFromUtc?:string|null; validToUtc?:string|null; }
export interface VenueAvailability { venueAvailabilityId:string; startAtUtc:string; endAtUtc:string; type:number|string; notes?:string|null; }
export interface VenueLayoutTemplate { venueLayoutTemplateId:string; name:string; version:number; layoutJson:string; isActive:boolean; }
export interface VenueMarketplace { venueId:string; facilities:VenueFacility[]; media:VenueMedia[]; rates:VenueRate[]; availability:VenueAvailability[]; layoutTemplates:VenueLayoutTemplate[]; }

export interface BookingDto { bookingId:string; bookingNumber:string; customerUserId:string; eventId:string; seatIds:string[]; totalAmount:number; status:number; createdAtUtc:string; confirmedAtUtc?:string|null; cancelledAtUtc?:string|null; }
export interface TicketDto { ticketId:string; ticketNo:string; bookingId:string; bookingNumber?:string; eventId:string; eventName?:string; venueName?:string; seatId?:string|null; rowLabel?:string|null; seatNumber?:string|null; status:number|string; issuedAtUtc?:string; checkedInAtUtc?:string|null; isAccessible?:boolean; qrPayload?:string; }
export interface PaymentDto { paymentId:string; bookingId:string; customerUserId:string; amount:number; currency:string; provider:string; checkoutReference:string; qrPayload:string; status:number|string; paidAtUtc?:string|null; createdAtUtc:string; }
export interface ManualPaymentReviewDto { paymentId:string; bookingId:string; eventId:string; amount:number; currency:string; proofUrl:string; submittedAtUtc:string; }
export interface ReceiptDto { receiptId:string; receiptNumber:string; paymentId:string; bookingId:string; customerUserId:string; amount:number; currency:string; issuedAtUtc:string; }

export interface ReceiptDeliveryDto { receiptDeliveryId:string; receiptId:string; channel:string; destinationMasked:string; status:number|string; attemptCount:number; lastAttemptAtUtc?:string|null; sentAtUtc?:string|null; lastError?:string|null; }
export interface BookingCalendarDto { bookingId:string; eventId:string; eventTitle:string; venueName:string; location:string; startAtUtc:string; endAtUtc:string; googleCalendarUrl:string; icsDownloadPath:string; }

export interface EventWeatherDto { eventId:string; eventTitle:string; venueId:string; venueName:string; eventStartUtc:string; location:string; available:boolean; message?:string|null; weatherCode?:number|null; condition?:string|null; minimumTemperatureC?:number|null; maximumTemperatureC?:number|null; precipitationProbabilityPercent?:number|null; precipitationMm?:number|null; maximumWindSpeedKmh?:number|null; warning?:string|null; }
export interface EventReviewDto { id:string; eventId:string; customerUserId:string; bookingId:string; rating:number; comment?:string|null; createdAtUtc:string; }
export interface EventRatingSummaryDto { eventId:string; reviewCount:number; averageRating:number; }
export interface WaitlistEntryDto { id:string; eventId:string; customerUserId:string; status:number|string; position?:number|null; joinedAtUtc:string; eligibleAtUtc?:string|null; }

export interface SeatSection { id:string; seatingLayoutId:string; name:string; code:string; rowCount:number; columnCount:number; x:number; y:number; width:number; height:number; displayOrder:number; isAccessibleSection:boolean; isEnabled:boolean; }
export interface SeatCategory { id:string; name:string; code:string; price:number; displayOrder:number; isActive:boolean; }
export interface PublishedSeat { seatId:string; eventId:string; sectionId:string; seatCategoryId?:string|null; categoryName?:string|null; categoryCode?:string|null; price?:number|null; rowLabel:string; rowNumber:number; columnNumber:number; seatNumber:string; x:number; y:number; isAccessible:boolean; state:string; heldUntilUtc?:string|null; }
export interface PublishedSeatingLayout { layoutId:string; eventId:string; stageType:number; canvasWidth:number; canvasHeight:number; stageX:number; stageY:number; stageWidth:number; stageHeight:number; sections:SeatSection[]; categories:SeatCategory[]; seats:PublishedSeat[]; }
export interface SeatHold { holdToken:string; eventId:string; seatIds:string[]; expiresAtUtc:string; }
export interface SeatHoldResponse { succeeded:boolean; hold?:SeatHold|null; conflictingSeatIds:string[]; errorCode?:string|null; message?:string|null; }
export interface SeatViewAsset { id:string; eventId:string; sectionId?:string|null; rowLabel?:string|null; seatId?:string|null; mediaUrl:string; viewerType:string; defaultYaw?:number|null; defaultPitch?:number|null; defaultFov?:number|null; isRepresentative:boolean; }

export interface ParkingZone { id:string; venueId:string; eventId?:string|null; name:string; level:string; entranceName:string; }
export interface ParkingSlot { id:string; parkingZoneId:string; eventId?:string|null; slotCode:string; x:number; y:number; isAccessible:boolean; status:string; }
export interface ParkingReservation { id:string; bookingId:string; parkingSlotId:string; vehicleId?:string|null; vehicleRegSnapshot:string; status:string; reservedAtUtc:string; }

export interface ParkingNodeDto { id:string; venueId:string; layoutId?:string|null; nodeCode:string; x:number; y:number; nodeType:string; }
export interface ParkingRecommendationDto { parkingSlotId:string; parkingZoneId:string; slotCode:string; distanceCost:number; isAccessible:boolean; reason:string; }
export interface ParkingRouteDto { startNodeId:string; endNodeId:string; totalCost:number; nodes:ParkingNodeDto[]; }
export interface VehicleDto { id:string; userId:string; nickname:string; registrationNo:string; vehicleType:string; isDefault:boolean; }

export interface FoodStall { id:string; eventId:string; vendorId:string; hallLayoutElementId?:string|null; stallName:string; isActive:boolean; opensAtUtc:string; closesAtUtc:string; }
export interface FoodVendor { id:string; ownerUserId?:string|null; name:string; description:string; status:string; }
export interface MenuItem { id:string; eventFoodStallId:string; menuItemId:string; name:string; description:string; price:number; currency:string; isAvailable:boolean; imageUrl:string; }
export interface FoodOrder { id:string; orderNo:string; customerUserId:string; eventId:string; eventFoodStallId:string; bookingId?:string|null; status:string; fulfillmentType:string; seatLabelSnapshot?:string|null; total:number; createdAtUtc:string; items:FoodOrderItem[]; }
export interface FoodOrderItem { id:string; menuItemId:string; itemNameSnapshot:string; unitPrice:number; quantity:number; lineTotal:number; }

export interface FoodOrderStatusHistoryDto { id:string; foodOrderId:string; oldStatus:string; newStatus:string; changedByUserId:string; changedAtUtc:string; note:string; }

export interface NearbyPlace { id:string; venueId:string; name:string; category:string; tags:string[]; audienceModes:string[]; address:string; distanceKm:number; latitude:number; longitude:number; isOpen:boolean; directionsUrl?:string|null; recommendationReason:string; }
export interface EventRecommendationDto { eventId:string; venueId:string; title:string; description:string; category:string; startAtUtc:string; endAtUtc:string; recommendationScore:number; reasons:string[]; }
export interface Recommendation<T=EventDto> { score?:number; reason?:string; item?:T; event?:T; }
export interface NotificationDto { notificationId:string; userId?:string; title:string; message:string; type?:string|number; category?:string; isRead:boolean; createdAtUtc:string; relatedEntityId?:string|null; actionUrl?:string|null; }

export interface PlatformReportDto { fromUtc:string; toUtc:string; users:number; events:number; publishedEvents:number; venues:number; bookings:number; confirmedBookings:number; successfulPayments:number; refunds:number; attendance:number; parkingReservations:number; foodOrders:number; grossRevenue:number; refundedAmount:number; netRevenue:number; foodRevenue:number; }
export interface OrganizerReportDto { organizerUserId:string; events:number; publishedEvents:number; confirmedBookings:number; attendance:number; parkingReservations:number; foodOrders:number; revenue:number; foodRevenue:number; }
export interface VenueOwnerReportDto { venueOwnerUserId:string; venues:number; rentalRequests:number; acceptedRentals:number; acceptedRentalValue:number; }

export interface AdminDashboardStats { totalUsers:number; activeUsers:number; suspendedUsers:number; totalVenues:number; activeVenues:number; totalEvents:number; publishedEvents:number; pendingBookings:number; confirmedBookings:number; successfulPayments:number; successfulRevenue:number; generatedAtUtc:string; }
export interface AdminUser { userId:string; firstName:string; lastName:string; email:string; phoneNumber?:string|null; role:UserRole; status:number; createdAtUtc:string; lastLoginAtUtc?:string|null; }


export interface CreateBookingRequest { eventId:string; holdToken:string; seatIds:string[]; }
export interface StartPaymentRequest { bookingId:string; }
export interface EventUpsertRequest { venueId:string; categoryId:string; category:string; title:string; description:string; startAtUtc:string; endAtUtc:string; }
export interface ConfigureSeatingLayoutRequest { stageType:number; rowCount:number; columnCount:number; canvasWidth:number; canvasHeight:number; stageX:number; stageY:number; stageWidth:number; stageHeight:number; }
export interface UpsertSeatSectionRequest { id?:string|null; name:string; code:string; rowCount:number; columnCount:number; x:number; y:number; width:number; height:number; displayOrder:number; isAccessibleSection:boolean; isEnabled:boolean; }
export interface UpsertSeatCategoryRequest { id?:string|null; name:string; code:string; price:number; displayOrder:number; isActive:boolean; }
export interface SeatPositionRequest { rowNumber:number; columnNumber:number; }
export interface LayoutGapRequest { rowNumber:number; startColumn:number; columnSpan:number; }
export interface GenerateSeatsRequest { sectionId:string; seatCategoryId?:string|null; rowCount:number; columnCount:number; startingRowLabel:string; startingSeatNumber:number; startX:number; startY:number; horizontalSpacing:number; verticalSpacing:number; unavailablePositions:SeatPositionRequest[]; accessiblePositions:SeatPositionRequest[]; gaps:LayoutGapRequest[]; }
export interface UpsertSeatViewAssetRequest { id?:string|null; sectionId?:string|null; rowLabel?:string|null; seatId?:string|null; mediaUrl:string; viewerType:string; defaultYaw?:number|null; defaultPitch?:number|null; defaultFov?:number|null; isRepresentative:boolean; }
export interface CheckInTicketResponse { succeeded:boolean; result:string; message:string; ticketId?:string|null; ticketNo?:string|null; scannedAtUtc?:string|null; }

export interface UpsertParkingZoneRequest { venueId:string; eventId?:string|null; name:string; level:string; entranceName:string; }
export interface UpsertParkingSlotRequest { parkingZoneId:string; eventId?:string|null; slotCode:string; x:number; y:number; isAccessible:boolean; status?:string; }
export interface BulkCreateParkingSlotsRequest { slots:UpsertParkingSlotRequest[]; }
export interface ParkingRecommendationRequest { venueId:string; eventId?:string|null; entranceNodeId:string; requiresAccessibleParking:boolean; savedVehicleId?:string|null; }
export interface CreateParkingReservationRequest { bookingId:string; parkingSlotId:string; vehicleId?:string|null; vehicleRegistration:string; }
export interface CreateVehicleRequest { nickname:string; registrationNo:string; vehicleType:string; isDefault:boolean; }
export type UpdateVehicleRequest = CreateVehicleRequest;

export interface CreateFoodOrderItemRequest { menuItemId:string; quantity:number; }
export interface CreateFoodOrderRequest { eventId:string; eventFoodStallId:string; bookingId?:string|null; fulfillmentType:string; seatLabelSnapshot?:string|null; items:CreateFoodOrderItemRequest[]; }
export interface UpdateFoodOrderStatusRequest { newStatus:string; note:string; }

export interface VenueUpsertRequest { name:string; description:string; addressLine1:string; addressLine2?:string|null; city:string; district:string; country:string; latitude?:number|null; longitude?:number|null; capacity:number; contactPhone?:string|null; contactEmail?:string|null; }
export interface CreateVenueRentalRequest { venueId:string; startAtUtc:string; endAtUtc:string; purpose:string; offeredAmount:number; }
export interface UpdateVenueRentalStatusRequest { status:number; ownerMessage?:string|null; }
export interface AddVenueMediaRequest { url:string; type:string; sortOrder:number; }
export interface AddVenueRateRequest { rateType:string; amount:number; currency:string; validFromUtc?:string|null; validToUtc?:string|null; }
export interface AddVenueAvailabilityRequest { startAtUtc:string; endAtUtc:string; type:number; notes?:string|null; }
export interface AddVenueLayoutTemplateRequest { name:string; version:number; layoutJson:string; }

export interface UpsertEventCategoryRequest { name:string; code:string; isActive:boolean; }
export interface UpsertFacilityRequest { name:string; category:string; isActive:boolean; }
export interface ManageNearbyPlaceRequest { venueId:string; name:string; category:string; tags:string[]; audienceModes:string[]; address:string; distanceKm:number; latitude:number; longitude:number; isOpen:boolean; directionsUrl?:string|null; isActive:boolean; }

export interface ApiErrorShape { title?:string; message?:string; error?:string; errors?:Record<string,string[]>; status?:number; }

// Additional typed backend contracts used by management and integration screens.
export interface VenueRentalDto { rentalRequestId:string; organizerUserId:string; venueId:string; startAtUtc:string; endAtUtc:string; purpose:string; offeredAmount:number; status:number; ownerMessage?:string|null; createdAtUtc:string; updatedAtUtc?:string|null; }
export interface SeatingLayoutDto { id:string; eventId:string; stageType:number; rowCount:number; columnCount:number; canvasWidth:number; canvasHeight:number; stageX:number; stageY:number; stageWidth:number; stageHeight:number; isPublished:boolean; publishedAtUtc?:string|null; sections:SeatSection[]; categories:SeatCategory[]; }
export interface SeatAvailabilityDto { seatId:string; eventId:string; sectionId:string; rowLabel:string; seatNumber:string; x:number; y:number; ticketTypeId?:string|null; isAccessible:boolean; state:string; heldUntilUtc?:string|null; }
export interface AuditLogDto { auditLogId:string; actorUserId?:string|null; action:string; entityType:string; entityId?:string|null; beforeSummary?:string|null; afterSummary?:string|null; correlationId?:string|null; ipAddress?:string|null; createdAtUtc:string; }
export interface PayHereCheckoutDto { paymentId:string; checkoutUrl:string; merchantId:string; returnUrl:string; cancelUrl:string; notifyUrl:string; firstName:string; lastName:string; email:string; phone:string; address:string; city:string; country:string; orderId:string; items:string; amount:string; currency:string; hash:string; }
export interface RefundDto { refundId:string; paymentId:string; bookingId:string; amount:number; currency:string; reason:string; refundReference:string; status:number|string; refundedAtUtc?:string|null; }
export interface MediaUploadDto { url:string; type:string; fileName:string; }
export interface MediaUrlDto { url:string; userId?:string; }
