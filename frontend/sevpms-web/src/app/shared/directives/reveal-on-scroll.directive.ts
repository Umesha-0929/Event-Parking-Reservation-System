import { isPlatformBrowser } from '@angular/common';
import { AfterViewInit, Directive, ElementRef, Input, OnDestroy, PLATFORM_ID, inject } from '@angular/core';

@Directive({ selector: '[appRevealOnScroll]' })
export class RevealOnScrollDirective implements AfterViewInit, OnDestroy {
  @Input() revealDelay = 0;
  @Input() revealThreshold = 0.14;
  private readonly platformId = inject(PLATFORM_ID);
  private observer?: IntersectionObserver;

  constructor(private readonly elementRef: ElementRef<HTMLElement>) {}

  ngAfterViewInit(): void {
    const element = this.elementRef.nativeElement;
    element.classList.add('scroll-reveal');
    element.style.setProperty('--reveal-delay', `${Math.max(0, this.revealDelay)}ms`);
    if (!isPlatformBrowser(this.platformId)) { element.classList.add('is-visible'); return; }
    const reducedMotion = window.matchMedia?.('(prefers-reduced-motion: reduce)').matches ?? false;
    if (reducedMotion || !('IntersectionObserver' in window)) { element.classList.add('is-visible'); return; }
    this.observer = new IntersectionObserver((entries) => {
      for (const entry of entries) {
        if (!entry.isIntersecting) continue;
        (entry.target as HTMLElement).classList.add('is-visible');
        this.observer?.unobserve(entry.target);
      }
    }, { threshold: this.revealThreshold, rootMargin: '0px 0px -7% 0px' });
    this.observer.observe(element);
  }
  ngOnDestroy(): void { this.observer?.disconnect(); }
}
