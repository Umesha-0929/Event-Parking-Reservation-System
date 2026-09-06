import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { NavItem, UserRole } from '../../core/models/nvent.models';
import { AuthService } from '../../core/services/auth.service';
import { NotificationCenterService } from '../../core/services/notification-center.service';
import { SessionService } from '../../core/services/session.service';

@Component({
  selector: 'app-workspace-layout',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './workspace-layout.html',
  styleUrl: './workspace-layout.scss',
})
export class WorkspaceLayoutComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  readonly session = inject(SessionService);
  readonly notifications = inject(NotificationCenterService);
  readonly mobileOpen = signal(false);
  readonly role = (this.route.snapshot.data['role'] ?? 'customer') as UserRole;
  readonly roleLabel = computed(() => this.role === 'venue-owner' ? 'Venue Owner' : this.role.charAt(0).toUpperCase() + this.role.slice(1));

  readonly menus: Record<'customer' | 'organizer' | 'venue-owner' | 'admin', NavItem[]> = {
    customer: [
      { label: 'Dashboard', path: '/customer/dashboard', icon: '⌂' }, { label: 'Browse Events', path: '/customer/browse-events', icon: '◇' },
      { label: 'Browse Venues', path: '/customer/browse-venues', icon: '▦' }, { label: 'My Bookings', path: '/customer/bookings', icon: '✓' },
      { label: 'My Tickets', path: '/customer/tickets', icon: '▤' }, { label: 'Parking', path: '/customer/parking', icon: 'P' },
      { label: 'Food', path: '/customer/food', icon: '◌' }, { label: 'Place Finder', path: '/customer/places', icon: '⌖' },
      { label: 'Notifications', path: '/customer/notifications', icon: '◉' }, { label: 'Profile', path: '/customer/profile', icon: '○' },
      { label: 'Settings', path: '/customer/settings', icon: '⚙' },
    ],
    organizer: [
      { label: 'Dashboard', path: '/organizer/dashboard', icon: '⌂' }, { label: 'Events', path: '/organizer/events', icon: '◇' },
      { label: 'Create Event', path: '/organizer/create-event', icon: '+' }, { label: 'Venues', path: '/organizer/venues', icon: '▦' },
      { label: 'Seating Layout', path: '/organizer/seating', icon: '▥' }, { label: 'Seat Categories', path: '/organizer/categories', icon: '◫' },
      { label: 'Analytics & Finance', path: '/organizer/reports', icon: '⌁' },
      { label: 'Notifications', path: '/organizer/notifications', icon: '◉' }, { label: 'Settings', path: '/organizer/settings', icon: '⚙' },
    ],
    'venue-owner': [
      { label: 'Dashboard', path: '/venue-owner/dashboard', icon: '⌂' }, { label: 'My Venues', path: '/venue-owner/venues', icon: '▦' },
      { label: 'Register Venue', path: '/venue-owner/venues/new', icon: '+' }, { label: 'Marketplace', path: '/venue-owner/marketplace', icon: '◇' },
      { label: 'Rentals', path: '/venue-owner/rentals', icon: '✓' }, { label: 'Availability', path: '/venue-owner/availability', icon: '◷' },
      { label: 'Parking', path: '/venue-owner/parking', icon: 'P' }, { label: 'Reports', path: '/venue-owner/reports', icon: '⌁' },
      { label: 'Notifications', path: '/venue-owner/notifications', icon: '◉' }, { label: 'Settings', path: '/venue-owner/settings', icon: '⚙' },
    ],
    admin: [
      { label: 'Dashboard', path: '/admin/dashboard', icon: '⌂' }, { label: 'Events', path: '/admin/events', icon: '◇' },
      { label: 'Event Categories', path: '/admin/event-categories', icon: '☷' }, { label: 'Venue Facilities', path: '/admin/venue-facilities', icon: '◫' },
      { label: 'Venues', path: '/admin/venues', icon: '▦' }, { label: 'Nearby Places', path: '/admin/nearby-places', icon: '⌖' },
      { label: 'Users', path: '/admin/users', icon: '○' }, { label: 'QR Check-in', path: '/admin/qr-checkin', icon: '⌗' },
      { label: 'Reports', path: '/admin/reports', icon: '⌁' }, { label: 'Audit Logs', path: '/admin/audit-logs', icon: '☷' },
      { label: 'Operations', path: '/admin/operations', icon: '◉' },
      { label: 'Notifications', path: '/admin/notifications', icon: '◉' }, { label: 'Settings', path: '/admin/settings', icon: '⚙' },
    ],
  };

  get menu(): NavItem[] { return this.menus[this.role as 'customer' | 'organizer' | 'venue-owner' | 'admin']; }
  get searchPath(): string {
    if (this.role === 'organizer') return '/organizer/events';
    if (this.role === 'venue-owner') return '/venue-owner/venues';
    if (this.role === 'admin') return '/admin/events';
    return '/customer/browse-events';
  }
  get accountPath(): string {
    if (this.role === 'organizer') return '/organizer/settings';
    if (this.role === 'venue-owner') return '/venue-owner/settings';
    if (this.role === 'admin') return '/admin/settings';
    return '/customer/profile';
  }
  get isFocusedFlow(): boolean { return this.router.url.includes('/customer/seats') || this.router.url.includes('/customer/payment'); }
  toggle(): void { this.mobileOpen.update((value) => !value); }
  close(): void { this.mobileOpen.set(false); }
  logout(): void {
    this.auth.logout().subscribe({
      next: () => void this.router.navigateByUrl('/'),
      error: () => { this.session.clear(); void this.router.navigateByUrl('/'); },
    });
  }
}
