import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import { BookingCalendarDto, BookingDto, CreateBookingRequest, PayHereCheckoutDto, PaymentDto, ReceiptDeliveryDto, ReceiptDto, RefundDto, StartPaymentRequest, TicketDto } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class BookingsService {
  private api = inject(ApiService);

  list() { return this.api.get<BookingDto[]>('bookings', { page: 1, pageSize: 100 }); }
  get(id: string) { return this.api.get<BookingDto>(`bookings/${id}`); }
  create(eventId: string, holdToken: string, seatIds: string[]) { const body: CreateBookingRequest = { eventId, holdToken, seatIds }; return this.api.post<BookingDto>('bookings', body); }
  cancel(id: string) { return this.api.delete<BookingDto>(`bookings/${id}`); }
  cancelConfirmed(id: string) { return this.api.post<RefundDto>(`bookings/${id}/cancel-confirmed`, {}); }

  tickets() { return this.api.get<TicketDto[]>('tickets/mine'); }
  bookingTickets(id: string) { return this.api.get<TicketDto[]>(`bookings/${id}/tickets`); }

  payments() { return this.api.get<PaymentDto[]>('payments', { page: 1, pageSize: 100 }); }
  startPayment(body: StartPaymentRequest) { return this.api.post<PaymentDto>('payments', body); }
  payHere(id: string) { return this.api.post<PayHereCheckoutDto>(`payments/${id}/payhere-checkout`, {}); }

  receipts() { return this.api.get<ReceiptDto[]>('receipts'); }
  receipt(id: string) { return this.api.get<ReceiptDto>(`receipts/${id}`); }
  receiptDeliveries(id: string) { return this.api.get<ReceiptDeliveryDto[]>(`receipts/${id}/deliveries`); }
  retryReceipt(id: string) { return this.api.post<ReceiptDeliveryDto[]>(`receipts/${id}/deliveries/retry`, {}); }

  calendar(id: string) { return this.api.get<BookingCalendarDto>(`bookings/${id}/calendar`); }
  calendarIcs(id: string) { return this.api.blob(`bookings/${id}/calendar.ics`); }
}
