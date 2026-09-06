import { Directive, ElementRef, OnDestroy, OnInit, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Directive({ selector: '[appMagicCard]' })
export class MagicCardDirective implements OnInit, OnDestroy {
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly platformId = inject(PLATFORM_ID);
  private cleanups: Array<() => void> = [];
  private gsapContext?: { revert(): void };

  async ngOnInit(): Promise<void> {
    if (!isPlatformBrowser(this.platformId) || matchMedia('(prefers-reduced-motion: reduce)').matches) return;
    const { gsap } = await import('gsap');
    const el = this.host.nativeElement;
    el.classList.add('magic-card');

    const onMove = (event: PointerEvent) => {
      const rect = el.getBoundingClientRect();
      const x = event.clientX - rect.left;
      const y = event.clientY - rect.top;
      const px = x / rect.width - 0.5;
      const py = y / rect.height - 0.5;
      el.style.setProperty('--magic-x', `${x}px`);
      el.style.setProperty('--magic-y', `${y}px`);
      el.style.setProperty('--magic-opacity', '1');
      gsap.to(el, {
        rotateX: py * -5,
        rotateY: px * 6,
        x: px * 4,
        y: py * 4 - 2,
        transformPerspective: 900,
        duration: 0.28,
        ease: 'power2.out',
        overwrite: true,
      });
    };
    const onLeave = () => {
      el.style.setProperty('--magic-opacity', '0');
      gsap.to(el, { rotateX: 0, rotateY: 0, x: 0, y: 0, duration: 0.4, ease: 'power3.out', overwrite: true });
    };
    const onClick = (event: PointerEvent) => {
      const rect = el.getBoundingClientRect();
      const ripple = document.createElement('span');
      ripple.className = 'magic-card__ripple';
      ripple.style.left = `${event.clientX - rect.left}px`;
      ripple.style.top = `${event.clientY - rect.top}px`;
      el.appendChild(ripple);
      gsap.fromTo(ripple, { scale: 0, opacity: 0.55 }, { scale: 8, opacity: 0, duration: 0.7, ease: 'power2.out', onComplete: () => ripple.remove() });
    };
    el.addEventListener('pointermove', onMove);
    el.addEventListener('pointerleave', onLeave);
    el.addEventListener('click', onClick);
    this.cleanups.push(
      () => el.removeEventListener('pointermove', onMove),
      () => el.removeEventListener('pointerleave', onLeave),
      () => el.removeEventListener('click', onClick),
    );
  }

  ngOnDestroy(): void {
    this.cleanups.forEach((cleanup) => cleanup());
    this.gsapContext?.revert();
  }
}
