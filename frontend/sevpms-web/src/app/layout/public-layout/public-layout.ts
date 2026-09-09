import { AfterViewInit, Component, ElementRef, HostListener, OnDestroy, PLATFORM_ID, ViewChild, inject, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { SessionService } from '../../core/services/session.service';
import { AuthService } from '../../core/services/auth.service';
import { NotificationCenterService } from '../../core/services/notification-center.service';

@Component({
  selector: 'app-public-layout',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './public-layout.html',
  styleUrl: './public-layout.scss',
})
export class PublicLayoutComponent implements AfterViewInit, OnDestroy {
  @ViewChild('menuToggle') private menuToggle?: ElementRef<HTMLButtonElement>;
  @ViewChild('mobileNav') private mobileNav?: ElementRef<HTMLElement>;
  private readonly platformId = inject(PLATFORM_ID);
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  readonly session = inject(SessionService);
  readonly notifications = inject(NotificationCenterService);
  readonly menuOpen = signal(false);
  readonly scrolled = signal(false);
  readonly scrollProgress = signal(0);
  readonly loggingOut = signal(false);
  readonly year = new Date().getFullYear();

  private readonly onScroll = (): void => {
    if (!isPlatformBrowser(this.platformId)) return;
    const y = window.scrollY || 0;
    const max = Math.max(1, document.documentElement.scrollHeight - window.innerHeight);
    this.scrolled.set(y > 24);
    this.scrollProgress.set(Math.min(1, Math.max(0, y / max)));
  };

  ngAfterViewInit(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    this.onScroll();
    window.addEventListener('scroll', this.onScroll, { passive: true });
  }

  ngOnDestroy(): void {
    if (isPlatformBrowser(this.platformId)) window.removeEventListener('scroll', this.onScroll);
  }

  toggleMenu(): void {
    const opening = !this.menuOpen();
    this.menuOpen.set(opening);
    if (opening && isPlatformBrowser(this.platformId)) {
      queueMicrotask(() => this.focusFirstMenuItem());
    }
  }

  closeMenu(restoreFocus = false): void {
    const wasOpen = this.menuOpen();
    this.menuOpen.set(false);
    if (restoreFocus && wasOpen && isPlatformBrowser(this.platformId)) {
      queueMicrotask(() => this.menuToggle?.nativeElement.focus());
    }
  }

  @HostListener('document:keydown', ['$event'])
  onDocumentKeydown(event: KeyboardEvent): void {
    if (!this.menuOpen() || !isPlatformBrowser(this.platformId)) return;
    if (event.key === 'Escape') {
      event.preventDefault();
      this.closeMenu(true);
      return;
    }
    if (event.key !== 'Tab') return;
    const focusable = this.menuFocusableElements();
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

  private focusFirstMenuItem(): void {
    this.menuFocusableElements()[0]?.focus();
  }

  private menuFocusableElements(): HTMLElement[] {
    const root = this.mobileNav?.nativeElement;
    if (!root) return [];
    return Array.from(root.querySelectorAll<HTMLElement>('a[href], button:not([disabled]), [tabindex]:not([tabindex="-1"])'));
  }

  logout(): void {
    if (this.loggingOut()) return;
    this.loggingOut.set(true);
    this.auth.logout().subscribe({
      next: () => this.finishLogout(),
      error: () => { this.session.clear(); this.finishLogout(); },
    });
  }

  private finishLogout(): void {
    this.session.clear();
    this.loggingOut.set(false);
    this.closeMenu();
    void this.router.navigateByUrl('/login');
  }
}
