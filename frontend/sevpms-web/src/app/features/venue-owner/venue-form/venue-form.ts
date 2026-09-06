import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { UpsertVenueRequest, VenueSummary } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';

@Component({
  selector: 'app-venue-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './venue-form.html',
  styleUrl: './venue-form.scss',
})
export class VenueFormComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly domain = inject(DomainApiService);
  readonly venueId = this.route.snapshot.paramMap.get('id');
  readonly editing = !!this.venueId;
  readonly loading = signal(this.editing);
  readonly saving = signal(false);
  readonly error = signal('');
  readonly success = signal('');

  readonly form = new FormGroup({
    name: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(160)] }),
    description: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.minLength(10), Validators.maxLength(3000)] }),
    addressLine1: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(250)] }),
    addressLine2: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(250)] }),
    city: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(120)] }),
    district: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(120)] }),
    country: new FormControl('Sri Lanka', { nonNullable: true, validators: [Validators.required, Validators.maxLength(120)] }),
    capacity: new FormControl<number | null>(null, { validators: [Validators.required, Validators.min(1), Validators.max(1000000)] }),
    latitude: new FormControl<number | null>(null),
    longitude: new FormControl<number | null>(null),
    contactPhone: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(40)] }),
    contactEmail: new FormControl('', { nonNullable: true, validators: [Validators.email, Validators.maxLength(200)] }),
  });

  ngOnInit(): void {
    if (!this.venueId) return;
    this.domain.venue(this.venueId).subscribe({
      next: (venue) => { this.patchVenue(venue); this.loading.set(false); },
      error: (error) => { this.error.set(httpErrorMessage(error, 'The venue could not be loaded.')); this.loading.set(false); },
    });
  }

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.saving()) return;
    const value = this.form.getRawValue();
    const body: UpsertVenueRequest = {
      name: value.name.trim(), description: value.description.trim(), addressLine1: value.addressLine1.trim(), addressLine2: value.addressLine2.trim() || null,
      city: value.city.trim(), district: value.district.trim(), country: value.country.trim(), capacity: Number(value.capacity),
      latitude: value.latitude == null ? null : Number(value.latitude), longitude: value.longitude == null ? null : Number(value.longitude),
      contactPhone: value.contactPhone.trim() || null, contactEmail: value.contactEmail.trim() || null,
    };
    this.saving.set(true); this.error.set(''); this.success.set('');
    const request = this.venueId ? this.domain.updateVenue(this.venueId, body) : this.domain.createVenue(body);
    request.subscribe({
      next: (venue) => {
        this.saving.set(false);
        this.success.set(this.editing ? 'Venue updated successfully.' : 'Venue registered successfully.');
        const id = venue.venueId ?? venue.id;
        if (!this.editing && id) void this.router.navigate(['/venue-owner/venues', id, 'edit'], { replaceUrl: true });
      },
      error: (error) => { this.saving.set(false); this.error.set(httpErrorMessage(error, 'The venue could not be saved.')); },
    });
  }

  private patchVenue(venue: VenueSummary): void {
    this.form.patchValue({
      name: venue.name ?? '', description: venue.description ?? '', addressLine1: venue.addressLine1 ?? '', addressLine2: venue.addressLine2 ?? '', city: venue.city ?? '',
      district: venue.district ?? '', country: venue.country ?? 'Sri Lanka', capacity: venue.capacity ?? null, latitude: venue.latitude ?? null, longitude: venue.longitude ?? null,
      contactPhone: venue.contactPhone ?? '', contactEmail: venue.contactEmail ?? '',
    });
  }
}
