import { Routes } from '@angular/router';
import { USER_ROLE } from './core/models/api.models';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'app/home' },

  {
    path: 'auth',
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'sign-in' },
      { path: 'sign-in', loadComponent: () => import('./features/auth/auth-page.component').then(m => m.AuthPageComponent), data: { mode: 'sign-in' } },
      { path: 'sign-up', loadComponent: () => import('./features/auth/auth-page.component').then(m => m.AuthPageComponent), data: { mode: 'sign-up' } },
      { path: 'verify-email', loadComponent: () => import('./features/auth/auth-page.component').then(m => m.AuthPageComponent), data: { mode: 'verify' } },
      { path: 'forgot-password', loadComponent: () => import('./features/auth/auth-page.component').then(m => m.AuthPageComponent), data: { mode: 'forgot' } },
      { path: 'reset-password', loadComponent: () => import('./features/auth/auth-page.component').then(m => m.AuthPageComponent), data: { mode: 'reset' } }
    ]
  },

  {
    path: 'app',
    loadComponent: () => import('./layout/workspace-layout.component').then(m => m.WorkspaceLayoutComponent),
    data: { workspace: 'customer' },
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'home' },
      { path: 'home', loadComponent: () => import('./features/customer/home-page.component').then(m => m.HomePageComponent) },
      { path: 'events', loadComponent: () => import('./features/customer/events-page.component').then(m => m.EventsPageComponent) },
      { path: 'events/:id', loadComponent: () => import('./features/customer/event-detail-page.component').then(m => m.EventDetailPageComponent) },
      { path: 'events/:id/seats', canActivate: [authGuard, roleGuard([USER_ROLE.Customer])], loadComponent: () => import('./features/customer/seat-selection-page.component').then(m => m.SeatSelectionPageComponent) },
      { path: 'venues', loadComponent: () => import('./features/customer/venues-page.component').then(m => m.VenuesPageComponent) },
      { path: 'venues/:id', loadComponent: () => import('./features/customer/venue-detail-page.component').then(m => m.VenueDetailPageComponent) },
      { path: 'parking', canActivate: [authGuard, roleGuard([USER_ROLE.Customer])], loadComponent: () => import('./features/customer/parking-page.component').then(m => m.ParkingPageComponent) },
      { path: 'food', canActivate: [authGuard, roleGuard([USER_ROLE.Customer])], loadComponent: () => import('./features/customer/food-page.component').then(m => m.FoodPageComponent) },
      { path: 'places', loadComponent: () => import('./features/customer/places-page.component').then(m => m.PlacesPageComponent) },
      { path: 'bookings', canActivate: [authGuard, roleGuard([USER_ROLE.Customer])], loadComponent: () => import('./features/customer/bookings-page.component').then(m => m.BookingsPageComponent) },
      { path: 'tickets', canActivate: [authGuard, roleGuard([USER_ROLE.Customer])], loadComponent: () => import('./features/customer/tickets-page.component').then(m => m.TicketsPageComponent) },
      { path: 'payments/:bookingId', canActivate: [authGuard, roleGuard([USER_ROLE.Customer])], loadComponent: () => import('./features/customer/payment-page.component').then(m => m.PaymentPageComponent) },
      { path: 'payment/result', canActivate: [authGuard, roleGuard([USER_ROLE.Customer])], loadComponent: () => import('./features/customer/payment-result-page.component').then(m => m.PaymentResultPageComponent) },
      { path: 'notifications', canActivate: [authGuard, roleGuard([USER_ROLE.Customer])], loadComponent: () => import('./features/customer/notifications-page.component').then(m => m.NotificationsPageComponent) },
      { path: 'profile', canActivate: [authGuard, roleGuard([USER_ROLE.Customer])], loadComponent: () => import('./features/customer/profile-page.component').then(m => m.ProfilePageComponent) }
    ]
  },

  {
    path: 'organizer',
    canActivate: [authGuard, roleGuard([USER_ROLE.EventOrganizer])],
    loadComponent: () => import('./layout/workspace-layout.component').then(m => m.WorkspaceLayoutComponent),
    data: { workspace: 'organizer' },
    children: [
      { path: '', loadComponent: () => import('./features/organizer/organizer-dashboard.component').then(m => m.OrganizerDashboardComponent) },
      { path: 'events', loadComponent: () => import('./features/organizer/organizer-events.component').then(m => m.OrganizerEventsComponent) },
      { path: 'host-event', loadComponent: () => import('./features/organizer/organizer-event-editor.component').then(m => m.OrganizerEventEditorComponent) },
      { path: 'events/:id/edit', loadComponent: () => import('./features/organizer/organizer-event-editor.component').then(m => m.OrganizerEventEditorComponent) },
      { path: 'seating', loadComponent: () => import('./features/organizer/organizer-seating.component').then(m => m.OrganizerSeatingComponent) },
      { path: 'bookings', loadComponent: () => import('./features/organizer/organizer-bookings.component').then(m => m.OrganizerBookingsComponent) },
      { path: 'check-in', loadComponent: () => import('./features/organizer/organizer-checkin.component').then(m => m.OrganizerCheckinComponent) },
      { path: 'orders', loadComponent: () => import('./features/organizer/organizer-orders.component').then(m => m.OrganizerOrdersComponent) },
      { path: 'analytics', loadComponent: () => import('./features/organizer/organizer-analytics.component').then(m => m.OrganizerAnalyticsComponent) },
      { path: 'notifications', loadComponent: () => import('./features/customer/notifications-page.component').then(m => m.NotificationsPageComponent) },
      { path: 'profile', loadComponent: () => import('./features/customer/profile-page.component').then(m => m.ProfilePageComponent) }
    ]
  },

  {
    path: 'venue-owner',
    canActivate: [authGuard, roleGuard([USER_ROLE.VenueOwner])],
    loadComponent: () => import('./layout/workspace-layout.component').then(m => m.WorkspaceLayoutComponent),
    data: { workspace: 'venue-owner' },
    children: [
      { path: '', loadComponent: () => import('./features/venue-owner/venue-owner-dashboard.component').then(m => m.VenueOwnerDashboardComponent) },
      { path: 'venues', loadComponent: () => import('./features/venue-owner/venue-owner-venues.component').then(m => m.VenueOwnerVenuesComponent) },
      { path: 'add-venue', loadComponent: () => import('./features/venue-owner/venue-owner-editor.component').then(m => m.VenueOwnerEditorComponent) },
      { path: 'venues/:id/edit', loadComponent: () => import('./features/venue-owner/venue-owner-editor.component').then(m => m.VenueOwnerEditorComponent) },
      { path: 'rentals', loadComponent: () => import('./features/venue-owner/venue-owner-rentals.component').then(m => m.VenueOwnerRentalsComponent) },
      { path: 'parking', loadComponent: () => import('./features/venue-owner/venue-owner-parking.component').then(m => m.VenueOwnerParkingComponent) },
      { path: 'payments', loadComponent: () => import('./features/venue-owner/venue-owner-payments.component').then(m => m.VenueOwnerPaymentsComponent) },
      { path: 'reports', loadComponent: () => import('./features/venue-owner/venue-owner-reports.component').then(m => m.VenueOwnerReportsComponent) },
      { path: 'notifications', loadComponent: () => import('./features/customer/notifications-page.component').then(m => m.NotificationsPageComponent) },
      { path: 'profile', loadComponent: () => import('./features/customer/profile-page.component').then(m => m.ProfilePageComponent) }
    ]
  },

  {
    path: 'admin',
    canActivate: [authGuard, roleGuard([USER_ROLE.Admin])],
    loadComponent: () => import('./layout/workspace-layout.component').then(m => m.WorkspaceLayoutComponent),
    data: { workspace: 'admin' },
    children: [
      { path: '', loadComponent: () => import('./features/admin/admin-dashboard.component').then(m => m.AdminDashboardComponent) },
      { path: 'users', loadComponent: () => import('./features/admin/admin-users.component').then(m => m.AdminUsersComponent) },
      { path: 'events', loadComponent: () => import('./features/admin/admin-resource-page.component').then(m => m.AdminResourcePageComponent), data: { mode: 'events' } },
      { path: 'venues', loadComponent: () => import('./features/admin/admin-resource-page.component').then(m => m.AdminResourcePageComponent), data: { mode: 'venues' } },
      { path: 'bookings', loadComponent: () => import('./features/admin/admin-resource-page.component').then(m => m.AdminResourcePageComponent), data: { mode: 'bookings' } },
      { path: 'parking', loadComponent: () => import('./features/admin/admin-parking.component').then(m => m.AdminParkingComponent) },
      { path: 'places', loadComponent: () => import('./features/admin/admin-places.component').then(m => m.AdminPlacesComponent) },
      { path: 'food', loadComponent: () => import('./features/admin/admin-resource-page.component').then(m => m.AdminResourcePageComponent), data: { mode: 'food' } },
      { path: 'payments', loadComponent: () => import('./features/admin/admin-payments.component').then(m => m.AdminPaymentsComponent) },
      { path: 'notifications', loadComponent: () => import('./features/customer/notifications-page.component').then(m => m.NotificationsPageComponent) },
      { path: 'audit-logs', loadComponent: () => import('./features/admin/admin-audit-logs.component').then(m => m.AdminAuditLogsComponent) },
      { path: 'reports', loadComponent: () => import('./features/admin/admin-reports.component').then(m => m.AdminReportsComponent) },
      { path: 'catalog', loadComponent: () => import('./features/admin/admin-catalog.component').then(m => m.AdminCatalogComponent) },
      { path: 'profile', loadComponent: () => import('./features/customer/profile-page.component').then(m => m.ProfilePageComponent) }
    ]
  },

  // Backward-compatible alias for password-reset emails generated by older builds.
  { path: 'password-reset', loadComponent: () => import('./features/auth/auth-page.component').then(m => m.AuthPageComponent), data: { mode: 'reset' } },
  { path: 'access-denied', loadComponent: () => import('./features/system/access-denied.component').then(m => m.AccessDeniedComponent) },
  { path: '**', loadComponent: () => import('./features/system/not-found.component').then(m => m.NotFoundComponent) }
];
