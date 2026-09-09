import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { EventCategoryDto, EventSummary, VenueSummary } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';

@Component({
  selector: 'app-event-builder',
  imports: [ReactiveFormsModule],
  templateUrl: './event-builder.html',
  styleUrl: './event-builder.scss'
})
export class EventBuilderComponent implements OnInit {
  private readonly domain = inject(DomainApiService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly venues = signal<VenueSummary[]>([]);
  readonly categories = signal<EventCategoryDto[]>([]);
  readonly loadingOptions = signal(true);
  readonly saving = signal(false);
  readonly error = signal('');
  readonly created = signal<EventSummary | null>(null);
  readonly editing = signal(false);
  readonly eventId = signal('');

  readonly form = new FormGroup({
    title: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    categoryId: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    venueId: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    startAt: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    endAt: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    description: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(10)]
    })
  });

  ngOnInit(): void {
    const editId = this.route.snapshot.paramMap.get('eventId') ?? '';
    this.eventId.set(editId);
    this.editing.set(!!editId);

    let remaining = editId ? 3 : 2;
    const done = () => {
      remaining -= 1;
      if (remaining === 0) this.loadingOptions.set(false);
    };

    this.domain.venues().subscribe({ next: (items) => { this.venues.set(items); done(); }, error: () => done() });
    this.domain.eventCategories().subscribe({ next: (items) => { this.categories.set(items.filter((category) => category.isActive)); done(); }, error: () => done() });

    if (editId) {
      this.domain.myEvents().subscribe({
        next: (items) => {
          const event = items.find((item) => (item.eventId ?? item.id) === editId);
          if (!event) {
            this.error.set('This event could not be found in your organizer account.');
            done();
            return;
          }
          this.form.patchValue({
            title: event.title ?? event.name ?? '',
            categoryId: event.categoryId ?? '',
            venueId: event.venueId ?? '',
            startAt: this.toLocalInput(event.startAtUtc ?? event.startDateTime),
            endAt: this.toLocalInput(event.endAtUtc),
            description: event.description ?? '',
          });
          done();
        },
        error: (error) => { this.error.set(httpErrorMessage(error, 'The event could not be loaded for editing.')); done(); },
      });
    }
  }

  endBeforeStart(): boolean {
    const startValue = this.form.controls.startAt.value;
    const endValue = this.form.controls.endAt.value;

    if (!startValue || !endValue) return false;

    const start = new Date(startValue);
    const end = new Date(endValue);

    if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime())) return false;
    return end.getTime() <= start.getTime();
  }

  save(): void {
    this.form.markAllAsTouched();
    this.error.set('');

    if (this.endBeforeStart()) {
      this.form.controls.endAt.markAsTouched();
      return;
    }

    if (this.form.invalid || this.saving()) return;

    const value = this.form.getRawValue();
    const start = new Date(value.startAt);
    const end = new Date(value.endAt);

    this.saving.set(true);

    const category = this.categories().find(
      (item) => item.eventCategoryId === value.categoryId
    );

    const body = {
      venueId: value.venueId,
      categoryId: value.categoryId,
      category: category?.name ?? '',
      title: value.title,
      description: value.description,
      startAtUtc: start.toISOString(),
      endAtUtc: end.toISOString()
    };
    const request = this.editing()
      ? this.domain.updateEvent(this.eventId(), body)
      : this.domain.createEvent(body);

    request.subscribe({
      next: (event) => {
        this.created.set(event);
        this.saving.set(false);
        const id = event.eventId ?? event.id ?? this.eventId();
        if (this.editing()) void this.router.navigate(['/organizer/events']);
        else if (id) void this.router.navigate(['/organizer/events', id, 'stage'], { queryParams: { created: 1 } });
      },
      error: (requestError) => {
        this.error.set(httpErrorMessage(requestError, this.editing() ? 'The event changes could not be saved.' : 'The event draft could not be created.'));
        this.saving.set(false);
      }
    });
  }

  private toLocalInput(value?: string): string {
    if (!value) return '';
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return '';
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
  }

  venueId(item: VenueSummary): string {
    return item.venueId ?? item.id ?? '';
  }
}
