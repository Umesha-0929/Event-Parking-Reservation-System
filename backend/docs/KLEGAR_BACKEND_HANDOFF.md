# SEVPMS — Klegar Seat + Ticket Backend Handoff

## Owner

Klegar

## Scope Completed

### 2D Stage + Seating Layout

- Stage type selection
- Arena Stage
- Proscenium Theatre Stage
- End-On Stage
- Thrust Stage
- Traverse Stage
- In-the-Round Stage
- Stage-linked seating layout
- Rows and columns
- Seat sections
- Seat categories
- VIP / Premium / Standard capable category model
- Seat pricing
- Seat generation
- Seat numbering
- 2D X/Y seat positioning
- Accessible seats
- Unavailable seats
- Aisles / gaps
- Organizer preview
- Publish / unpublish layout
- Customer published-layout retrieval
- Customer seat category and price response

### Seat Availability and Reservation

- Available seats
- Held seats
- Booked seats
- Unavailable seats
- Temporary seat holds
- Hold expiry
- Hold release
- Double-booking protection
- Concurrency-safe persistence
- Booking conversion
- Realtime seat-state notifications

### 360 Degree Seat View

360 seat-view support remains included.

Supported mapping levels:

1. Individual seat
2. Row
3. Section

Lookup priority:

1. Direct seat view
2. Seat-mapped panorama
3. Row panorama
4. Section panorama

Nearby seats may share representative row or section panoramas.

### Ticket and QR

- Ticket generation
- Booking-linked tickets
- Seat-linked tickets
- Secure QR payload generation
- QR hash persistence
- Ticket validation
- Event check-in
- Duplicate scan protection
- Check-in evidence records
- Ticket cancellation support

## Main Organizer 2D Layout APIs

- GET /api/events/{eventId}/seating-layout/organizer
- PUT /api/events/{eventId}/seating-layout
- PUT /api/events/{eventId}/seating-layout/sections
- PUT /api/events/{eventId}/seating-layout/categories
- POST /api/events/{eventId}/seating-layout/generate-seats
- PUT /api/events/{eventId}/seating-layout/publish

## Customer Layout API

- GET /api/events/{eventId}/seating-layout/published

Customers receive only the published organizer layout.

## Seat APIs

- GET /api/events/{eventId}/seats
- PUT /api/events/{eventId}/seats
- POST /api/events/{eventId}/seat-holds
- DELETE /api/seat-holds/{holdToken}
- POST /api/seat-holds/{holdToken}/commit

## 360 View APIs

- GET /api/events/{eventId}/seats/{seatId}/view
- PUT /api/events/{eventId}/seat-view-assets

## Ticket APIs

- POST /api/bookings/{bookingId}/tickets/issue
- GET /api/bookings/{bookingId}/tickets
- GET /api/tickets/{ticketNo}
- POST /api/tickets/{ticketNo}/cancel

## Check-In API

- POST /api/events/{eventId}/check-ins/scan

## Booking / Payment Integration Boundary

Klegar does not replace the shared Booking or Payment modules.

After server-side payment verification, the shared booking/payment flow can call:

ISeatTicketFulfillmentService.CompletePaidBookingAsync(...)

This service:

1. Converts the customer's valid seat hold
2. Marks the held seat(s) as booked
3. Issues booking-linked ticket(s)
4. Generates QR ticket payloads

Authentication, JWT, core booking, payment and shared SignalR server infrastructure remain owned by the shared backend modules.

## Database Migrations

Klegar backend migrations include:

- KlegarSeatTicketBackend
- Klegar2DSeatingLayout

## Automated Verification

Coverage includes:

- Seat availability
- Active seat-hold conflict
- Hold expiration
- Seat service behaviour
- Ticket issue behaviour
- QR check-in
- Duplicate QR scan
- 360 seat -> row -> section fallback
- 2D stage/layout configuration
- Seat generation
- Accessible seat generation
- Unavailable seat generation
- Aisle/gap generation
- Published-layout rules
- Paid booking -> seat -> ticket -> QR flow

## Runtime Verification

Use:

docs/KLEGAR_RUNTIME_VERIFICATION.http

The file contains the manual end-to-end API verification sequence.

## Final Validation Commands

From the backend folder:

dotnet restore SEVPMS.sln
dotnet build SEVPMS.sln --no-restore
dotnet test SEVPMS.sln --no-build

Database:

dotnet ef database update --project src/SEVPMS.Infrastructure --startup-project src/SEVPMS.Api

Migration verification:

dotnet ef migrations list --project src/SEVPMS.Infrastructure --startup-project src/SEVPMS.Api

## Current Feature Baseline

Both features are required and retained:

- 2D seating layout
- 360 degree seat-view preview
