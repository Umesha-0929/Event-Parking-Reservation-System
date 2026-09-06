import { Component, PLATFORM_ID, inject, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { AnalyticsService } from '../../../core/services/analytics.service';

@Component({
  selector: 'app-cookie-banner',
  template: `
    @if (visible()) {
      <aside class="cookie" aria-label="Cookie preferences">
        <div>
          <strong>Choose your cookie preferences</strong>
          <p>Essential storage keeps Nvent working. Non-essential analytics stays off unless you accept it.</p>
        </div>
        <div class="cookie__actions">
          <button class="btn btn--ghost" type="button" (click)="choose(false)">Reject non-essential</button>
          <button class="btn btn--primary" type="button" (click)="choose(true)">Accept</button>
        </div>
      </aside>
    }
  `,
  styles: [`
    .cookie{position:fixed;z-index:80;left:50%;bottom:22px;transform:translateX(-50%);width:min(920px,calc(100% - 28px));display:flex;align-items:center;justify-content:space-between;gap:22px;padding:18px 20px;border:1px solid rgba(138,193,200,.38);border-radius:18px;background:rgba(255,255,255,.94);backdrop-filter:blur(18px);box-shadow:0 24px 70px rgba(3,72,77,.2)}
    strong{color:var(--nvent-primary)} p{margin:5px 0 0;color:var(--nvent-muted);font-size:.86rem;line-height:1.5}.cookie__actions{display:flex;gap:8px;flex-shrink:0}@media(max-width:700px){.cookie{align-items:stretch;flex-direction:column}.cookie__actions{display:grid;grid-template-columns:1fr 1fr}}
  `],
})
export class CookieBannerComponent {
  private readonly analytics = inject(AnalyticsService);
  private readonly platformId = inject(PLATFORM_ID);
  readonly visible = signal(false);

  constructor() {
    if (isPlatformBrowser(this.platformId)) this.visible.set(localStorage.getItem('nvent-cookie-choice') === null);
  }

  choose(accepted: boolean): void {
    if (!isPlatformBrowser(this.platformId)) return;
    localStorage.setItem('nvent-cookie-choice', accepted ? 'accepted' : 'essential-only');
    this.analytics.setConsent(accepted);
    this.visible.set(false);
  }
}
