import { DatePipe } from '@angular/common';
import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TicketDto } from '../../core/models/api.models';
import { QrCodeComponent } from './qr-code.component';
import { StatusBadgeComponent } from './status-badge.component';

@Component({
  selector: 'app-ticket-card',
  standalone: true,
  imports: [DatePipe, RouterLink, QrCodeComponent, StatusBadgeComponent],
  template: `
    <article class="nv-card nv-ticket-pattern overflow-hidden grid sm:grid-cols-[1fr_180px] h-full">
      <div class="p-5">
        <div class="flex flex-wrap gap-2 items-center">
          <app-status-badge [label]="String(ticket.status)" [tone]="String(ticket.status).toLowerCase()==='active'?'success':'neutral'"/>
          <span class="text-xs nv-muted">{{ticket.ticketNo}}</span>
        </div>
        <h2 class="text-xl font-black mt-4">{{ticket.eventName||'Event ticket'}}</h2>
        <p class="nv-muted mt-1">{{ticket.venueName||'Venue'}}</p>
        <div class="grid grid-cols-2 gap-3 mt-5 text-sm">
          <div class="nv-card-soft p-3"><span class="nv-muted block">Booking</span><strong>{{ticket.bookingNumber||ticket.bookingId}}</strong></div>
          <div class="nv-card-soft p-3"><span class="nv-muted block">Seat</span><strong>{{ticket.rowLabel&&ticket.seatNumber?(ticket.rowLabel+' '+ticket.seatNumber):'General'}}</strong></div>
        </div>
        <p class="text-xs nv-muted mt-4">Issued {{ticket.issuedAtUtc|date:'MMM d, y · h:mm a'}}</p>
        @if(ticket.checkedInAtUtc){<p class="text-xs mt-1" style="color:var(--success)">Checked in {{ticket.checkedInAtUtc|date:'h:mm a'}}</p>}
        <div class="flex flex-wrap gap-2 mt-5">
          <a [routerLink]="['/app/events',ticket.eventId]" class="nv-btn nv-btn-secondary">Event Details</a>
          <a routerLink="/app/parking" [queryParams]="{event:ticket.eventId}" class="nv-btn nv-btn-secondary">Parking</a>
        </div>
      </div>
      <div class="p-5 flex items-center justify-center border-t sm:border-t-0 sm:border-l relative" style="border-color:var(--border)"><div class="absolute inset-y-4 left-0 border-l border-dashed hidden sm:block" style="border-color:var(--border)"></div>
        <div class="w-36 h-36">
          @if(ticket.qrPayload){<app-qr-code [value]="ticket.qrPayload"/>}
          @else{<div class="w-full h-full nv-card-soft grid place-items-center text-xs text-center p-3 nv-muted">QR not available</div>}
        </div>
      </div>
    </article>
  `
})
export class TicketCardComponent {
  @Input({ required: true }) ticket!: TicketDto;
  String = String;
}
