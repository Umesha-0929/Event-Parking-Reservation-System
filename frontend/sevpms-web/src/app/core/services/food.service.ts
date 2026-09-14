import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import { CreateFoodOrderRequest, FoodOrder, FoodOrderStatusHistoryDto, FoodStall, MenuItem, UpdateFoodOrderStatusRequest } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class FoodService {
  private api = inject(ApiService);

  stalls(eventId: string) { return this.api.get<FoodStall[]>(`food/events/${eventId}/stalls`); }
  menu(stallId: string) { return this.api.get<MenuItem[]>(`food/stalls/${stallId}/menu`); }
  orders() { return this.api.get<FoodOrder[]>('food/orders', { page: 1, pageSize: 100 }); }
  organizerOrders(page = 1, pageSize = 100) { return this.api.get<FoodOrder[]>('food/organizer/orders', { page, pageSize }); }
  adminOrders(page = 1, pageSize = 100) { return this.api.get<FoodOrder[]>('food/admin/orders', { page, pageSize }); }
  order(id: string) { return this.api.get<FoodOrder>(`food/orders/${id}`); }
  createOrder(body: CreateFoodOrderRequest) { return this.api.post<FoodOrder>('food/orders', body); }
  history(id: string) { return this.api.get<FoodOrderStatusHistoryDto[]>(`food/orders/${id}/history`); }
  updateStatus(id: string, newStatus: string, note = '') { const body: UpdateFoodOrderStatusRequest = { newStatus, note }; return this.api.patch<FoodOrder>(`food/orders/${id}/status`, body); }
}
