import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { EventCategoryDto, UpsertEventCategoryRequest } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';

@Component({
  selector: 'app-event-categories',
  imports: [ReactiveFormsModule],
  templateUrl: './event-categories.html',
  styleUrl: './event-categories.scss',
})
export class EventCategoriesComponent implements OnInit {
  private readonly domain = inject(DomainApiService);

  readonly categories = signal<EventCategoryDto[]>([]);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly actionId = signal<string | null>(null);
  readonly error = signal('');
  readonly success = signal('');
  readonly editingId = signal<string | null>(null);

  readonly form = new FormGroup({
    name: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(100)] }),
    code: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(50), Validators.pattern(/^[A-Za-z0-9_-]+$/)] }),
    isActive: new FormControl(true, { nonNullable: true }),
  });

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    this.domain.adminEventCategories().subscribe({
      next: (items) => {
        this.categories.set([...items].sort((a, b) => a.name.localeCompare(b.name)));
        this.loading.set(false);
      },
      error: (error) => {
        this.error.set(httpErrorMessage(error, 'Event categories could not be loaded.'));
        this.loading.set(false);
      },
    });
  }

  submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.saving()) return;

    const raw = this.form.getRawValue();
    const body: UpsertEventCategoryRequest = {
      name: raw.name.trim(),
      code: raw.code.trim().toUpperCase(),
      isActive: raw.isActive,
    };

    if (!body.name || !body.code) return;

    this.saving.set(true);
    this.error.set('');
    this.success.set('');
    const id = this.editingId();
    const request = id
      ? this.domain.updateEventCategory(id, body)
      : this.domain.createEventCategory(body);

    request.subscribe({
      next: () => {
        this.success.set(id ? 'Event category updated.' : 'Event category created.');
        this.saving.set(false);
        this.cancelEdit(false);
        this.load();
      },
      error: (error) => {
        this.error.set(httpErrorMessage(error, 'The event category could not be saved.'));
        this.saving.set(false);
      },
    });
  }

  edit(item: EventCategoryDto): void {
    this.editingId.set(item.eventCategoryId);
    this.success.set('');
    this.error.set('');
    this.form.setValue({ name: item.name, code: item.code, isActive: item.isActive });
    if (typeof window !== 'undefined') window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  cancelEdit(clearMessages = true): void {
    this.editingId.set(null);
    this.form.reset({ name: '', code: '', isActive: true });
    if (clearMessages) {
      this.error.set('');
      this.success.set('');
    }
  }

  deactivate(item: EventCategoryDto): void {
    if (this.actionId()) return;
    if (typeof window !== 'undefined' && !window.confirm(`Deactivate “${item.name}”? Organizers will no longer be able to choose it for new events.`)) return;

    this.actionId.set(item.eventCategoryId);
    this.error.set('');
    this.success.set('');
    this.domain.deactivateEventCategory(item.eventCategoryId).subscribe({
      next: () => {
        this.actionId.set(null);
        this.success.set('Event category deactivated.');
        this.load();
      },
      error: (error) => {
        this.actionId.set(null);
        this.error.set(httpErrorMessage(error, 'The event category could not be deactivated.'));
      },
    });
  }

  activate(item: EventCategoryDto): void {
    if (this.actionId()) return;
    this.actionId.set(item.eventCategoryId);
    this.error.set('');
    this.success.set('');
    this.domain.updateEventCategory(item.eventCategoryId, { name: item.name, code: item.code, isActive: true }).subscribe({
      next: () => {
        this.actionId.set(null);
        this.success.set('Event category activated.');
        this.load();
      },
      error: (error) => {
        this.actionId.set(null);
        this.error.set(httpErrorMessage(error, 'The event category could not be activated.'));
      },
    });
  }
}
