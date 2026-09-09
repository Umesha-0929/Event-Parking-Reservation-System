import { Component, ElementRef, OnDestroy, OnInit, ViewChild, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BrowserQRCodeReader, IScannerControls } from '@zxing/browser';
import { EventSummary } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { SessionService } from '../../../core/services/session.service';
import { httpErrorMessage } from '../../../core/utils/http-error';

interface CheckInResult { succeeded?: boolean; result?: string; message?: string; ticketId?: string; ticketNo?: string; scannedAtUtc?: string; }
type CheckInTone = 'success' | 'warning' | 'danger' | 'neutral';

@Component({ selector: 'app-qr-checkin', imports: [FormsModule], templateUrl: './qr-checkin.html', styleUrl: './qr-checkin.scss' })
export class QrCheckinComponent implements OnInit, OnDestroy {
  @ViewChild('camera') camera?: ElementRef<HTMLVideoElement>;
  private readonly domain = inject(DomainApiService);
  private readonly session = inject(SessionService);
  readonly events = signal<EventSummary[]>([]);
  readonly state = signal<'idle' | 'requesting' | 'camera' | 'verifying' | 'denied' | 'unsupported' | 'result'>('idle');
  readonly result = signal<CheckInResult | null>(null);
  readonly error = signal('');
  readonly lastPayload = signal('');
  eventId = '';
  gate = 'Main Gate';
  private reader?: BrowserQRCodeReader;
  private controls?: IScannerControls;

  ngOnInit(): void {
    const source = this.session.role() === 'EventOrganizer' ? this.domain.organizerEvents() : this.domain.events();
    source.subscribe({
      next: (events) => { this.events.set(events); if (!this.eventId && events.length === 1) this.eventId = events[0].eventId ?? events[0].id ?? ''; },
      error: (e) => this.error.set(httpErrorMessage(e, 'Events could not be loaded for check-in.')),
    });
  }

  async startScanner(): Promise<void> {
    if (!navigator.mediaDevices?.getUserMedia) { this.state.set('unsupported'); return; }
    if (!this.eventId.trim()) { this.error.set('Choose an event before starting the scanner.'); return; }
    this.error.set(''); this.result.set(null); this.state.set('requesting');
    try {
      this.reader = new BrowserQRCodeReader();
      await new Promise<void>((resolve) => queueMicrotask(resolve));
      const video = this.camera?.nativeElement;
      if (!video) { this.state.set('idle'); this.error.set('Camera view was not ready. Try again.'); return; }
      this.controls = await this.reader.decodeFromVideoDevice(undefined, video, (decoded) => {
        if (decoded && this.state() === 'camera') { const text = decoded.getText(); this.lastPayload.set(text); this.stopCamera(false); this.verify(text); }
      });
      this.state.set('camera');
    } catch (error) {
      this.state.set('denied');
      this.error.set(error instanceof Error ? error.message : 'Camera permission was denied or no camera is available.');
    }
  }

  verify(payload: string): void {
    this.state.set('verifying'); this.error.set('');
    this.domain.scanTicket(this.eventId, { qrPayload: payload, gate: this.gate }).subscribe({
      next: (response) => { this.result.set(response as CheckInResult); this.state.set('result'); },
      error: (error) => { const body = (error?.error ?? {}) as CheckInResult; this.result.set(body); this.error.set(body.message ?? httpErrorMessage(error, 'Ticket verification failed.')); this.state.set('result'); },
    });
  }

  scanNext(): void { this.result.set(null); this.error.set(''); this.lastPayload.set(''); this.state.set('idle'); void this.startScanner(); }
  stopCamera(reset = true): void { this.controls?.stop(); this.controls = undefined; if (reset) this.state.set('idle'); }
  resultKey(): string { return String(this.result()?.result ?? '').trim().toLowerCase(); }
  resultTone(): CheckInTone { const key=this.resultKey(); if(key==='accepted')return'success'; if(key==='duplicate'||key==='wrongevent'||key==='wrong_event')return'warning'; if(key==='cancelled'||key==='voided'||key==='invalid')return'danger'; return this.result()?.succeeded?'success':'neutral'; }
  resultTitle(): string { const key=this.resultKey(); if(key==='accepted')return'Entry accepted'; if(key==='duplicate')return'Duplicate scan'; if(key==='wrongevent'||key==='wrong_event')return'Wrong event'; if(key==='cancelled')return'Ticket cancelled'; if(key==='voided')return'Ticket voided'; if(key==='invalid')return'Invalid ticket'; return this.result()?.succeeded?'Entry approved':'Entry not approved'; }
  ngOnDestroy(): void { this.controls?.stop(); }
}
