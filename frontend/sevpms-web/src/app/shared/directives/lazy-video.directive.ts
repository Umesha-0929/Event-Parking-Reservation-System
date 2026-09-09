import { Directive, ElementRef, Input, OnDestroy, OnInit, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Directive({ selector: 'video[appLazyVideo]' })
export class LazyVideoDirective implements OnInit, OnDestroy {
  private readonly host = inject<ElementRef<HTMLVideoElement>>(ElementRef);
  private readonly platformId = inject(PLATFORM_ID);
  private observer?: IntersectionObserver;

  @Input({ required: true }) appLazyVideo = '';

  ngOnInit(): void {
    if (!isPlatformBrowser(this.platformId) || !this.appLazyVideo) return;
    const video = this.host.nativeElement;
    video.preload = 'none';

    if (!('IntersectionObserver' in window)) {
      this.activate();
      return;
    }

    this.observer = new IntersectionObserver((entries) => {
      if (entries.some((entry) => entry.isIntersecting)) {
        this.activate();
        this.observer?.disconnect();
      }
    }, { rootMargin: '320px 0px' });
    this.observer.observe(video);
  }

  ngOnDestroy(): void { this.observer?.disconnect(); }

  private activate(): void {
    const video = this.host.nativeElement;
    if (video.src) return;
    video.src = this.appLazyVideo;
    video.preload = 'metadata';
    video.load();
    if (!window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
      void video.play().catch(() => undefined);
    }
  }
}
