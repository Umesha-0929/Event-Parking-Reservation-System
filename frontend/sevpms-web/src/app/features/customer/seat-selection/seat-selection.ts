import { Component, OnDestroy, OnInit, computed, effect, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { PublishedSeatDto, PublishedSeatingLayoutDto, SeatViewAssetDto } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { RealtimeService } from '../../../core/services/realtime.service';
import { httpErrorMessage } from '../../../core/utils/http-error';
import { SeatMotionDirective } from '../../../shared/directives/seat-motion.directive';
import { SeatPanoramaComponent } from '../../../shared/components/seat-panorama/seat-panorama';

@Component({
  selector:'app-seat-selection',
  imports:[RouterLink, SeatMotionDirective, SeatPanoramaComponent],
  templateUrl:'./seat-selection.html',
  styleUrl:'./seat-selection.scss',
})
export class SeatSelectionComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly domain = inject(DomainApiService);
  private readonly realtime = inject(RealtimeService);
  readonly eventId = this.route.snapshot.queryParamMap.get('eventId') ?? '';
  readonly loading = signal(false);
  readonly error = signal('');
  readonly layout = signal<PublishedSeatingLayoutDto | null>(null);
  readonly selectedIds = signal<Set<string>>(new Set());
  readonly zoom = signal(1);
  readonly holdToken = signal('');
  readonly holdExpiresAt = signal<Date | null>(null);
  readonly holdSeconds = signal(0);
  readonly holding = signal(false);
  readonly creatingBooking = signal(false);
  readonly holdError = signal('');
  readonly previewSeat = signal<PublishedSeatDto | null>(null);
  readonly previewAsset = signal<SeatViewAssetDto | null>(null);
  readonly previewLoading = signal(false);
  readonly previewError = signal('');
  private timer?: ReturnType<typeof setInterval>;

  readonly selected = computed(() => this.layout()?.seats.filter((seat) => this.selectedIds().has(seat.seatId)) ?? []);
  readonly total = computed(() => this.selected().reduce((sum, seat) => sum + Number(seat.price ?? 0), 0));
  readonly stageStyle = computed<Record<string, string>>((): Record<string, string> => {
    const layout = this.layout();
    if (!layout) return {} as Record<string, string>;
    return {
      left: `${layout.stageX}px`,
      top: `${layout.stageY}px`,
      width: `${layout.stageWidth}px`,
      height: `${layout.stageHeight}px`,
    };
  });

  private readonly realtimeRefresh = effect(() => {
    const update = this.realtime.latestSeatUpdate();
    if (update && this.eventId && this.layout()) this.loadLayout(false);
  });

  ngOnInit(): void {
    if (!this.eventId) return;
    this.loadLayout(true);
    void this.realtime.joinEvent(this.eventId);
  }

  ngOnDestroy(): void {
    if (this.timer) clearInterval(this.timer);
    this.realtimeRefresh.destroy();
  }

  loadLayout(showLoading = true): void {
    if (!this.eventId) return;
    if (showLoading) this.loading.set(true);
    this.error.set('');
    this.domain.publishedLayout(this.eventId).subscribe({
      next: (layout) => { this.layout.set(layout); this.loading.set(false); this.removeInvalidSelections(layout); },
      error: (error) => { this.error.set(httpErrorMessage(error, 'The published seat layout could not be loaded.')); this.loading.set(false); },
    });
  }

  toggle(seat: PublishedSeatDto): void {
    if (!this.isSelectable(seat)) return;
    const next = new Set(this.selectedIds());
    if (next.has(seat.seatId)) next.delete(seat.seatId); else next.add(seat.seatId);
    this.selectedIds.set(next);
    if (this.holdToken()) this.releaseExistingHold(false);
  }

  isSelectable(seat: PublishedSeatDto): boolean {
    return this.selectedIds().has(seat.seatId) || seat.state.toLowerCase() === 'available';
  }

  seatClass(seat: PublishedSeatDto): string {
    if (this.selectedIds().has(seat.seatId)) return 'selected';
    return seat.state.toLowerCase();
  }

  seatStyle(seat: PublishedSeatDto): Record<string, string> { return { left: `${seat.x}px`, top: `${seat.y}px` }; }
  setZoom(delta:number):void{this.zoom.update((value)=>Math.min(1.6,Math.max(.6,Number((value+delta).toFixed(2)))));}

  holdSelection(): void {
    if (!this.eventId || !this.selected().length || this.holding()) return;
    this.holding.set(true); this.holdError.set('');
    const acquire = () => this.domain.holdSeats(this.eventId, { seatIds: this.selected().map((seat) => seat.seatId) }).subscribe({
      next: (result) => {
        this.holding.set(false);
        if (!result.succeeded || !result.hold) { this.holdError.set(result.message ?? 'One or more seats could not be held. Refresh the map and try again.'); this.loadLayout(false); return; }
        this.holdToken.set(result.hold.holdToken);
        this.holdExpiresAt.set(new Date(result.hold.expiresAtUtc));
        this.startTimer();
      },
      error: (error) => { this.holding.set(false); this.holdError.set(httpErrorMessage(error, 'Seats could not be held.')); this.loadLayout(false); },
    });
    if (this.holdToken()) {
      this.domain.releaseHold(this.holdToken()).subscribe({ next: () => { this.clearHoldState(); acquire(); }, error: () => { this.clearHoldState(); acquire(); } });
    } else acquire();
  }

  releaseExistingHold(clearSelection = false): void {
    const token = this.holdToken();
    if (!token) return;
    this.domain.releaseHold(token).subscribe({ next: () => this.clearHoldState(), error: () => this.clearHoldState() });
    if (clearSelection) this.selectedIds.set(new Set());
  }

  continueToPayment(): void {
    if (!this.holdToken()) { this.holdSelection(); return; }
    if (this.creatingBooking()) return;
    this.creatingBooking.set(true); this.holdError.set('');
    this.domain.createBooking({ eventId: this.eventId, holdToken: this.holdToken(), seatIds: this.selected().map((seat) => seat.seatId) }).subscribe({
      next: (booking) => {
        this.creatingBooking.set(false);
        const bookingId = booking.bookingId ?? booking.id;
        if (!bookingId) { this.holdError.set('The booking response did not include a booking identifier.'); return; }
        void this.router.navigate(['/customer/payment'], { queryParams: { bookingId, amount: booking.totalAmount ?? this.total() } });
      },
      error: (error) => { this.creatingBooking.set(false); this.holdError.set(httpErrorMessage(error, 'The booking could not be created before the hold expires.')); this.loadLayout(false); },
    });
  }

  openPreview(seat: PublishedSeatDto): void {
    this.previewSeat.set(seat); this.previewAsset.set(null); this.previewError.set(''); this.previewLoading.set(true);
    this.domain.seatView(this.eventId, seat.seatId).subscribe({
      next: (asset) => { this.previewAsset.set(asset); this.previewLoading.set(false); },
      error: (error) => { this.previewError.set(error?.status === 404 ? 'A 360° view has not been provided for this seat.' : httpErrorMessage(error, 'The seat view could not be loaded.')); this.previewLoading.set(false); },
    });
  }
  closePreview(): void { this.previewSeat.set(null); this.previewAsset.set(null); this.previewError.set(''); }

  private startTimer(): void {
    if (this.timer) clearInterval(this.timer);
    const update = () => {
      const expiry = this.holdExpiresAt();
      const seconds = expiry ? Math.max(0, Math.ceil((expiry.getTime() - Date.now()) / 1000)) : 0;
      this.holdSeconds.set(seconds);
      if (seconds === 0 && this.holdToken()) { this.clearHoldState(); this.selectedIds.set(new Set()); this.loadLayout(false); }
    };
    update(); this.timer = setInterval(update, 1000);
  }

  private clearHoldState(): void { this.holdToken.set(''); this.holdExpiresAt.set(null); this.holdSeconds.set(0); if (this.timer) { clearInterval(this.timer); this.timer = undefined; } }
  private removeInvalidSelections(layout: PublishedSeatingLayoutDto): void {
    const available = new Set(layout.seats.filter((seat) => seat.state.toLowerCase() === 'available').map((seat) => seat.seatId));
    this.selectedIds.set(new Set([...this.selectedIds()].filter((id) => available.has(id))));
  }
}
