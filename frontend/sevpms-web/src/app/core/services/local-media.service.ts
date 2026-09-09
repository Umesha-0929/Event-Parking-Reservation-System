import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface UploadedLocalMedia { url: string; type: 'Photo' | 'Video' | string; fileName?: string; }
export interface PaymentQrInfo { userId?: string; url: string; }
export interface EventCoverInfo { url: string; }

@Injectable({ providedIn: 'root' })
export class LocalMediaService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiBaseUrl.replace(/\/$/, '');
  private api(path: string): string { return `${this.base}/${path.replace(/^\//, '')}`; }

  upload(file: File, category = 'general'): Observable<UploadedLocalMedia> {
    const body = new FormData(); body.append('file', file);
    return this.http.post<UploadedLocalMedia>(this.api(`media/uploads?category=${encodeURIComponent(category)}`), body);
  }
  uploadEventCover(eventId: string, file: File): Observable<EventCoverInfo> {
    const body = new FormData(); body.append('file', file);
    return this.http.post<EventCoverInfo>(this.api(`events/${eventId}/cover`), body);
  }
  eventCover(eventId: string): Observable<EventCoverInfo> { return this.http.get<EventCoverInfo>(this.api(`events/${eventId}/cover`)); }
  uploadMyPaymentQr(file: File): Observable<PaymentQrInfo> {
    const body = new FormData(); body.append('file', file);
    return this.http.post<PaymentQrInfo>(this.api('payment-qr/mine'), body);
  }
  myPaymentQr(): Observable<PaymentQrInfo> { return this.http.get<PaymentQrInfo>(this.api('payment-qr/mine')); }
  paymentQrForUser(userId: string): Observable<PaymentQrInfo> { return this.http.get<PaymentQrInfo>(this.api(`payment-qr/users/${userId}`)); }
}
