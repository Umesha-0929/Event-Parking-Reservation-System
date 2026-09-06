import { Directive, ElementRef, OnDestroy, OnInit, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Directive({ selector: '[appSeatMotion]' })
export class SeatMotionDirective implements OnInit, OnDestroy {
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly platformId = inject(PLATFORM_ID);
  private cleanup?: () => void;

  async ngOnInit(): Promise<void> {
    if (!isPlatformBrowser(this.platformId) || matchMedia('(prefers-reduced-motion: reduce)').matches) return;
    const { gsap } = await import('gsap');
    const el = this.host.nativeElement;
    const isInteractive = () => !el.matches('[disabled], .seat--booked, .seat--held, .seat--unavailable');
    const enter = () => { if (isInteractive()) gsap.to(el, { y: -4, scale: 1.08, rotateX: -7, duration: .18, ease: 'power2.out' }); };
    const leave = () => gsap.to(el, { y: 0, scale: 1, rotateX: 0, duration: .28, ease: 'power3.out' });
    el.addEventListener('pointerenter', enter); el.addEventListener('pointerleave', leave);
    this.cleanup = () => { el.removeEventListener('pointerenter', enter); el.removeEventListener('pointerleave', leave); };
  }
  ngOnDestroy(): void { this.cleanup?.(); }
}
