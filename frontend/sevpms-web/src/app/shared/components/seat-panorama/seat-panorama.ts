import { isPlatformBrowser } from '@angular/common';
import { AfterViewInit, Component, ElementRef, Input, OnDestroy, PLATFORM_ID, ViewChild, inject } from '@angular/core';

@Component({ selector: 'app-seat-panorama', template: '<div #viewer class="seat-panorama" aria-label="360 degree seat view"></div>', styles: [':host{display:block}.seat-panorama{width:100%;height:100%;min-height:320px;border-radius:18px;overflow:hidden;background:#073a3f}'] })
export class SeatPanoramaComponent implements AfterViewInit, OnDestroy {
  @ViewChild('viewer', { static: true }) viewerElement!: ElementRef<HTMLElement>;
  @Input({ required: true }) mediaUrl = '';
  @Input() defaultYaw?: number | null;
  @Input() defaultPitch?: number | null;
  private readonly platformId = inject(PLATFORM_ID);
  private destroyViewer?: () => void;

  async ngAfterViewInit(): Promise<void> {
    if (!isPlatformBrowser(this.platformId) || !this.mediaUrl) return;
    const { Viewer } = await import('@photo-sphere-viewer/core');
    const viewer = new Viewer({
      container: this.viewerElement.nativeElement,
      panorama: this.mediaUrl,
      defaultYaw: this.defaultYaw ?? 0,
      defaultPitch: this.defaultPitch ?? 0,
      navbar: ['zoom', 'move', 'fullscreen'],
      loadingTxt: 'Loading seat view…',
    });
    this.destroyViewer = () => viewer.destroy();
  }
  ngOnDestroy(): void { this.destroyViewer?.(); }
}
