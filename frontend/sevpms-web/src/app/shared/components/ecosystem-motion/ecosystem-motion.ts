import { isPlatformBrowser } from '@angular/common';
import { AfterViewInit, Component, ElementRef, OnDestroy, PLATFORM_ID, inject } from '@angular/core';

@Component({
  selector: 'app-ecosystem-motion',
  templateUrl: './ecosystem-motion.html',
  styleUrl: './ecosystem-motion.scss',
})
export class EcosystemMotionComponent implements AfterViewInit, OnDestroy {
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly platformId = inject(PLATFORM_ID);
  private cleanup?: () => void;

  async ngAfterViewInit(): Promise<void> {
    if (!isPlatformBrowser(this.platformId) || matchMedia('(prefers-reduced-motion: reduce)').matches) return;
    const { gsap } = await import('gsap');
    const { ScrollTrigger } = await import('gsap/ScrollTrigger');
    gsap.registerPlugin(ScrollTrigger);
    const root = this.host.nativeElement;
    const center = root.querySelector<HTMLElement>('.ecosystem__core');
    const lines = root.querySelectorAll<SVGPathElement>('.ecosystem__line');
    const nodes = root.querySelectorAll<HTMLElement>('.ecosystem__node');
    if (!center) return;
    lines.forEach((line) => {
      const length = line.getTotalLength();
      line.style.strokeDasharray = `${length}`;
      line.style.strokeDashoffset = `${length}`;
    });
    const ctx = gsap.context(() => {
      const timeline = gsap.timeline({
        scrollTrigger: { trigger: root, start: 'top 72%', end: 'bottom 30%', scrub: 0.7 },
      });
      timeline
        .fromTo(center, { scale: 0.72, opacity: 0.45 }, { scale: 1.28, opacity: 1, duration: 1, ease: 'power2.out' })
        .to(lines, { strokeDashoffset: 0, duration: 1.2, stagger: 0.05, ease: 'none' }, 0.15)
        .fromTo(nodes, { scale: 0.65, opacity: 0, y: 14 }, { scale: 1, opacity: 1, y: 0, duration: 0.8, stagger: 0.06, ease: 'back.out(1.5)' }, 0.28)
        .to(center, { scale: 1, duration: 0.6, ease: 'power2.out' }, 1.2);
    }, root);
    this.cleanup = () => ctx.revert();
  }

  ngOnDestroy(): void { this.cleanup?.(); }
}
