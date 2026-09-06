import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { catchError, forkJoin, of } from 'rxjs';
import { EventSummary, SeatApiModel, SeatCategoryDto, SeatSectionDto, SeatingLayoutDto, VenueRentalDto } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';

type SetupStep = 'stage' | 'seating' | 'categories' | 'preview';
interface StageChoice { label: string; value: number; help: string; }

@Component({
  selector: 'app-event-setup',
  imports: [FormsModule, RouterLink],
  templateUrl: './event-setup.html',
  styleUrl: './event-setup.scss'
})
export class OrganizerEventSetupComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly domain = inject(DomainApiService);

  readonly eventId = this.route.snapshot.paramMap.get('eventId') ?? '';
  readonly step = (this.route.snapshot.data['step'] ?? 'stage') as SetupStep;
  readonly event = signal<EventSummary | null>(null);
  readonly layout = signal<SeatingLayoutDto | null>(null);
  readonly seats = signal<SeatApiModel[]>([]);
  readonly rentals = signal<VenueRentalDto[]>([]);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly error = signal('');
  readonly notice = signal('');

  readonly stages: StageChoice[] = [
    { label: 'Arena Stage', value: 1, help: 'A central performance area suited to large audiences.' },
    { label: 'Proscenium Theatre Stage', value: 2, help: 'A framed stage with the audience facing forward.' },
    { label: 'End-On Stage', value: 3, help: 'The audience faces the stage from one end of the room.' },
    { label: 'Thrust Stage', value: 4, help: 'The stage projects into the audience on three sides.' },
    { label: 'Traverse Stage', value: 5, help: 'Audience seating runs along two opposing sides.' },
    { label: 'In-the-Round Stage', value: 6, help: 'The audience surrounds the performance area.' }
  ];

  stageType = 2;
  rows = 8;
  columns = 12;

  editingSectionId: string | null = null;
  sectionName = 'Main Floor';
  sectionCode = 'MAIN';
  sectionRows = 8;
  sectionColumns = 12;
  sectionAccessible = false;

  editingCategoryId: string | null = null;
  categoryName = 'Standard';
  categoryCode = 'STD';
  categoryPrice = 5000;
  categoryActive = true;

  selectedSectionId = '';
  selectedCategoryId = '';
  startingRowLabel = 'A';
  startingSeatNumber = 1;
  aisleAfterColumn = 0;
  blockedSeats = '';
  accessibleSeats = '';

  rentalPurpose = '';
  rentalOffer = 0;

  readonly activeCategories = computed(() => this.layout()?.categories.filter((item) => item.isActive) ?? []);
  readonly enabledSections = computed(() => this.layout()?.sections.filter((item) => item.isEnabled) ?? []);
  readonly isPublishedEvent = computed(() => Number(this.event()?.status) === 1 || String(this.event()?.status).toLowerCase() === 'published');
  readonly isPublishedLayout = computed(() => !!this.layout()?.isPublished);
  readonly currentRental = computed(() => this.findCurrentRental());
  readonly acceptedRental = computed(() => this.rentalStatusCode(this.currentRental()?.status) === 1);

  ngOnInit(): void { this.load(); }

  load(): void {
    if (!this.eventId) { this.error.set('Choose an event before opening setup.'); this.loading.set(false); return; }
    this.loading.set(true);
    this.error.set('');
    forkJoin({
      events: this.domain.myEvents(),
      layout: this.domain.organizerLayout(this.eventId).pipe(catchError(() => of(null))),
      seats: this.domain.seats(this.eventId).pipe(catchError(() => of([] as SeatApiModel[]))),
      rentals: this.domain.venueRentalsMine().pipe(catchError(() => of([] as VenueRentalDto[])))
    }).subscribe({
      next: ({ events, layout, seats, rentals }) => {
        const event = events.find((item) => (item.eventId ?? item.id) === this.eventId) ?? null;
        if (!event) {
          this.error.set('This event is not available in your organizer account.');
          this.loading.set(false);
          return;
        }
        this.event.set(event);
        this.layout.set(layout);
        this.seats.set(seats);
        this.rentals.set(rentals);
        this.rentalPurpose = event.title || event.name || 'Event venue booking';
        if (layout) this.hydrateLayout(layout);
        if (this.route.snapshot.queryParamMap.get('created') === '1') this.notice.set('Event draft created. Continue with the stage setup.');
        this.loading.set(false);
      },
      error: (error) => { this.error.set(httpErrorMessage(error, 'Event setup could not be loaded.')); this.loading.set(false); }
    });
  }

  private hydrateLayout(layout: SeatingLayoutDto): void {
    this.stageType = Number(layout.stageType) || 2;
    this.rows = layout.rowCount || 8;
    this.columns = layout.columnCount || 12;
    this.sectionRows = this.rows;
    this.sectionColumns = this.columns;
    this.selectedSectionId ||= layout.sections.find((item) => item.isEnabled)?.id ?? '';
    this.selectedCategoryId ||= layout.categories.find((item) => item.isActive)?.id ?? '';
  }

  eventTitle(): string { return this.event()?.title || this.event()?.name || 'Event'; }
  stepNumber(): number { return this.step === 'stage' ? 2 : this.step === 'seating' ? 3 : this.step === 'categories' ? 4 : 5; }
  stageLabel(): string { return this.stages.find((item) => item.value === this.stageType)?.label ?? 'Stage'; }
  selectedSection(): SeatSectionDto | null { return this.layout()?.sections.find((item) => item.id === this.selectedSectionId) ?? null; }
  previewSeatIndexes(): number[] { const r = Math.max(1, Math.min(8, Number(this.rows) || 1)); const c = Math.max(1, Math.min(16, Number(this.columns) || 1)); return Array.from({ length: r * c }, (_, index) => index); }

  go(step: SetupStep): void { void this.router.navigate(['/organizer/events', this.eventId, step]); }

  saveStage(): void {
    if (this.readOnly()) return;
    this.saving.set(true); this.error.set(''); this.notice.set('');
    const g = this.stageGeometry();
    this.domain.saveLayout(this.eventId, {
      stageType: this.stageType,
      rowCount: this.clamp(this.rows, 1, 30),
      columnCount: this.clamp(this.columns, 1, 40),
      canvasWidth: 900, canvasHeight: 620,
      stageX: g.x, stageY: g.y, stageWidth: g.w, stageHeight: g.h
    }).subscribe({
      next: (layout) => { this.layout.set(layout); this.hydrateLayout(layout); this.saving.set(false); void this.router.navigate(['/organizer/events', this.eventId, 'seating']); },
      error: (error) => { this.error.set(httpErrorMessage(error, 'The stage setup could not be saved.')); this.saving.set(false); }
    });
  }

  updateGrid(): void {
    const layout = this.layout();
    if (!layout || this.readOnly()) return;
    this.saving.set(true); this.error.set('');
    const g = this.stageGeometry();
    this.domain.saveLayout(this.eventId, {
      stageType: this.stageType,
      rowCount: this.clamp(this.rows, 1, 30),
      columnCount: this.clamp(this.columns, 1, 40),
      canvasWidth: 900, canvasHeight: 620,
      stageX: g.x, stageY: g.y, stageWidth: g.w, stageHeight: g.h
    }).subscribe({
      next: (saved) => { this.layout.set({ ...saved, sections: layout.sections, categories: layout.categories }); this.saving.set(false); this.notice.set('Seating grid updated.'); },
      error: (error) => { this.error.set(httpErrorMessage(error, 'The seating grid could not be updated.')); this.saving.set(false); }
    });
  }

  saveSection(): void {
    const layout = this.layout();
    if (!layout || this.readOnly()) { this.error.set('Save the stage setup before adding a seating section.'); return; }
    if (!this.sectionName.trim() || !this.sectionCode.trim()) { this.error.set('Section name and code are required.'); return; }
    this.saving.set(true); this.error.set(''); this.notice.set('');
    const order = this.editingSectionId ? (layout.sections.find((item) => item.id === this.editingSectionId)?.displayOrder ?? 1) : layout.sections.length + 1;
    const width = Math.max(260, 740 / Math.max(1, Math.min(layout.sections.length + 1, 3)));
    this.domain.saveSections(this.eventId, {
      id: this.editingSectionId,
      name: this.sectionName.trim(), code: this.sectionCode.trim().toUpperCase(),
      rowCount: this.clamp(this.sectionRows, 1, this.rows), columnCount: this.clamp(this.sectionColumns, 1, this.columns),
      x: 80 + ((order - 1) * 30), y: 210 + ((order - 1) * 24), width, height: 350,
      displayOrder: order, isAccessibleSection: this.sectionAccessible, isEnabled: true
    }).subscribe({
      next: (saved) => {
        const sections = this.replaceById(layout.sections, saved);
        this.layout.set({ ...layout, sections });
        this.selectedSectionId ||= saved.id;
        this.resetSectionForm();
        this.saving.set(false); this.notice.set('Seating section saved.');
      },
      error: (error) => { this.error.set(httpErrorMessage(error, 'The seating section could not be saved.')); this.saving.set(false); }
    });
  }

  editSection(section: SeatSectionDto): void {
    this.editingSectionId = section.id; this.sectionName = section.name; this.sectionCode = section.code;
    this.sectionRows = section.rowCount; this.sectionColumns = section.columnCount; this.sectionAccessible = section.isAccessibleSection;
  }
  resetSectionForm(): void { this.editingSectionId = null; this.sectionName = 'Main Floor'; this.sectionCode = 'MAIN'; this.sectionRows = this.rows; this.sectionColumns = this.columns; this.sectionAccessible = false; }

  saveCategory(): void {
    const layout = this.layout();
    if (!layout || this.readOnly()) { this.error.set('Complete the stage and seating setup before adding categories.'); return; }
    if (!this.categoryName.trim() || !this.categoryCode.trim()) { this.error.set('Category name and code are required.'); return; }
    if (Number(this.categoryPrice) < 0) { this.error.set('Seat price cannot be negative.'); return; }
    this.saving.set(true); this.error.set(''); this.notice.set('');
    const order = this.editingCategoryId ? (layout.categories.find((item) => item.id === this.editingCategoryId)?.displayOrder ?? 1) : layout.categories.length + 1;
    this.domain.saveCategories(this.eventId, {
      id: this.editingCategoryId,
      name: this.categoryName.trim(), code: this.categoryCode.trim().toUpperCase(), price: Number(this.categoryPrice) || 0,
      displayOrder: order, isActive: this.categoryActive
    }).subscribe({
      next: (saved) => {
        const categories = this.replaceById(layout.categories, saved);
        this.layout.set({ ...layout, categories });
        this.selectedCategoryId ||= saved.id;
        this.resetCategoryForm();
        this.saving.set(false); this.notice.set('Seat category saved.');
      },
      error: (error) => { this.error.set(httpErrorMessage(error, 'The seat category could not be saved.')); this.saving.set(false); }
    });
  }

  editCategory(category: SeatCategoryDto): void {
    this.editingCategoryId = category.id; this.categoryName = category.name; this.categoryCode = category.code; this.categoryPrice = category.price; this.categoryActive = category.isActive;
  }
  resetCategoryForm(): void { this.editingCategoryId = null; this.categoryName = ''; this.categoryCode = ''; this.categoryPrice = 0; this.categoryActive = true; }

  generateSectionSeats(): void {
    const section = this.selectedSection();
    if (!section || !this.selectedCategoryId || this.readOnly()) { this.error.set('Choose a seating section and an active seat category.'); return; }
    this.saving.set(true); this.error.set(''); this.notice.set('');
    const blocked = this.parseSeatList(this.blockedSeats, section);
    const accessible = this.parseSeatList(this.accessibleSeats, section);
    const aisle = this.clamp(this.aisleAfterColumn, 0, Math.max(0, section.columnCount - 1));
    const gaps = aisle > 0 ? Array.from({ length: section.rowCount }, (_, index) => ({ rowNumber: index + 1, startColumn: aisle + 1, columnSpan: 1 })) : [];
    this.domain.generateSeats(this.eventId, {
      sectionId: section.id, seatCategoryId: this.selectedCategoryId,
      rowCount: section.rowCount, columnCount: section.columnCount,
      startingRowLabel: this.startingRowLabel.trim().toUpperCase() || 'A', startingSeatNumber: this.clamp(this.startingSeatNumber, 1, 999),
      startX: 110, startY: 255,
      horizontalSpacing: Math.max(24, 650 / Math.max(section.columnCount, 1)), verticalSpacing: Math.max(24, 285 / Math.max(section.rowCount, 1)),
      unavailablePositions: blocked, accessiblePositions: accessible, gaps
    }).subscribe({
      next: () => {
        this.domain.seats(this.eventId).subscribe({
          next: (items) => { this.seats.set(items); this.saving.set(false); this.notice.set(`Seats generated for ${section.name}.`); },
          error: () => { this.saving.set(false); this.notice.set(`Seats generated for ${section.name}.`); }
        });
      },
      error: (error) => { this.error.set(httpErrorMessage(error, 'Seats could not be generated.')); this.saving.set(false); }
    });
  }

  requestRental(): void {
    const event = this.event();
    if (!event?.venueId || !event.startAtUtc || !event.endAtUtc) { this.error.set('The event needs a venue and complete dates before requesting venue approval.'); return; }
    this.saving.set(true); this.error.set(''); this.notice.set('');
    this.domain.createVenueRental({
      venueId: event.venueId, startAtUtc: event.startAtUtc, endAtUtc: event.endAtUtc,
      purpose: this.rentalPurpose.trim() || this.eventTitle(), offeredAmount: Math.max(0, Number(this.rentalOffer) || 0)
    }).subscribe({
      next: (rental) => { this.rentals.update((items) => [rental, ...items]); this.saving.set(false); this.notice.set('Venue approval request sent. The venue owner can review it from Rentals.'); },
      error: (error) => { this.error.set(httpErrorMessage(error, 'The venue approval request could not be sent.')); this.saving.set(false); }
    });
  }

  refreshRental(): void {
    this.domain.venueRentalsMine().subscribe({ next: (items) => { this.rentals.set(items); this.notice.set('Venue approval status refreshed.'); }, error: (error) => this.error.set(httpErrorMessage(error, 'Venue approval status could not be refreshed.')) });
  }

  publishJourney(): void {
    if (this.readOnly()) return;
    if (!this.layout() || this.enabledSections().length === 0 || this.activeCategories().length === 0) { this.error.set('Complete the stage, seating sections and seat categories before publishing.'); return; }
    if (this.seats().length === 0) { this.error.set('Generate seats for at least one section before publishing.'); return; }
    if (!this.acceptedRental()) { this.error.set('The venue owner must accept a rental request covering this event time before the event can be published.'); return; }
    this.saving.set(true); this.error.set(''); this.notice.set('');
    this.domain.publishLayout(this.eventId, { publish: true }).subscribe({
      next: (layout) => {
        this.layout.set(layout);
        this.domain.publishEvent(this.eventId).subscribe({
          next: () => { this.saving.set(false); this.notice.set('Event published successfully.'); setTimeout(() => void this.router.navigate(['/organizer/events']), 500); },
          error: (error) => {
            this.domain.publishLayout(this.eventId, { publish: false }).subscribe({ next: (rollback) => this.layout.set(rollback), error: () => undefined });
            this.error.set(httpErrorMessage(error, 'The event could not be published.')); this.saving.set(false);
          }
        });
      },
      error: (error) => { this.error.set(httpErrorMessage(error, 'The seating layout could not be published.')); this.saving.set(false); }
    });
  }

  readOnly(): boolean { return this.isPublishedEvent() || this.isPublishedLayout(); }
  rentalStatusLabel(status: VenueRentalDto['status'] | undefined): string {
    const code = this.rentalStatusCode(status);
    return code === 1 ? 'Accepted' : code === 2 ? 'Rejected' : code === 3 ? 'Negotiating' : code === 4 ? 'Cancelled' : 'Pending';
  }

  private findCurrentRental(): VenueRentalDto | null {
    const event = this.event();
    if (!event?.venueId || !event.startAtUtc || !event.endAtUtc) return null;
    const eventStart = new Date(event.startAtUtc).getTime(); const eventEnd = new Date(event.endAtUtc).getTime();
    const matches = this.rentals().filter((rental) => rental.venueId === event.venueId && new Date(rental.startAtUtc).getTime() <= eventStart && new Date(rental.endAtUtc).getTime() >= eventEnd);
    return matches.find((rental) => this.rentalStatusCode(rental.status) === 1) ?? matches[0] ?? null;
  }
  private rentalStatusCode(status: VenueRentalDto['status'] | undefined): number {
    if (typeof status === 'number') return status;
    const normalized = String(status ?? '').toLowerCase();
    return normalized === 'accepted' ? 1 : normalized === 'rejected' ? 2 : normalized === 'negotiating' ? 3 : normalized === 'cancelled' ? 4 : 0;
  }
  private replaceById<T extends { id: string }>(items: T[], saved: T): T[] { return items.some((item) => item.id === saved.id) ? items.map((item) => item.id === saved.id ? saved : item) : [...items, saved]; }
  private clamp(value: number, min: number, max: number): number { return Math.max(min, Math.min(max, Number(value) || min)); }
  private stageGeometry(): { x: number; y: number; w: number; h: number } {
    switch (this.stageType) {
      case 1: return { x: 300, y: 65, w: 300, h: 125 };
      case 4: return { x: 330, y: 55, w: 240, h: 190 };
      case 5: return { x: 350, y: 80, w: 200, h: 380 };
      case 6: return { x: 350, y: 100, w: 200, h: 200 };
      default: return { x: 245, y: 55, w: 410, h: 95 };
    }
  }
  private parseSeatList(value: string, section: SeatSectionDto): { rowNumber: number; columnNumber: number }[] {
    return value.split(',').map((item) => item.trim().toUpperCase()).filter(Boolean).map((token) => {
      const match = token.match(/^([A-Z]+)\s*(\d+)$/);
      if (!match) return null;
      const rowNumber = this.rowLabelNumber(match[1]); const columnNumber = Number(match[2]);
      return rowNumber >= 1 && rowNumber <= section.rowCount && columnNumber >= 1 && columnNumber <= section.columnCount ? { rowNumber, columnNumber } : null;
    }).filter((item): item is { rowNumber: number; columnNumber: number } => !!item);
  }
  private rowLabelNumber(label: string): number { return label.split('').reduce((sum, char) => (sum * 26) + (char.charCodeAt(0) - 64), 0); }
}
