import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [RouterLink],
  template: `
    <main id="main-content" tabindex="-1" class="relative min-h-screen overflow-hidden grid place-items-center p-5 sm:p-8" style="background:var(--bg)">
      <div aria-hidden="true" class="absolute -top-24 -left-24 w-80 h-80 rounded-full blur-3xl opacity-25" style="background:var(--primary)"></div>
      <div aria-hidden="true" class="absolute -bottom-28 -right-16 w-96 h-96 rounded-full blur-3xl opacity-20" style="background:var(--accent)"></div>
      <section class="relative nv-card nv-glass-panel p-7 sm:p-10 max-w-2xl w-full text-center overflow-hidden">
        <div class="flex items-center justify-center gap-3 mb-6">
          <img src="assets/brand/nvent-mark.svg" alt="" class="w-11 h-11">
          <span class="text-2xl font-black tracking-[-.04em]">Nvent</span>
        </div>
        <div class="mx-auto w-fit rounded-full px-4 py-2 text-xs font-black tracking-[.18em] uppercase" style="background:color-mix(in srgb,var(--primary) 12%,var(--surface));color:var(--primary)">Error 404</div>
        <div class="mt-5 text-[clamp(5rem,18vw,9rem)] leading-none font-black tracking-[-.09em] select-none" style="background:linear-gradient(135deg,var(--primary),var(--accent));-webkit-background-clip:text;background-clip:text;color:transparent">404</div>
        <h1 class="text-3xl sm:text-5xl font-black tracking-[-.05em] mt-2">This page left the venue.</h1>
        <p class="nv-muted mt-4 max-w-lg mx-auto text-base sm:text-lg">The link may be outdated, the address may be mistyped, or the page may have moved. Your Nvent account and bookings are still safe.</p>
        <div class="mt-7 flex flex-col sm:flex-row justify-center gap-3">
          <a [routerLink]="auth.isAuthenticated()?auth.defaultRoute():'/app/home'" class="nv-btn nv-btn-primary">{{auth.isAuthenticated()?'Return to Dashboard':'Explore Nvent'}}</a>
          @if(!auth.isAuthenticated()){<a routerLink="/auth/sign-in" class="nv-btn nv-btn-secondary">Sign In</a>}
        </div>
        <p class="nv-muted text-xs mt-6">If you followed a password-reset email, request a new reset link from the sign-in page.</p>
      </section>
    </main>`
})
export class NotFoundComponent { auth = inject(AuthService); }
