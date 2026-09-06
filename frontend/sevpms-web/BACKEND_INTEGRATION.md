# Backend integration map

Development API: `http://localhost:5090`

The dev server uses `proxy.conf.json`, so Angular calls `/api/...` and `/hubs/...` without hard-coding a production hostname.

## Core API areas already wired

- Auth: `/api/auth/login`, `/register`, `/refresh`, `/logout`
- Current user: `/api/users/me`
- Events: `/api/events`, `/api/events/mine`
- Event categories: `/api/event-categories`
- Venues: `/api/venues`, `/api/venues/mine`
- Customer bookings: `/api/bookings`
- Payments: `/api/payments`, `/api/payments/{id}/payhere-checkout`
- Notifications: `/api/notifications`
- Seating: `/api/events/{eventId}/seating-layout/...`
- Seat availability: `/api/events/{eventId}/seats`
- Seat holds: `/api/events/{eventId}/seat-holds`
- Seat 360 view: `/api/events/{eventId}/seats/{seatId}/view`
- Tickets: `/api/bookings/{bookingId}/tickets`
- Check-in: `/api/events/{eventId}/check-ins/scan`
- Parking zones/recommendations/reservations: `/api/parking/...`
- Food: `/api/food/...`
- Places: `/api/places/...`
- Admin users/stats/audit: `/api/admin/...`
- Reports: `/api/reports/...`
- Venue rentals/marketplace: `/api/venue-rentals/...`, `/api/venues/{venueId}/marketplace/...`
- Realtime: `/hubs/notifications`, `/hubs/events`

## Important backend-boundary notes

1. The customer booking endpoint is customer-only. Organizer/admin cross-customer booking screens therefore remain honest capability shells until a backend-authorized list/report endpoint exists.
2. 360 seat views are shown only when the backend returns a real media asset.
3. QR values come from ticket backend responses; the frontend only renders/scans them.
4. Seat holds and seat status remain backend-authoritative.
5. Parking recommendation requires backend context; no client-side slot allocation is performed.
