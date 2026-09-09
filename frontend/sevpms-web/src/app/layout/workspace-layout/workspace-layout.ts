import { Component, ElementRef, HostListener, ViewChild, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { NavItem, UserRole } from '../../core/models/nvent.models';
import { AuthService } from '../../core/services/auth.service';
import { NotificationCenterService } from '../../core/services/notification-center.service';
import { SessionService } from '../../core/services/session.service';
import { NavIconComponent } from '../../shared/components/nav-icon/nav-icon';

@Component({
  selector: 'app-workspace-layout',
  imports: [RouterLink, RouterLinkActive, RouterOutlet, NavIconComponent],
  templateUrl: './workspace-layout.html',
  styleUrl: './workspace-layout.scss',
})
export class WorkspaceLayoutComponent {
  @ViewChild('workspaceMenuButton') private menuButton?: ElementRef<HTMLButtonElement>;
  @ViewChild('workspaceSidebar') private sidebar?: ElementRef<HTMLElement>;
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
      { label: 'Dashboard', path: '/customer/dashboard', icon: 'home' }, { label: 'Browse Events', path: '/customer/browse-events', icon: 'calendar' },
      { label: 'Browse Venues', path: '/customer/browse-venues', icon: 'building' }, { label: 'My Bookings', path: '/customer/bookings', icon: 'check' }, { label: 'Smart Services', path: '/customer/services', icon: 'check' },
      { label: 'My Tickets', path: '/customer/tickets', icon: 'ticket' }, { label: 'Parking', path: '/customer/parking', icon: 'parking' },
      { label: 'Food', path: '/customer/food', icon: 'food' }, { label: 'Place Finder', path: '/customer/places', icon: 'pin' },
      { label: 'Notifications', path: '/customer/notifications', icon: 'bell' }, { label: 'Profile', path: '/customer/profile', icon: 'user' },
      { label: 'Settings', path: '/customer/settings', icon: 'settings' },
    ],
    organizer: [
      { label: 'Dashboard', path: '/organizer/dashboard', icon: 'home' }, { label: 'Events', path: '/organizer/events', icon: 'calendar' },
      { label: 'Create Event', path: '/organizer/create-event', icon: 'plus-circle' }, { label: 'Venues', path: '/organizer/venues', icon: 'building' },
      { label: 'Event Seating', path: '/organizer/events', icon: 'seat' },
      { label: 'QR Check-in', path: '/organizer/qr-checkin', icon: 'qr' }, { label: 'Payment QR', path: '/organizer/payment-qr', icon: 'qr' }, { label: 'Venue Payments', path: '/organizer/venue-payments', icon: 'market' }, { label: 'Payment Reviews', path: '/organizer/payment-reviews', icon: 'check' }, { label: 'Analytics & Finance', path: '/organizer/reports', icon: 'chart' },
      { label: 'Notifications', path: '/organizer/notifications', icon: 'bell' }, { label: 'Settings', path: '/organizer/settings', icon: 'settings' },
    ],
    'venue-owner': [
      { label: 'Dashboard', path: '/venue-owner/dashboard', icon: 'home' }, { label: 'My Venues', path: '/venue-owner/venues', icon: 'building' },
      { label: 'Register Venue', path: '/venue-owner/venues/new', icon: 'plus-circle' }, { label: 'Marketplace', path: '/venue-owner/marketplace', icon: 'market' },
      { label: 'Rentals', path: '/venue-owner/rentals', icon: 'check' }, { label: 'Availability', path: '/venue-owner/availability', icon: 'clock' },
      { label: 'Parking', path: '/venue-owner/parking', icon: 'parking' }, { label: 'Payment QR', path: '/venue-owner/payment-qr', icon: 'qr' }, { label: 'Reports', path: '/venue-owner/reports', icon: 'chart' },
      { label: 'Notifications', path: '/venue-owner/notifications', icon: 'bell' }, { label: 'Settings', path: '/venue-owner/settings', icon: 'settings' },
    ],
    admin: [
      { label: 'Dashboard', path: '/admin/dashboard', icon: 'home' }, { label: 'Events', path: '/admin/events', icon: 'calendar' },
      { label: 'Event Categories', path: '/admin/event-categories', icon: 'list' }, { label: 'Venue Facilities', path: '/admin/venue-facilities', icon: 'tag' },
      { label: 'Venues', path: '/admin/venues', icon: 'building' }, { label: 'Nearby Places', path: '/admin/nearby-places', icon: 'pin' },
      { label: 'Users', path: '/admin/users', icon: 'users' }, { label: 'Payment Reviews', path: '/admin/payments', icon: 'check' }, { label: 'QR Check-in', path: '/admin/qr-checkin', icon: 'qr' },
      { label: 'Reports', path: '/admin/reports', icon: 'chart' }, { label: 'Audit Logs', path: '/admin/audit-logs', icon: 'audit' }, { label: 'Receipt Delivery', path: '/admin/receipt-delivery', icon: 'audit' },
      { label: 'Operations', path: '/admin/operations', icon: 'operations' },
      { label: 'Notifications', path: '/admin/notifications', icon: 'bell' }, { label: 'Settings', path: '/admin/settings', icon: 'settings' },
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
  toggle(): void {
    const opening = !this.mobileOpen();
    this.mobileOpen.set(opening);
    if (opening && typeof document !== 'undefined') queueMicrotask(() => this.focusFirstSidebarItem());
  }

  close(restoreFocus = false): void {
    const wasOpen = this.mobileOpen();
    this.mobileOpen.set(false);
    if (restoreFocus && wasOpen && typeof document !== 'undefined') queueMicrotask(() => this.menuButton?.nativeElement.focus());
  }

  @HostListener('document:keydown', ['$event'])
  onDocumentKeydown(event: KeyboardEvent): void {
    if (!this.mobileOpen() || typeof document === 'undefined') return;
    if (event.key === 'Escape') {
      event.preventDefault();
      this.close(true);
      return;
    }
    if (event.key !== 'Tab') return;
    const focusable = this.sidebarFocusableElements();
    if (!focusable.length) return;
    const first = focusable[0];
    const last = focusable[focusable.length - 1];
    if (event.shiftKey && document.activeElement === first) {
      event.preventDefault();
      last.focus();
    } else if (!event.shiftKey && document.activeElement === last) {
      event.preventDefault();
      first.focus();
    }
  }

  private focusFirstSidebarItem(): void { this.sidebarFocusableElements()[0]?.focus(); }
  private sidebarFocusableElements(): HTMLElement[] {
    const root = this.sidebar?.nativeElement;
    if (!root) return [];
    return Array.from(root.querySelectorAll<HTMLElement>('a[href], button:not([disabled]), [tabindex]:not([tabindex="-1"])'));
  }
  logout(): void {
    this.auth.logout().subscribe({
      next: () => void this.router.navigateByUrl('/'),
      error: () => { this.session.clear(); void this.router.navigateByUrl('/'); },
    });
  }
}
