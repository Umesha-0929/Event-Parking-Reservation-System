import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { forkJoin, of, catchError } from 'rxjs';
import { BookingSummary, TicketApiModel } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';
import { QrCodeComponent } from '../../../shared/components/qr-code/qr-code';

interface TicketWithBooking { ticket: TicketApiModel; booking: BookingSummary; }
@Component({selector:'app-tickets',imports:[QrCodeComponent,DatePipe],templateUrl:'./tickets.html',styleUrl:'./tickets.scss'})
export class TicketsComponent {
  private readonly domain=inject(DomainApiService);
  readonly loading=signal(true); readonly error=signal(''); readonly items=signal<TicketWithBooking[]>([]); readonly selected=signal<TicketWithBooking|null>(null);
  constructor(){this.load();}
  load():void{this.loading.set(true);this.error.set('');this.domain.bookings().subscribe({next:(bookings)=>{if(!bookings.length){this.items.set([]);this.loading.set(false);return;}const calls=bookings.map((booking)=>{const id=booking.bookingId??booking.id;return id?this.domain.ticketsForBooking(id).pipe(catchError(()=>of([] as TicketApiModel[]))):of([] as TicketApiModel[]);});forkJoin(calls).subscribe((groups)=>{this.items.set(groups.flatMap((tickets,index)=>tickets.map((ticket)=>({ticket,booking:bookings[index]}))));this.loading.set(false);});},error:(error)=>{this.error.set(httpErrorMessage(error,'Tickets could not be loaded.'));this.loading.set(false);}});}
  qr(item:TicketWithBooking):string{return item.ticket.qrPayload??item.ticket.qrValue??'';}
}
