import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import { AdminDashboardStats, AdminUser, AuditLogDto, BookingDto, EventCategory, ManageNearbyPlaceRequest, ManualPaymentReviewDto, NearbyPlace, PaymentDto, PlatformReportDto, ReceiptDeliveryDto, ReceiptDto, UpsertEventCategoryRequest, UpsertFacilityRequest, VenueFacility } from '../models/api.models';
@Injectable({providedIn:'root'})
export class AdminService {
 private api=inject(ApiService);
 stats(){return this.api.get<AdminDashboardStats>('admin/dashboard/stats');}
 users(params:Record<string,unknown>={}){return this.api.get<AdminUser[]>('admin/users',params);}
 bookings(params:Record<string,unknown>={page:1,pageSize:100}){return this.api.get<BookingDto[]>('admin/bookings',params);}
 user(id:string){return this.api.get<AdminUser>(`admin/users/${id}`);}
 setStatus(id:string,status:number){return this.api.put<AdminUser>(`admin/users/${id}/status`,{status});}
 deleteUser(id:string){return this.api.delete<void>(`admin/users/${id}`);}
 audits(params:Record<string,unknown>={}){return this.api.get<AuditLogDto[]>('admin/audit-logs',params);}
 receipts(){return this.api.get<ReceiptDto[]>('admin/receipts');}
 receiptDeliveries(id:string){return this.api.get<ReceiptDeliveryDto[]>(`admin/receipts/${id}/deliveries`);}
 retryReceipt(id:string){return this.api.post<ReceiptDeliveryDto[]>(`admin/receipts/${id}/deliveries/retry`,{});}
 platformReport(params:Record<string,unknown>={}){return this.api.get<PlatformReportDto>('reports/platform',params);}
 platformCsv(params:Record<string,unknown>={}){return this.api.blob('reports/platform.csv',params);}
 categoriesAdmin(){return this.api.get<EventCategory[]>('event-categories/admin');}
 createCategory(body:UpsertEventCategoryRequest){return this.api.post<EventCategory>('event-categories',body);}
 updateCategory(id:string,body:UpsertEventCategoryRequest){return this.api.put<EventCategory>(`event-categories/${id}`,body);}
 deactivateCategory(id:string){return this.api.delete<void>(`event-categories/${id}`);}
 deleteCategory(id:string){return this.api.delete<void>(`event-categories/${id}/permanent`);}
 facilitiesAdmin(){return this.api.get<VenueFacility[]>('venue-facilities/admin');}
 createFacility(body:UpsertFacilityRequest){return this.api.post<VenueFacility>('venue-facilities',body);}
 updateFacility(id:string,body:UpsertFacilityRequest){return this.api.put<VenueFacility>(`venue-facilities/${id}`,body);}
 deleteFacility(id:string){return this.api.delete<void>(`venue-facilities/${id}/permanent`);}
 manualPayments(){return this.api.get<ManualPaymentReviewDto[]>('payments/manual-review');}
 approvePayment(id:string){return this.api.post<PaymentDto>(`payments/${id}/manual-approve`,{});}
 rejectPayment(id:string){return this.api.post<PaymentDto>(`payments/${id}/manual-reject`,{});}
 completePayment(id:string){return this.api.post<PaymentDto>(`payments/${id}/complete`,{});}
 failPayment(id:string){return this.api.post<PaymentDto>(`payments/${id}/fail`,{});}
 createPlace(body:ManageNearbyPlaceRequest){return this.api.post<NearbyPlace>('places',body);}
 updatePlace(id:string,body:ManageNearbyPlaceRequest){return this.api.put<NearbyPlace>(`places/${id}`,body);}
 deletePlace(id:string){return this.api.delete<void>(`places/${id}`);}
}
