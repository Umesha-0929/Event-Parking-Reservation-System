import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class AnalyticsService {
  private consent = false;

  setConsent(granted: boolean): void {
    this.consent = granted;
  }

  track(_eventName: string, _payload: Record<string, unknown> = {}): void {
    if (!this.consent) return;
    // Intentionally no-op until a real analytics provider + tracking ID are configured.
  }
}
