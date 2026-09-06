import { isPlatformBrowser } from '@angular/common';
import { AfterViewInit, Component, ElementRef, OnDestroy, PLATFORM_ID, inject } from '@angular/core';

@Component({ selector: 'app-end-flow', templateUrl: './end-flow.html', styleUrl: './end-flow.scss' })
export class EndFlowComponent implements AfterViewInit, OnDestroy {
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly platformId = inject(PLATFORM_ID);
  private cleanup?: () => void;

  async ngAfterViewInit(): Promise<void> {
    if (!isPlatformBrowser(this.platformId) || matchMedia('(prefers-reduced-motion: reduce)').matches) return;
    const { gsap } = await import('gsap');
    const { ScrollTrigger } = await import('gsap/ScrollTrigger');
    gsap.registerPlugin(ScrollTrigger);
    const root = this.host.nativeElement;
    const track = root.querySelector<HTMLElement>('.end-flow__track');
    if (!track) return;
    const ctx = gsap.context(() => {
      const tween = gsap.to(track, {
        x: () => -(track.scrollWidth - root.clientWidth + 80),
        ease: 'none',
        scrollTrigger: {
          trigger: root,
          start: 'top top',
          end: () => `+=${Math.max(track.scrollWidth * 0.72, window.innerHeight * 1.6)}`,
          scrub: 1,
          pin: true,
          invalidateOnRefresh: true,
        },
      });
      gsap.fromTo(root.querySelectorAll('.end-flow__icon'), { rotate: -8, scale: 0.8 }, { rotate: 8, scale: 1.08, stagger: 0.15, ease: 'sine.inOut', yoyo: true, repeat: -1, duration: 1.4 });
      void tween;
    }, root);
    this.cleanup = () => ctx.revert();
  }
  ngOnDestroy(): void { this.cleanup?.(); }
}
