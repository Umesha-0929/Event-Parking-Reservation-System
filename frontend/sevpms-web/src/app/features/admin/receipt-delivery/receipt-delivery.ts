import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BackendCoverageApiService, ReceiptDeliveryDto, ReceiptDto } from '../../../core/services/backend-coverage-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';

@Component({ selector: 'app-admin-receipt-delivery', imports: [FormsModule], templateUrl: './receipt-delivery.html', styleUrl: './receipt-delivery.scss' })
export class AdminReceiptDeliveryComponent implements OnInit {
  private readonly api = inject(BackendCoverageApiService);
  readonly receipts = signal<ReceiptDto[]>([]);
  receiptId = '';
  readonly deliveries = signal<ReceiptDeliveryDto[]>([]);
  readonly busy = signal(false);
  readonly error = signal('');
  readonly message = signal('');

  ngOnInit(): void { this.refreshReceipts(); }

  refreshReceipts(): void {
    this.busy.set(true); this.error.set('');
    this.api.adminReceipts().subscribe({
      next: (items) => { this.receipts.set(items ?? []); this.busy.set(false); if (!this.receiptId && items?.length) { this.receiptId = items[0].receiptId; this.load(); } },
      error: (e) => { this.error.set(httpErrorMessage(e, 'Receipts could not be loaded.')); this.busy.set(false); },
    });
  }

  load(): void {
    const id = this.receiptId.trim();
    if (!id) { this.deliveries.set([]); return; }
    this.error.set(''); this.message.set(''); this.busy.set(true);
    this.api.adminReceiptDeliveries(id).subscribe({
      next: (items) => { this.deliveries.set(items ?? []); this.busy.set(false); },
      error: (e) => { this.error.set(httpErrorMessage(e, 'Receipt delivery history could not be loaded.')); this.busy.set(false); },
    });
  }

  retry(): void {
    const id = this.receiptId.trim();
    if (!id) return;
    this.error.set(''); this.message.set(''); this.busy.set(true);
    this.api.retryAdminReceiptDelivery(id).subscribe({
      next: (items) => { this.deliveries.set(items ?? []); this.message.set('Receipt delivery retry completed.'); this.busy.set(false); },
      error: (e) => { this.error.set(httpErrorMessage(e, 'Receipt delivery retry failed.')); this.busy.set(false); },
    });
  }

  receiptLabel(receipt: ReceiptDto): string { return `${receipt.receiptNumber} · ${receipt.currency} ${receipt.amount}`; }
  status(value: string | number): string { if (typeof value === 'number') return ['Pending', 'Sent', 'Failed'][value] ?? `Status ${value}`; return String(value); }
  date(value?: string | null): string { if (!value) return '—'; const d = new Date(value); return Number.isNaN(d.getTime()) ? value : d.toLocaleString(); }
}
