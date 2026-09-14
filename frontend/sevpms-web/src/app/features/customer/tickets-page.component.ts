import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { BookingsService } from '../../core/services/bookings.service';
import { TicketDto } from '../../core/models/api.models';
import { PageStateComponent } from '../../shared/components/page-state.component';
import { TicketCardComponent } from '../../shared/components/ticket-card.component';

@Component({
  selector:'app-tickets-page',
  standalone:true,
  imports:[PageStateComponent,TicketCardComponent],
  template:`
  <div class="nv-page space-y-6">
    <section class="nv-hero min-h-[190px]">
      <div class="relative z-10">
        <p class="font-extrabold text-sm opacity-75">Ticket wallet</p>
        <h1 class="nv-page-title mt-1">My Tickets</h1>
        <p class="mt-2 opacity-80">Your event tickets and check-in QR codes.</p>
      </div>
    </section>

    @if(loading()){
      <div class="nv-skeleton h-80"></div>
    }@else if(error()){
      <app-page-state kind="error" title="We couldn't load your tickets" message="Please check your connection and try again." actionLabel="Retry" [action]="retry"/>
    }@else if(tickets().length){
      <div class="grid xl:grid-cols-2 gap-5">
        @for(ticket of tickets();track ticket.ticketId){
          <app-ticket-card [ticket]="ticket"/>
        }
      </div>
    }@else{
      <app-page-state title="No tickets yet" message="Tickets will appear here after a booking is successfully paid and issued." actionLabel="Explore Events" [action]="explore"/>
    }
  </div>`
})
export class TicketsPageComponent implements OnInit {
  private api=inject(BookingsService);
  private router=inject(Router);
  tickets=signal<TicketDto[]>([]);
  loading=signal(true);
  error=signal(false);
  explore=()=>this.router.navigateByUrl('/app/events');
  retry=()=>this.load();

  ngOnInit(){this.load();}
  load(){
    this.loading.set(true);this.error.set(false);
    this.api.tickets().subscribe({
      next:value=>{this.tickets.set(value);this.loading.set(false);},
      error:()=>{this.error.set(true);this.loading.set(false);}
    });
  }
}
