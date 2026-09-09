import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { catchError, map, of, switchMap } from 'rxjs';
import { BookingSummary, EventSummary, PayHereCheckoutDto, PaymentResponseDto } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { LocalMediaService } from '../../../core/services/local-media.service';
import { httpErrorMessage } from '../../../core/utils/http-error';

@Component({
  selector: 'app-payment',
  imports: [RouterLink, FormsModule],
  templateUrl: './payment.html',
  styleUrl: './payment.scss',
})
export class PaymentComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly domain = inject(DomainApiService);
  private readonly media = inject(LocalMediaService);

  readonly bookingId = this.route.snapshot.queryParamMap.get('bookingId') ?? '';
  readonly booking = signal<BookingSummary | null>(null);
  readonly event = signal<EventSummary | null>(null);
  readonly organizerQr = signal('');
  readonly payment = signal<PaymentResponseDto | null>(null);
  readonly checkout = signal<PayHereCheckoutDto | null>(null);
  readonly proofFile = signal<File | null>(null);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly message = signal('');
  proofReference = '';

  ngOnInit(): void {
    if (!this.bookingId) return;
    this.loading.set(true);
    this.domain.booking(this.bookingId).subscribe({
      next: (booking) => {
        this.booking.set(booking);
        this.loading.set(false);
        if (!booking.eventId) return;
        this.domain.event(booking.eventId).subscribe({
          next: (event) => {
            this.event.set(event);
            if (!event.organizerUserId) return;
            this.media.paymentQrForUser(event.organizerUserId)
              .pipe(catchError(() => of(null)))
              .subscribe((qr) => this.organizerQr.set(qr?.url ?? ''));
          },
        });
      },
      error: (e) => {
        this.error.set(httpErrorMessage(e, 'The booking could not be loaded.'));
        this.loading.set(false);
      },
    });
  }

  startPayment(): void {
    if (!this.bookingId || this.loading()) return;
    this.loading.set(true); this.error.set(''); this.message.set('');
    this.ensurePayment().pipe(
      switchMap((payment) => this.domain.payHereCheckout(payment.paymentId)),
    ).subscribe({
      next: (checkout) => { this.checkout.set(checkout); this.loading.set(false); },
      error: (e) => {
        this.loading.set(false);
        this.error.set(httpErrorMessage(e, 'The online payment provider is not configured. You can still use the Organizer payment QR when available.'));
      },
    });
  }

  submitPayHere(): void {
    const checkout = this.checkout();
    if (!checkout || typeof document === 'undefined') return;
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = checkout.checkoutUrl;
    const fields: Record<string, string> = {
      merchant_id: checkout.merchantId,
      return_url: checkout.returnUrl,
      cancel_url: checkout.cancelUrl,
      notify_url: checkout.notifyUrl,
      first_name: checkout.firstName,
      last_name: checkout.lastName,
      email: checkout.email,
      phone: checkout.phone,
      address: checkout.address,
      city: checkout.city,
      country: checkout.country,
      order_id: checkout.orderId,
      items: checkout.items,
      currency: checkout.currency,
      amount: checkout.amount,
      hash: checkout.hash,
    };
    for (const [name, value] of Object.entries(fields)) {
      const input = document.createElement('input');
      input.type = 'hidden'; input.name = name; input.value = value ?? '';
      form.appendChild(input);
    }
    document.body.appendChild(form);
    form.submit();
  }

  onProofSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.proofFile.set(input.files?.[0] ?? null);
  }

  submitProof(): void {
    const file = this.proofFile();
    if (!file) { this.error.set('Choose a screenshot or image of the completed payment first.'); return; }
    if (!this.bookingId || this.loading()) return;
    this.loading.set(true); this.error.set(''); this.message.set('');
    this.ensurePayment().pipe(
      switchMap((payment) => this.media.upload(file, 'payment-proof').pipe(map((uploaded) => ({ payment, uploaded })))),
      switchMap(({ payment, uploaded }) => this.domain.submitManualPaymentProof(payment.paymentId, {
        proofUrl: uploaded.url,
        reference: this.proofReference.trim() || null,
      })),
    ).subscribe({
      next: (payment) => {
        this.payment.set(payment);
        this.loading.set(false);
        this.message.set('Payment proof submitted. The Event Organizer will approve or reject it before tickets are issued.');
      },
      error: (e) => {
        this.loading.set(false);
        this.error.set(httpErrorMessage(e, 'Payment proof could not be submitted.'));
      },
    });
  }

  statusText(value: unknown): string {
    const n = Number(value);
    if (n === 0) return 'Pending';
    if (n === 1) return 'Confirmed';
    if (n === 2) return 'Cancelled';
    return String(value ?? 'Pending');
  }

  private ensurePayment() {
    const existing = this.payment();
    return existing ? of(existing) : this.domain.createPayment({ bookingId: this.bookingId }).pipe(map((p) => { this.payment.set(p); return p; }));
  }
}
