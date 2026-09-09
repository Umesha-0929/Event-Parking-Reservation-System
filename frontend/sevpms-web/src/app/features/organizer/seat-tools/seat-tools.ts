import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { catchError, forkJoin, from, mergeMap, of, switchMap, toArray } from 'rxjs';
import { SeatingLayoutDto } from '../../../core/models/api.models';
import {
  KlegarSeatAvailabilityDto,
  KlegarSeatViewAssetDto,
  SeatViewTargetLevel,
} from '../../../core/models/klegar-seat-ticket.models';
import { KlegarSeatTicketApiService } from '../../../core/services/klegar-seat-ticket-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';
import { SeatPanoramaComponent } from '../../../shared/components/seat-panorama/seat-panorama';
import { MediaDropzoneComponent } from '../../../shared/components/media-dropzone/media-dropzone';
import { UploadedLocalMedia } from '../../../core/services/local-media.service';
import { stagePatternPositions, stageTemplateGeometry } from '../../../shared/utils/stage-seat-patterns';

@Component({
  selector: 'app-organizer-seat-tools',
  imports: [FormsModule, RouterLink, SeatPanoramaComponent, MediaDropzoneComponent],
  templateUrl: './seat-tools.html',
  styleUrl: './seat-tools.scss',
})
export class OrganizerSeatToolsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(KlegarSeatTicketApiService);

  readonly eventId = this.route.snapshot.paramMap.get('eventId') ?? '';
  readonly loading = signal(true);
  readonly savingSeat = signal(false);
  readonly applyingPattern = signal(false);
  readonly savingView = signal(false);
  readonly checkingView = signal(false);
  readonly error = signal('');
  readonly notice = signal('');
  readonly layout = signal<SeatingLayoutDto | null>(null);
  readonly seats = signal<KlegarSeatAvailabilityDto[]>([]);
  readonly selectedSeatId = signal('');
  readonly savedView = signal<KlegarSeatViewAssetDto | null>(null);
  readonly resolvedView = signal<KlegarSeatViewAssetDto | null>(null);
  readonly viewLookupMessage = signal('');
  readonly draggingSeatId = signal('');

  private dragPointerId: number | null = null;
  private dragOffsetX = 0;
  private dragOffsetY = 0;
  private dragMoved = false;
  private suppressSeatClick = false;

  sectionFilter = '';
  seatX = 0;
  seatY = 0;
  seatAccessible = false;
  seatStatus: 'Available' | 'Blocked' = 'Available';
  nudgeStep = 5;

  viewLevel: SeatViewTargetLevel = 'section';
  viewSectionId = '';
  viewRowLabel = '';
  viewSeatId = '';
  mediaUrl = '';
  viewerType = 'panorama';
  defaultYaw: number | null = 0;
  defaultPitch: number | null = 0;
  defaultFov: number | null = 90;
  isRepresentative = true;
  previewMedia = signal('');

  readonly selectedSeat = computed(() =>
    this.seats().find((seat) => seat.seatId === this.selectedSeatId()) ?? null,
  );

  readonly editable = computed(() => !this.layout()?.isPublished);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    if (!this.eventId) {
      this.error.set('Event context is missing. Open Seat Tools from an organizer event.');
      this.loading.set(false);
      return;
    }

    this.loading.set(true);
    this.error.set('');
    forkJoin({
      layout: this.api.organizerLayout(this.eventId),
      seats: this.api.seats(this.eventId),
    }).subscribe({
      next: ({ layout, seats }) => {
        this.layout.set(layout);
        this.seats.set(seats);
        const firstSection = layout.sections.find((section) => section.isEnabled) ?? layout.sections[0];
        this.sectionFilter = firstSection?.id ?? '';
        this.viewSectionId = firstSection?.id ?? '';
        this.syncViewDefaults();
        const firstSeat = seats.find((seat) => !this.isLockedSeat(seat)) ?? seats[0];
        if (firstSeat) this.selectSeat(firstSeat);
        this.loading.set(false);
        if (this.shouldAutoArrange(layout, seats)) {
          this.notice.set(`Applying the ${this.stageTypeName()} layout to the existing generated seats…`);
          setTimeout(() => this.applyStagePattern(), 0);
        }
      },
      error: (error) => {
        this.error.set(httpErrorMessage(error, 'Seat tools could not be loaded.'));
        this.loading.set(false);
      },
    });
  }

  selectSection(sectionId: string): void {
    this.sectionFilter = sectionId;
    const firstSeat = this.seats().find((seat) => !sectionId || seat.sectionId === sectionId);
    if (firstSeat) this.selectSeat(firstSeat);
  }

  selectSeat(seat: KlegarSeatAvailabilityDto): void {
    this.selectedSeatId.set(seat.seatId);
    this.seatX = Number(seat.x);
    this.seatY = Number(seat.y);
    this.seatAccessible = !!seat.isAccessible;
    this.seatStatus = this.normalizedState(seat.state) === 'Blocked' ? 'Blocked' : 'Available';
  }

  startDrag(event: PointerEvent, seat: KlegarSeatAvailabilityDto): void {
    this.selectSeat(seat);
    if (!this.canEditSeat(seat)) return;
    const layout = this.layout();
    const target = event.currentTarget as HTMLElement | null;
    const canvas = target?.parentElement;
    if (!layout || !target || !canvas) return;

    const rect = canvas.getBoundingClientRect();
    const scaleX = layout.canvasWidth / Math.max(rect.width, 1);
    const scaleY = layout.canvasHeight / Math.max(rect.height, 1);
    const pointerX = (event.clientX - rect.left) * scaleX;
    const pointerY = (event.clientY - rect.top) * scaleY;

    this.dragPointerId = event.pointerId;
    this.dragOffsetX = pointerX - this.seatX;
    this.dragOffsetY = pointerY - this.seatY;
    this.draggingSeatId.set(seat.seatId);
    this.dragMoved = false;
    target.setPointerCapture?.(event.pointerId);
    event.preventDefault();
  }

  dragSeat(event: PointerEvent, seat: KlegarSeatAvailabilityDto): void {
    if (this.dragPointerId !== event.pointerId || this.draggingSeatId() !== seat.seatId) return;
    const layout = this.layout();
    const target = event.currentTarget as HTMLElement | null;
    const canvas = target?.parentElement;
    if (!layout || !target || !canvas || !this.canEditSeat(seat)) return;

    const rect = canvas.getBoundingClientRect();
    const scaleX = layout.canvasWidth / Math.max(rect.width, 1);
    const scaleY = layout.canvasHeight / Math.max(rect.height, 1);
    const pointerX = (event.clientX - rect.left) * scaleX;
    const pointerY = (event.clientY - rect.top) * scaleY;

    this.seatX = this.clamp(Number((pointerX - this.dragOffsetX).toFixed(2)), 18, layout.canvasWidth - 18);
    this.seatY = this.clamp(Number((pointerY - this.dragOffsetY).toFixed(2)), 18, layout.canvasHeight - 18);
    this.dragMoved = true;
    event.preventDefault();
  }

  endDrag(event: PointerEvent): void {
    if (this.dragPointerId !== event.pointerId) return;
    const target = event.currentTarget as HTMLElement | null;
    if (target?.hasPointerCapture?.(event.pointerId)) target.releasePointerCapture(event.pointerId);
    this.dragPointerId = null;
    const movedId = this.draggingSeatId();
    this.draggingSeatId.set('');
    if (this.dragMoved && movedId) {
      this.seats.update((items) => items.map((item) => item.seatId === movedId ? { ...item, x: this.seatX, y: this.seatY } : item));
      this.suppressSeatClick = true;
      setTimeout(() => this.suppressSeatClick = false, 0);
      this.notice.set('Seat moved on the canvas. Use “Save seat position” to store the new position.');
    }
    this.dragMoved = false;
    event.preventDefault();
  }

  seatClick(seat: KlegarSeatAvailabilityDto): void {
    if (this.suppressSeatClick) return;
    this.selectSeat(seat);
  }

  nudge(dx: number, dy: number): void {
    const seat = this.selectedSeat();
    const layout = this.layout();
    if (!seat || !layout || !this.canEditSeat(seat)) return;
    this.seatX = this.clamp(Number((this.seatX + dx).toFixed(2)), 18, layout.canvasWidth - 18);
    this.seatY = this.clamp(Number((this.seatY + dy).toFixed(2)), 18, layout.canvasHeight - 18);
  }

  normalizePosition(): void {
    const layout = this.layout();
    if (!layout) return;
    this.seatX = this.clamp(Number(this.seatX) || 18, 18, layout.canvasWidth - 18);
    this.seatY = this.clamp(Number(this.seatY) || 18, 18, layout.canvasHeight - 18);
  }

  saveSelectedSeat(): void {
    const seat = this.selectedSeat();
    if (!seat || !this.canEditSeat(seat) || this.savingSeat()) return;
    this.normalizePosition();
    this.savingSeat.set(true);
    this.error.set('');
    this.notice.set('');

    this.api.seatView(this.eventId, seat.seatId).pipe(
      catchError(() => of(null)),
      switchMap((asset) => this.api.saveSeat(this.eventId, {
        seatId: seat.seatId,
        sectionId: seat.sectionId,
        rowLabel: seat.rowLabel,
        seatNumber: seat.seatNumber,
        x: Number(this.seatX),
        y: Number(this.seatY),
        ticketTypeId: seat.ticketTypeId ?? null,
        isAccessible: this.seatAccessible,
        status: this.seatStatus,
        seatViewAssetId: asset?.seatId === seat.seatId ? asset.id : null,
      })),
    ).subscribe({
      next: (saved) => {
        this.seats.update((items) => items.map((item) => item.seatId === saved.seatId ? saved : item));
        this.selectSeat(saved);
        this.notice.set(`Seat ${saved.rowLabel}${saved.seatNumber} position saved.`);
        this.savingSeat.set(false);
      },
      error: (error) => {
        this.error.set(httpErrorMessage(error, 'The seat could not be updated.'));
        this.savingSeat.set(false);
      },
    });
  }

  applyStagePattern(): void {
    const layout = this.layout();
    if (!layout || !this.editable() || this.applyingPattern()) return;
    const movable = this.seats().filter((seat) => !this.isLockedSeat(seat));
    if (!movable.length) {
      this.error.set('There are no editable generated seats to arrange.');
      return;
    }

    const stageType = Number(layout.stageType) || 2;
    const geometry = stageTemplateGeometry(stageType);
    const targets = stagePatternPositions(movable, stageType, layout.canvasWidth, layout.canvasHeight);
    const positions = new Map(targets.map((item) => [item.seat.seatId, item]));

    this.applyingPattern.set(true);
    this.error.set('');
    this.notice.set('');

    this.api.saveLayout(this.eventId, {
      stageType,
      rowCount: layout.rowCount,
      columnCount: layout.columnCount,
      canvasWidth: layout.canvasWidth,
      canvasHeight: layout.canvasHeight,
      stageX: geometry.x,
      stageY: geometry.y,
      stageWidth: geometry.w,
      stageHeight: geometry.h,
    }).pipe(
      switchMap((savedLayout) => {
        this.layout.set(savedLayout);
        return from(movable).pipe(
          mergeMap((seat) => {
            const target = positions.get(seat.seatId)!;
            return this.api.seatView(this.eventId, seat.seatId).pipe(
              catchError(() => of(null)),
              switchMap((asset) => this.api.saveSeat(this.eventId, {
                seatId: seat.seatId,
                sectionId: seat.sectionId,
                rowLabel: seat.rowLabel,
                seatNumber: seat.seatNumber,
                x: target.x,
                y: target.y,
                ticketTypeId: seat.ticketTypeId ?? null,
                isAccessible: seat.isAccessible,
                status: this.normalizedState(seat.state) === 'Blocked' ? 'Blocked' : 'Available',
                seatViewAssetId: asset?.seatId === seat.seatId ? asset.id : null,
              })),
            );
          }, 6),
          toArray(),
        );
      }),
    ).subscribe({
      next: (updatedSeats) => {
        const byId = new Map(updatedSeats.map((seat) => [seat.seatId, seat]));
        this.seats.update((items) => items.map((seat) => byId.get(seat.seatId) ?? seat));
        const selected = this.selectedSeat();
        if (selected) this.selectSeat(byId.get(selected.seatId) ?? selected);
        this.notice.set(`${this.stageTypeName()} seating pattern applied. The customer will see these saved positions after publishing.`);
        this.applyingPattern.set(false);
      },
      error: (error) => {
        this.error.set(httpErrorMessage(error, 'The stage-specific seating pattern could not be applied.'));
        this.applyingPattern.set(false);
      },
    });
  }

  onViewLevelChanged(): void {
    this.savedView.set(null);
    this.resolvedView.set(null);
    this.viewLookupMessage.set('');
    this.isRepresentative = this.viewLevel !== 'seat';
    this.syncViewDefaults();
  }

  onViewSectionChanged(): void {
    this.viewRowLabel = '';
    this.viewSeatId = '';
    this.syncViewDefaults();
  }

  onViewMediaUploaded(media: UploadedLocalMedia): void {
    if (media.type === 'Video') { this.error.set('Use an equirectangular panorama image for the 360° seat viewer.'); return; }
    this.mediaUrl = media.url; this.previewMedia.set(media.url); this.notice.set('Panorama image uploaded. Save the 360° view mapping to attach it.');
  }

  previewEnteredMedia(): void {
    this.previewMedia.set(this.mediaUrl.trim());
  }

  saveViewAsset(): void {
    if (!this.editable() || this.savingView()) return;
    const mediaUrl = this.mediaUrl.trim();
    if (!mediaUrl) {
      this.error.set('Add a real 360 panorama URL first. No fake panorama is generated.');
      return;
    }
    const target = this.viewTarget();
    if (!target) return;

    this.savingView.set(true);
    this.error.set('');
    this.notice.set('');
    this.api.saveSeatViewAsset(this.eventId, {
      id: null,
      sectionId: target.sectionId,
      rowLabel: target.rowLabel,
      seatId: target.seatId,
      mediaUrl,
      viewerType: this.viewerType.trim() || 'panorama',
      defaultYaw: this.nullableNumber(this.defaultYaw),
      defaultPitch: this.nullableNumber(this.defaultPitch),
      defaultFov: this.nullableNumber(this.defaultFov),
      isRepresentative: this.viewLevel === 'seat' ? false : this.isRepresentative,
    }).subscribe({
      next: (asset) => {
        this.savedView.set(asset);
        this.previewMedia.set(asset.mediaUrl);
        this.notice.set(`360° view saved for ${this.targetLabel()}.`);
        this.savingView.set(false);
        this.verifyFallbackAfterSave(asset);
      },
      error: (error) => {
        this.error.set(httpErrorMessage(error, 'The 360° view could not be saved.'));
        this.savingView.set(false);
      },
    });
  }

  verifySelectedSeatView(): void {
    const seatId = this.lookupSeatId();
    if (!seatId) {
      this.viewLookupMessage.set('Choose a section, row or seat that contains at least one generated seat.');
      return;
    }
    this.checkingView.set(true);
    this.viewLookupMessage.set('');
    this.resolvedView.set(null);
    this.api.seatView(this.eventId, seatId).subscribe({
      next: (asset) => {
        this.resolvedView.set(asset);
        this.previewMedia.set(asset.mediaUrl);
        this.viewLookupMessage.set(`Customer preview is using the saved ${this.resolutionLevel(asset)} view.`);
        this.checkingView.set(false);
      },
      error: (error) => {
        this.viewLookupMessage.set(error?.status === 404
          ? 'No 360° preview is available for this seating area yet.'
          : httpErrorMessage(error, 'The customer preview could not be loaded.'));
        this.checkingView.set(false);
      },
    });
  }

  canEditSeat(seat: KlegarSeatAvailabilityDto): boolean {
    return this.editable() && !this.isLockedSeat(seat);
  }

  isLockedSeat(seat: KlegarSeatAvailabilityDto): boolean {
    const state = this.normalizedState(seat.state);
    return state === 'Held' || state === 'Booked';
  }

  stateLabel(state: string): string {
    const normalized = this.normalizedState(state);
    return normalized === 'Blocked' ? 'Unavailable' : normalized;
  }

  seatClass(seat: KlegarSeatAvailabilityDto): string {
    const state = this.normalizedState(seat.state).toLowerCase();
    return state === 'blocked' ? 'unavailable' : state;
  }

  seatStyle(seat: KlegarSeatAvailabilityDto): Record<string, string> {
    if (seat.seatId === this.selectedSeatId()) {
      return { left: `${this.seatX}px`, top: `${this.seatY}px` };
    }
    return { left: `${seat.x}px`, top: `${seat.y}px` };
  }

  stageStyle(): Record<string, string> {
    const layout = this.layout();
    if (!layout) return {};
    return {
      left: `${layout.stageX}px`, top: `${layout.stageY}px`,
      width: `${layout.stageWidth}px`, height: `${layout.stageHeight}px`,
    };
  }

  stageClass(): string {
    const type = Number(this.layout()?.stageType) || 2;
    return ['arena', 'proscenium', 'end-on', 'thrust', 'traverse', 'in-round'][type - 1] ?? 'proscenium';
  }

  stageTypeName(): string {
    const type = Number(this.layout()?.stageType) || 2;
    return ['Arena Stage', 'Proscenium Theatre Stage', 'End-On Stage', 'Thrust Stage', 'Traverse Stage', 'In-the-Round Stage'][type - 1] ?? 'Stage';
  }

  sectionName(sectionId: string): string {
    return this.layout()?.sections.find((section) => section.id === sectionId)?.name ?? 'Section';
  }

  visibleSeats(): KlegarSeatAvailabilityDto[] {
    return this.sectionFilter ? this.seats().filter((seat) => seat.sectionId === this.sectionFilter) : this.seats();
  }

  viewSectionSeats(): KlegarSeatAvailabilityDto[] {
    return this.viewSectionId ? this.seats().filter((seat) => seat.sectionId === this.viewSectionId) : [];
  }

  viewRows(): string[] {
    const rows = new Set(this.viewSectionSeats().map((seat) => seat.rowLabel).filter(Boolean));
    return [...rows].sort((a, b) => a.localeCompare(b, undefined, { numeric: true }));
  }

  targetLabel(): string {
    if (this.viewLevel === 'seat') {
      const seat = this.seats().find((item) => item.seatId === this.viewSeatId);
      return seat ? `Seat ${seat.rowLabel}${seat.seatNumber}` : 'Seat';
    }
    if (this.viewLevel === 'row') return `Row ${this.viewRowLabel || '—'} · ${this.sectionName(this.viewSectionId)}`;
    return this.sectionName(this.viewSectionId);
  }

  syncViewDefaults(): void {
    const seats = this.viewSectionSeats();
    const rows = this.viewRows();
    if (!this.viewRowLabel || !rows.includes(this.viewRowLabel)) this.viewRowLabel = rows[0] ?? '';
    const matchingSeats = this.viewLevel === 'row' ? seats.filter((seat) => seat.rowLabel === this.viewRowLabel) : seats;
    if (!matchingSeats.some((seat) => seat.seatId === this.viewSeatId)) this.viewSeatId = matchingSeats[0]?.seatId ?? '';
  }

  private viewTarget(): { sectionId: string | null; rowLabel: string | null; seatId: string | null } | null {
    if (!this.viewSectionId) {
      this.error.set('Choose a seating section for the 360° view.');
      return null;
    }
    if (this.viewLevel === 'section') return { sectionId: this.viewSectionId, rowLabel: null, seatId: null };
    if (this.viewLevel === 'row') {
      if (!this.viewRowLabel) { this.error.set('Choose a row for the 360° view.'); return null; }
      return { sectionId: this.viewSectionId, rowLabel: this.viewRowLabel, seatId: null };
    }
    if (!this.viewSeatId) { this.error.set('Choose an individual seat for the 360° view.'); return null; }
    const seat = this.seats().find((item) => item.seatId === this.viewSeatId);
    return { sectionId: seat?.sectionId ?? this.viewSectionId, rowLabel: seat?.rowLabel ?? null, seatId: this.viewSeatId };
  }

  private lookupSeatId(): string {
    if (this.viewLevel === 'seat') return this.viewSeatId;
    const seats = this.viewSectionSeats();
    if (this.viewLevel === 'row') return seats.find((seat) => seat.rowLabel === this.viewRowLabel)?.seatId ?? '';
    return seats[0]?.seatId ?? '';
  }

  private verifyFallbackAfterSave(asset: KlegarSeatViewAssetDto): void {
    const seatId = this.lookupSeatId();
    if (!seatId) { this.resolvedView.set(asset); return; }
    this.api.seatView(this.eventId, seatId).subscribe({
      next: (resolved) => {
        this.resolvedView.set(resolved);
        this.viewLookupMessage.set(`Saved. Customer preview resolves from the ${this.resolutionLevel(resolved)} level.`);
      },
      error: () => undefined,
    });
  }

  private resolutionLevel(asset: KlegarSeatViewAssetDto): string {
    if (asset.seatId) return 'seat';
    if (asset.rowLabel) return 'row';
    return 'section';
  }

  private normalizedState(state: string): 'Available' | 'Held' | 'Booked' | 'Blocked' {
    const normalized = String(state ?? '').trim().toLowerCase();
    if (normalized === 'held') return 'Held';
    if (normalized === 'booked') return 'Booked';
    if (normalized === 'blocked' || normalized === 'unavailable') return 'Blocked';
    return 'Available';
  }

  private nullableNumber(value: number | null): number | null {
    if (value === null || value === undefined) return null;
    const result = Number(value);
    return Number.isFinite(result) ? result : null;
  }

  private clamp(value: number, min: number, max: number): number {
    return Math.max(min, Math.min(max, value));
  }

  private shouldAutoArrange(layout: SeatingLayoutDto, seats: KlegarSeatAvailabilityDto[]): boolean {
    if (layout.isPublished || !seats.length || seats.some((seat) => this.isLockedSeat(seat))) return false;
    const editableSeats = seats.filter((seat) => !this.isLockedSeat(seat));
    const geometry = stageTemplateGeometry(Number(layout.stageType) || 2);
    const margin = 20;
    const overlaps = editableSeats.filter((seat) =>
      Number(seat.x) >= geometry.x - margin && Number(seat.x) <= geometry.x + geometry.w + margin &&
      Number(seat.y) >= geometry.y - margin && Number(seat.y) <= geometry.y + geometry.h + margin).length;
    const threshold = Math.max(2, Math.ceil(editableSeats.length * 0.08));
    return overlaps >= threshold;
  }
}
