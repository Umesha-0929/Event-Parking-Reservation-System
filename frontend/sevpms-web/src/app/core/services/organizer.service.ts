import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import { BookingDto, CheckInTicketResponse, FoodOrder, ManualPaymentReviewDto, PaymentDto, UpdateFoodOrderStatusRequest } from '../models/api.models';

@Injectable({providedIn:'root'})
export class OrganizerService {
  private api=inject(ApiService);
  checkIn(eventId:string,qrPayload:string,gate='Main Gate'){return this.api.post<CheckInTicketResponse>(`events/${eventId}/check-ins/scan`,{qrPayload,gate});}
  manualPayments(){return this.api.get<ManualPaymentReviewDto[]>('payments/manual-review');}
  approvePayment(id:string){return this.api.post<PaymentDto>(`payments/${id}/manual-approve`,{});}
  rejectPayment(id:string){return this.api.post<PaymentDto>(`payments/${id}/manual-reject`,{});}

  bookings(page=1,pageSize=100){return this.api.get<BookingDto[]>('organizer/bookings',{page,pageSize});}
  updateFoodOrder(id:string,newStatus:string,note=''){const body:UpdateFoodOrderStatusRequest={newStatus,note};return this.api.patch<FoodOrder>(`food/orders/${id}/status`,body);}
}
