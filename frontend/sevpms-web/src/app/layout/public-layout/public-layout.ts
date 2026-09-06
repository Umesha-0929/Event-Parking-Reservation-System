import { AfterViewInit, Component, OnDestroy, PLATFORM_ID, inject, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { SessionService } from '../../core/services/session.service';
import { AuthService } from '../../core/services/auth.service';
import { NotificationCenterService } from '../../core/services/notification-center.service';
import { CookieBannerComponent } from '../../shared/components/cookie-banner/cookie-banner';

@Component({
  selector: 'app-public-layout',
  imports: [RouterLink, RouterLinkActive, RouterOutlet, CookieBannerComponent],
  templateUrl: './public-layout.html',
  styleUrl: './public-layout.scss',
})
export class PublicLayoutComponent implements AfterViewInit, OnDestroy {
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

  toggleMenu(): void { this.menuOpen.update((value) => !value); }
  closeMenu(): void { this.menuOpen.set(false); }

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
