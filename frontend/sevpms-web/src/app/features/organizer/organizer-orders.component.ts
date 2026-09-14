import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { FoodService } from '../../core/services/food.service';
import { EventsService } from '../../core/services/events.service';
import { ReportsService } from '../../core/services/reports.service';
import { EventDto, FoodOrder, OrganizerReportDto } from '../../core/models/api.models';
import { PageStateComponent } from '../../shared/components/page-state.component';

@Component({
  selector: 'app-organizer-orders',
  standalone: true,
  imports: [DatePipe, DecimalPipe, FormsModule, PageStateComponent],
  template: `
  <div class="nv-page space-y-6">
    <div>
      <p class="font-extrabold text-sm nv-muted">Event Operations</p>
      <h1 class="nv-page-title">Food Orders</h1>
      <p class="nv-muted mt-1">Monitor and update food orders placed for your events.</p>
    </div>

    @if(report()){
      <div class="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
        <article class="nv-card p-6"><div class="nv-muted">Food orders</div><div class="text-4xl font-black mt-2">{{report()!.foodOrders}}</div></article>
        <article class="nv-card p-6"><div class="nv-muted">Food revenue</div><div class="text-4xl font-black mt-2">LKR {{report()!.foodRevenue|number:'1.0-0'}}</div></article>
        <article class="nv-card p-6"><div class="nv-muted">Events</div><div class="text-4xl font-black mt-2">{{report()!.events}}</div></article>
      </div>
    }

    <section class="nv-card p-4 md:p-5">
      <div class="grid md:grid-cols-[1fr_220px_auto] gap-3 items-end">
        <div>
          <label class="nv-label" for="food-order-search">Search orders</label>
          <input id="food-order-search" class="nv-input" [(ngModel)]="query" name="query" placeholder="Order number, event or fulfilment" />
        </div>
        <div>
          <label class="nv-label" for="food-order-status">Status</label>
          <select id="food-order-status" class="nv-input" [(ngModel)]="status" name="status">
            @for(option of statuses;track option){<option [value]="option">{{option}}</option>}
          </select>
        </div>
        <button type="button" class="nv-btn nv-btn-secondary" (click)="load()">Refresh</button>
      </div>
    </section>

    @if(loading()){
      <div class="space-y-3">@for(i of [1,2,3];track i){<div class="nv-skeleton h-40"></div>}</div>
    } @else if(filtered.length){
      <div class="space-y-4">
        @for(order of filtered;track order.id){
          <article class="nv-card p-5 md:p-6">
            <div class="flex flex-col xl:flex-row xl:items-start gap-5">
              <div class="flex-1 min-w-0">
                <div class="flex flex-wrap items-center gap-2">
                  <span class="nv-status" [class.success]="order.status==='Completed'">{{order.status}}</span>
                  <span class="text-xs nv-muted">{{order.createdAtUtc|date:'medium'}}</span>
                </div>
                <div class="mt-3 flex flex-wrap items-baseline gap-x-3 gap-y-1">
                  <h2 class="font-black text-xl">{{order.orderNo}}</h2>
                  <span class="nv-muted text-sm">{{eventName(order.eventId)}}</span>
                </div>
                <div class="grid sm:grid-cols-2 lg:grid-cols-4 gap-3 mt-4 text-sm">
                  <div class="nv-card-soft p-3"><div class="nv-muted">Fulfilment</div><div class="font-extrabold mt-1">{{order.fulfillmentType}}</div></div>
                  <div class="nv-card-soft p-3"><div class="nv-muted">Seat / pickup</div><div class="font-extrabold mt-1">{{order.seatLabelSnapshot||'Pickup point'}}</div></div>
                  <div class="nv-card-soft p-3"><div class="nv-muted">Items</div><div class="font-extrabold mt-1">{{itemCount(order)}}</div></div>
                  <div class="nv-card-soft p-3"><div class="nv-muted">Total</div><div class="font-extrabold mt-1">LKR {{order.total|number:'1.0-2'}}</div></div>
                </div>
                <div class="mt-4 flex flex-wrap gap-2">
                  @for(item of order.items;track item.id){<span class="nv-chip">{{item.quantity}} × {{item.itemNameSnapshot}}</span>}
                </div>
              </div>

              <div class="xl:w-[260px] shrink-0">
                <div class="nv-card-soft p-4">
                  <div class="text-sm nv-muted">Order progress</div>
                  <div class="font-black mt-1">{{order.status}}</div>
                  @if(nextStatuses(order.status).length){
                    <div class="grid gap-2 mt-4">
                      @for(next of nextStatuses(order.status);track next){
                        <button type="button" class="nv-btn" [class.nv-btn-primary]="next!=='Cancelled'&&next!=='Rejected'" [class.nv-btn-secondary]="next==='Cancelled'||next==='Rejected'" [disabled]="busyId()===order.id" (click)="update(order,next)">{{busyId()===order.id?'Updating...':actionLabel(next)}}</button>
                      }
                    </div>
                  } @else {
                    <p class="text-sm nv-muted mt-3">No further status action is required.</p>
                  }
                </div>
              </div>
            </div>
          </article>
        }
      </div>
    } @else {
      <app-page-state title="No matching food orders" message="Food orders for your events will appear here when customers place them." />
    }

    @if(message()){<div class="fixed right-4 bottom-24 lg:bottom-4 nv-card px-4 py-3 text-sm" role="status">{{message()}}</div>}
  </div>`
})
export class OrganizerOrdersComponent implements OnInit {
  private food = inject(FoodService);
  private eventsApi = inject(EventsService);
  private reports = inject(ReportsService);

  report = signal<OrganizerReportDto | null>(null);
  orders = signal<FoodOrder[]>([]);
  events = signal<EventDto[]>([]);
  loading = signal(true);
  busyId = signal('');
  message = signal('');
  query = '';
  status = 'All';
  readonly statuses = ['All', 'Placed', 'Accepted', 'Preparing', 'Ready', 'Completed', 'Rejected', 'Cancelled'];

  get filtered() {
    const q = this.query.trim().toLowerCase();
    return this.orders().filter(order => {
      if (this.status !== 'All' && order.status !== this.status) return false;
      if (!q) return true;
      return [order.orderNo, order.fulfillmentType, order.seatLabelSnapshot ?? '', this.eventName(order.eventId)]
        .some(value => value.toLowerCase().includes(q));
    });
  }

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    forkJoin({
      report: this.reports.organizer(),
      orders: this.food.organizerOrders(),
      events: this.eventsApi.mine()
    }).subscribe({
      next: result => {
        this.report.set(result.report);
        this.orders.set(result.orders);
        this.events.set(result.events);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.message.set('Food orders could not be loaded.');
      }
    });
  }

  eventName(eventId: string) {
    return this.events().find(event => event.eventId === eventId)?.title ?? `Event ${this.short(eventId)}`;
  }

  itemCount(order: FoodOrder) { return order.items.reduce((total, item) => total + item.quantity, 0); }

  nextStatuses(current: string): string[] {
    switch (current) {
      case 'Placed': return ['Accepted', 'Rejected', 'Cancelled'];
      case 'Accepted': return ['Preparing', 'Cancelled'];
      case 'Preparing': return ['Ready'];
      case 'Ready': return ['Completed'];
      default: return [];
    }
  }

  actionLabel(status: string) {
    return status === 'Accepted' ? 'Accept Order' :
      status === 'Preparing' ? 'Start Preparing' :
      status === 'Ready' ? 'Mark Ready' :
      status === 'Completed' ? 'Complete Order' :
      status === 'Rejected' ? 'Reject Order' : 'Cancel Order';
  }

  update(order: FoodOrder, nextStatus: string) {
    this.busyId.set(order.id);
    this.food.updateStatus(order.id, nextStatus).subscribe({
      next: updated => {
        this.orders.update(items => items.map(item => item.id === updated.id ? updated : item));
        this.busyId.set('');
        this.message.set(`Order ${updated.orderNo} is now ${updated.status}.`);
      },
      error: error => {
        this.busyId.set('');
        this.message.set(error?.error?.message || error?.error?.error || 'Order status could not be updated.');
      }
    });
  }

  short(value: string) { return value.length > 12 ? `${value.slice(0, 8)}…${value.slice(-4)}` : value; }
}
