import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-access-denied',
  standalone: true,
  imports: [RouterLink],
  template: `
    <main id="main-content" tabindex="-1" class="min-h-screen grid place-items-center p-5" style="background:var(--bg)">
      <section class="nv-card p-7 sm:p-10 max-w-xl w-full text-center">
        <div class="mx-auto w-16 h-16 rounded-3xl grid place-items-center text-2xl font-black" style="background:color-mix(in srgb,var(--danger) 13%,var(--surface));color:var(--danger)">!</div>
        <p class="font-extrabold text-sm nv-muted mt-5">Access denied</p>
        <h1 class="text-3xl sm:text-4xl font-black tracking-[-.04em] mt-1">You do not have permission to open this area.</h1>
        <p class="nv-muted mt-3">Return to your permitted Nvent workspace.</p>
        <a [routerLink]="auth.defaultRoute()" class="nv-btn nv-btn-primary mt-6">Return to Dashboard</a>
      </section>
    </main>`
})
export class AccessDeniedComponent { auth = inject(AuthService); }
