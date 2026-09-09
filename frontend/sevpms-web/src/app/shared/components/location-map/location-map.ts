import { Component, EventEmitter, Input, Output, inject, signal } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { ApiService } from '../../../core/services/api.service';

export type PlatformMapKind = 'venue' | 'event' | 'food' | 'place' | 'selected';
export interface PlatformMapPin {
  id: string;
  name: string;
  latitude: number;
  longitude: number;
  kind: PlatformMapKind;
  subtitle?: string;
  visualOffsetX?: number;
  visualOffsetY?: number;
}
export interface MapLocationSelection {
  latitude: number;
  longitude: number;
  displayName?: string;
  addressLine1?: string;
  city?: string;
  district?: string;
  country?: string;
}

@Component({
  selector: 'app-location-map',
  standalone: true,
  templateUrl: './location-map.html',
  styleUrl: './location-map.scss',
})
export class LocationMapComponent {
  private readonly sanitizer = inject(DomSanitizer);
  private readonly api = inject(ApiService);

  @Input() pins: PlatformMapPin[] = [];
  @Input() latitude: number | null = null;
  @Input() longitude: number | null = null;
  @Input() selectable = true;
  @Input() height = 360;

  @Output() locationSelected = new EventEmitter<MapLocationSelection>();
  @Output() pinSelected = new EventEmitter<PlatformMapPin>();

  readonly reverseBusy = signal(false);
  readonly zoom = signal(1);

  private center(): { lat: number; lon: number } {
    const all = [...this.pins];
    if (this.latitude != null && this.longitude != null) {
      all.push({
        id: 'selected',
        name: 'Selected',
        latitude: Number(this.latitude),
        longitude: Number(this.longitude),
        kind: 'selected',
      });
    }
    if (!all.length) return { lat: 7.8731, lon: 80.7718 };
    return {
      lat: all.reduce((sum, pin) => sum + Number(pin.latitude), 0) / all.length,
      lon: all.reduce((sum, pin) => sum + Number(pin.longitude), 0) / all.length,
    };
  }

  span(): number { return 7 / this.zoom(); }

  bbox(): { left: number; right: number; bottom: number; top: number } {
    const c = this.center();
    const s = this.span();
    return { left: c.lon - s, right: c.lon + s, bottom: c.lat - s * 0.62, top: c.lat + s * 0.62 };
  }

  mapUrl(): SafeResourceUrl {
    const b = this.bbox();
    const marker = this.latitude != null && this.longitude != null
      ? `&marker=${Number(this.latitude)}%2C${Number(this.longitude)}`
      : '';
    const url = `https://www.openstreetmap.org/export/embed.html?bbox=${b.left}%2C${b.bottom}%2C${b.right}%2C${b.top}&layer=mapnik${marker}`;
    return this.sanitizer.bypassSecurityTrustResourceUrl(url);
  }

  pinStyle(pin: PlatformMapPin): Record<string, string> {
    const b = this.bbox();
    const left = ((Number(pin.longitude) - b.left) / (b.right - b.left)) * 100;
    const top = ((b.top - Number(pin.latitude)) / (b.top - b.bottom)) * 100;
    return {
      left: `${Math.max(0, Math.min(100, left))}%`,
      top: `${Math.max(0, Math.min(100, top))}%`,
      marginLeft: `${Number(pin.visualOffsetX ?? 0)}px`,
      marginTop: `${Number(pin.visualOffsetY ?? 0)}px`,
    };
  }

  selectedPin(): PlatformMapPin | null {
    if (this.latitude == null || this.longitude == null) return null;
    return {
      id: 'selected-location',
      name: 'Selected location',
      latitude: Number(this.latitude),
      longitude: Number(this.longitude),
      kind: 'selected',
    };
  }

  choose(event: MouseEvent, overlay: HTMLElement): void {
    if (!this.selectable) return;
    const rect = overlay.getBoundingClientRect();
    if (!rect.width || !rect.height) return;
    const b = this.bbox();
    const x = Math.max(0, Math.min(1, (event.clientX - rect.left) / rect.width));
    const y = Math.max(0, Math.min(1, (event.clientY - rect.top) / rect.height));
    const longitude = b.left + x * (b.right - b.left);
    const latitude = b.top - y * (b.top - b.bottom);
    this.resolveAndEmit(Number(latitude.toFixed(6)), Number(longitude.toFixed(6)));
  }

  zoomBy(value: number): void {
    this.zoom.set(Math.max(0.55, Math.min(8, this.zoom() * value)));
  }

  useMyLocation(): void {
    if (typeof navigator === 'undefined' || !navigator.geolocation) return;
    navigator.geolocation.getCurrentPosition(
      (position) => this.resolveAndEmit(position.coords.latitude, position.coords.longitude),
      () => undefined,
      { enableHighAccuracy: true, timeout: 8000 },
    );
  }

  private resolveAndEmit(latitude: number, longitude: number): void {
    this.latitude = latitude;
    this.longitude = longitude;
    const fallback: MapLocationSelection = { latitude, longitude };
    this.reverseBusy.set(true);
    this.api.get<MapLocationSelection>('maps/reverse', { lat: latitude, lon: longitude }).subscribe({
      next: (value) => {
        this.reverseBusy.set(false);
        this.latitude = Number(value.latitude);
        this.longitude = Number(value.longitude);
        this.locationSelected.emit(value);
      },
      error: () => {
        this.reverseBusy.set(false);
        this.locationSelected.emit(fallback);
      },
    });
  }
}
