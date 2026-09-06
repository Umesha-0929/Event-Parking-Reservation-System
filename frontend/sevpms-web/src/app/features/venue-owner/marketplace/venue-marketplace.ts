
import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { VenueFacilityDto, VenueMarketplaceDto, VenueSummary } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';

@Component({
  selector: 'app-venue-marketplace',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './venue-marketplace.html',
  styleUrl: './venue-marketplace.scss',
})
export class VenueMarketplaceComponent implements OnInit {
  private readonly domain = inject(DomainApiService);
  private readonly route = inject(ActivatedRoute);

  readonly venues = signal<VenueSummary[]>([]);
  readonly facilities = signal<VenueFacilityDto[]>([]);
  readonly marketplace = signal<VenueMarketplaceDto | null>(null);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly error = signal('');
  readonly success = signal('');
  selectedVenueId = '';
  selectedFacilityIds = new Set<string>();

  mediaUrl = ''; mediaType = 'Photo'; mediaSortOrder = 0;
  rateType = 'Hourly'; rateAmount = 0; rateCurrency = 'LKR'; rateFrom = ''; rateTo = '';
  layoutName = 'Standard layout'; layoutVersion = 1; layoutRows = 8; layoutColumns = 12; layoutAisleEvery = 6; layoutAlignment = 'Centered';

  ngOnInit(): void {
    this.domain.myVenues().subscribe({
      next: (venues) => {
        this.venues.set(venues.filter(v => v.isActive !== false));
        const fromQuery = this.route.snapshot.queryParamMap.get('venueId');
        this.selectedVenueId = (fromQuery && venues.some(v => this.id(v) === fromQuery)) ? fromQuery : this.id(venues[0]);
        if (this.selectedVenueId) this.load();
        else this.loading.set(false);
      },
      error: (e) => { this.loading.set(false); this.error.set(httpErrorMessage(e, 'Your venues could not be loaded.')); }
    });
  }

  id(v?: VenueSummary): string { return v?.venueId ?? v?.id ?? ''; }

  load(): void {
    if (!this.selectedVenueId) return;
    this.loading.set(true); this.error.set(''); this.success.set('');
    forkJoin({
      marketplace: this.domain.venueMarketplace(this.selectedVenueId),
      facilities: this.domain.venueFacilities(),
    }).subscribe({
      next: ({marketplace, facilities}) => {
        this.marketplace.set(marketplace);
        this.facilities.set(facilities.filter(x => x.isActive));
        this.selectedFacilityIds = new Set(marketplace.facilities.map(x => x.facilityId));
        this.loading.set(false);
      },
      error: (e) => { this.loading.set(false); this.error.set(httpErrorMessage(e, 'Marketplace details could not be loaded.')); }
    });
  }

  toggleFacility(id: string, checked: boolean): void {
    if (checked) this.selectedFacilityIds.add(id); else this.selectedFacilityIds.delete(id);
  }

  saveFacilities(): void {
    if (!this.selectedVenueId || this.saving()) return;
    this.saving.set(true); this.error.set(''); this.success.set('');
    this.domain.setVenueFacilities(this.selectedVenueId, [...this.selectedFacilityIds]).subscribe({
      next: () => { this.saving.set(false); this.success.set('Facilities updated.'); this.load(); },
      error: (e) => { this.saving.set(false); this.error.set(httpErrorMessage(e, 'Facilities could not be saved.')); }
    });
  }

  addMedia(): void {
    if (!this.selectedVenueId || !this.mediaUrl.trim() || this.saving()) return;
    this.saving.set(true); this.error.set('');
    this.domain.addVenueMedia(this.selectedVenueId, {url:this.mediaUrl.trim(), type:this.mediaType, sortOrder:Number(this.mediaSortOrder)||0}).subscribe({
      next: () => { this.mediaUrl=''; this.saving.set(false); this.success.set('Media added.'); this.load(); },
      error: e => { this.saving.set(false); this.error.set(httpErrorMessage(e, 'Media could not be added.')); }
    });
  }

  addRate(): void {
    if (!this.selectedVenueId || this.rateAmount < 0 || this.saving()) return;
    this.saving.set(true); this.error.set('');
    this.domain.addVenueRate(this.selectedVenueId, {
      rateType:this.rateType, amount:Number(this.rateAmount), currency:this.rateCurrency || 'LKR',
      validFromUtc:this.rateFrom ? new Date(this.rateFrom).toISOString() : null,
      validToUtc:this.rateTo ? new Date(this.rateTo).toISOString() : null
    }).subscribe({
      next: () => { this.saving.set(false); this.success.set('Rate added.'); this.load(); },
      error: e => { this.saving.set(false); this.error.set(httpErrorMessage(e, 'Rate could not be added.')); }
    });
  }

  addLayoutTemplate(): void {
    if (!this.selectedVenueId || !this.layoutName.trim() || this.layoutRows < 1 || this.layoutColumns < 1 || this.saving()) return;
    const layoutJson = JSON.stringify({
      alignment: this.layoutAlignment,
      rows: Number(this.layoutRows),
      columns: Number(this.layoutColumns),
      aisleEvery: Number(this.layoutAisleEvery),
      source: 'venue-owner'
    });
    this.saving.set(true); this.error.set('');
    this.domain.addVenueLayoutTemplate(this.selectedVenueId, {name:this.layoutName.trim(), version:Number(this.layoutVersion)||1, layoutJson}).subscribe({
      next: () => { this.saving.set(false); this.success.set('Layout template saved.'); this.load(); },
      error: e => { this.saving.set(false); this.error.set(httpErrorMessage(e, 'Layout template could not be saved.')); }
    });
  }

  layoutSummary(json: string): string {
    try {
      const x = JSON.parse(json) as Record<string, unknown>;
      return `${x['alignment'] ?? 'Custom'} · ${x['rows'] ?? '?'} rows × ${x['columns'] ?? '?'} columns`;
    } catch { return 'Custom layout metadata'; }
  }
}
