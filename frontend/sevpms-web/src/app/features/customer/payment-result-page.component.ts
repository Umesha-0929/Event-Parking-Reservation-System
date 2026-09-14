import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { BookingsService } from '../../core/services/bookings.service';

@Component({
  selector:'app-payment-result-page',
  standalone:true,
  imports:[RouterLink],
  template:`
  <div class="nv-page max-w-3xl">
    <section class="nv-card p-6 sm:p-8 text-center">
      <div class="w-16 h-16 mx-auto rounded-2xl grid place-items-center text-2xl font-black" [style.background]="iconBackground" [style.color]="iconColor">{{icon}}</div>
      <p class="text-sm font-extrabold nv-muted mt-5">Payment return</p>
      <h1 class="nv-page-title mt-1">We're checking your payment</h1>
      <p class="nv-muted mt-3 max-w-xl mx-auto">PayHere confirms the final result to Nvent through the server callback. Refresh your bookings to see the latest confirmed status and ticket availability.</p>
      <div class="flex flex-wrap justify-center gap-3 mt-6">
        <button type="button" class="nv-btn nv-btn-primary" [disabled]="checking()" (click)="check()">{{checking()?'Checking...':'Check Booking Status'}}</button>
        <a routerLink="/app/bookings" class="nv-btn nv-btn-secondary">My Bookings</a>
        <a routerLink="/app/tickets" class="nv-btn nv-btn-secondary">My Tickets</a>
      </div>
      @if(message()){<p class="mt-5 text-sm font-bold" role="status" [style.color]="confirmed()?'var(--success)':'var(--text-2)'">{{message()}}</p>}
    </section>
  </div>`
})
export class PaymentResultPageComponent implements OnInit{
  private bookings=inject(BookingsService);
  private route=inject(ActivatedRoute);
  checking=signal(false);
  confirmed=signal(false);
  failed=signal(false);
  message=signal('');
  paymentId='';
  get icon(){return this.confirmed()?'✓':this.failed()?'×':'…';}
  get iconColor(){return this.confirmed()?'var(--success)':this.failed()?'var(--danger)':'var(--text-2)';}
  get iconBackground(){const c=this.confirmed()?'var(--success)':this.failed()?'var(--danger)':'var(--text-2)';return `color-mix(in srgb,${c} 15%,var(--surface))`;}
  ngOnInit(){this.paymentId=this.route.snapshot.queryParamMap.get('paymentId')??'';this.check();}
  check(){
    this.checking.set(true);this.message.set('');
    this.bookings.payments().subscribe({
      next:rows=>{
        this.checking.set(false);
        const payment=this.paymentId?rows.find(row=>row.paymentId===this.paymentId):undefined;
        if(!this.paymentId){this.confirmed.set(false);this.failed.set(true);this.message.set('Payment reference is missing. Open the result page from the PayHere checkout return link, or check My Bookings.');return;}
        if(!payment){this.confirmed.set(false);this.failed.set(false);this.message.set('This payment is not visible yet. The PayHere callback may still be processing; try again shortly.');return;}
        const successful=payment.status===1||String(payment.status).toLowerCase()==='successful';
        const failed=payment.status===2||String(payment.status).toLowerCase()==='failed';
        this.confirmed.set(successful);
        this.failed.set(failed);
        this.message.set(successful?'Your payment is confirmed. Your booking and ticket wallet are ready to refresh.':failed?'This payment was not completed. Return to My Bookings to retry checkout.':'This payment is still pending. The PayHere callback may still be processing; try again shortly.');
      },
      error:()=>{this.checking.set(false);this.message.set('Payment status could not be refreshed right now. Please try again.');}
    });
  }
}
