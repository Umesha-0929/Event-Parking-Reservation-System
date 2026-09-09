import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ManualPaymentReviewDto } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';

@Component({
  selector: 'app-payment-review',
  imports: [CommonModule],
  templateUrl: './payment-review.html',
  styleUrl: './payment-review.scss',
})
export class PaymentReviewComponent implements OnInit {
  private readonly domain = inject(DomainApiService);
  readonly items = signal<ManualPaymentReviewDto[]>([]);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly message = signal('');

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true); this.error.set('');
    this.domain.manualPaymentReviews().subscribe({
      next: (items) => { this.items.set(items); this.loading.set(false); },
      error: (e) => { this.error.set(httpErrorMessage(e, 'Payment proofs could not be loaded.')); this.loading.set(false); },
    });
  }

  approve(item: ManualPaymentReviewDto): void {
    this.loading.set(true); this.error.set(''); this.message.set('');
    this.domain.approveManualPayment(item.paymentId).subscribe({
      next: () => { this.message.set('Payment approved. Booking confirmation, tickets and receipt can now be issued by the normal payment completion flow.'); this.load(); },
      error: (e) => { this.error.set(httpErrorMessage(e, 'Payment proof could not be approved.')); this.loading.set(false); },
    });
  }

  reject(item: ManualPaymentReviewDto): void {
    this.loading.set(true); this.error.set(''); this.message.set('');
    this.domain.rejectManualPayment(item.paymentId).subscribe({
      next: () => { this.message.set('Payment proof rejected. The customer has been notified to try again.'); this.load(); },
      error: (e) => { this.error.set(httpErrorMessage(e, 'Payment proof could not be rejected.')); this.loading.set(false); },
    });
  }
}
