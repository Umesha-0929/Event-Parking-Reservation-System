import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { BookingSummary, PayHereCheckoutDto, PaymentResponseDto } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';

@Component({selector:'app-payment',imports:[RouterLink],templateUrl:'./payment.html',styleUrl:'./payment.scss'})
export class PaymentComponent implements OnInit {
 private readonly route=inject(ActivatedRoute);private readonly domain=inject(DomainApiService);
 readonly bookingId=this.route.snapshot.queryParamMap.get('bookingId')??'';readonly booking=signal<BookingSummary|null>(null);readonly payment=signal<PaymentResponseDto|null>(null);readonly checkout=signal<PayHereCheckoutDto|null>(null);readonly loading=signal(false);readonly error=signal('');
 ngOnInit():void{if(!this.bookingId)return;this.loading.set(true);this.domain.booking(this.bookingId).subscribe({next:(booking)=>{this.booking.set(booking);this.loading.set(false);},error:(error)=>{this.error.set(httpErrorMessage(error,'The booking could not be loaded.'));this.loading.set(false);}});}
 startPayment():void{if(!this.bookingId||this.loading())return;this.loading.set(true);this.error.set('');this.domain.createPayment({bookingId:this.bookingId}).subscribe({next:(payment)=>{this.payment.set(payment);this.domain.payHereCheckout(payment.paymentId).subscribe({next:(checkout)=>{this.checkout.set(checkout);this.loading.set(false);},error:(error)=>{this.error.set(httpErrorMessage(error,'Payment was created, but PayHere checkout is not configured.'));this.loading.set(false);}});},error:(error)=>{this.error.set(httpErrorMessage(error,'The payment could not be started.'));this.loading.set(false);}});}
}
