import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { AdminService } from '../../core/services/admin.service';
import { ManualPaymentReviewDto, ReceiptDeliveryDto, ReceiptDto } from '../../core/models/api.models';
import { PageStateComponent } from '../../shared/components/page-state.component';

@Component({
  selector: 'app-admin-payments',
  standalone: true,
  imports: [DatePipe, DecimalPipe, PageStateComponent],
  template: `
  <div class="nv-page space-y-6">
    <div>
      <p class="font-extrabold text-sm nv-muted">Platform Finance</p>
      <h1 class="nv-page-title">Payments & Receipts</h1>
      <p class="nv-muted mt-1">Review manual payment proofs, payment status and receipt delivery.</p>
    </div>

    <section class="nv-card p-5 md:p-6">
      <div class="flex flex-wrap items-center justify-between gap-3">
        <div><h2 class="font-black text-xl">Manual payment review</h2><p class="nv-muted text-sm mt-1">Only real submitted proofs are listed.</p></div>
        <button type="button" class="nv-btn nv-btn-secondary" (click)="loadPayments()">Refresh</button>
      </div>
      @if(paymentLoading()){
        <div class="space-y-3 mt-5" aria-label="Loading payment proofs">@for(i of [1,2,3];track i){<div class="nv-skeleton h-16"></div>}</div>
      } @else if(paymentError()){
        <div class="mt-5"><app-page-state kind="error" title="Payment proofs unavailable" message="We couldn't load manual payment proofs right now." actionLabel="Retry" [action]="retryPayments"/></div>
      } @else if(payments().length){
        <div class="overflow-x-auto mt-5"><table class="w-full text-sm"><caption class="sr-only">Manual payment proofs awaiting review</caption><thead><tr class="text-left nv-muted"><th class="py-3 pr-4">Payment</th><th class="py-3 pr-4">Event / Booking</th><th class="py-3 pr-4">Amount</th><th class="py-3 pr-4">Proof</th><th class="py-3">Actions</th></tr></thead><tbody>@for(p of payments();track p.paymentId){<tr class="border-t" style="border-color:var(--border)"><td class="py-4 pr-4"><div class="font-extrabold">{{short(p.paymentId)}}</div><div class="text-xs nv-muted">Submitted {{p.submittedAtUtc|date:'medium'}}</div></td><td class="py-4 pr-4"><div class="font-bold">Event {{short(p.eventId)}}</div><div class="text-xs nv-muted mt-1">Booking {{short(p.bookingId)}}</div></td><td class="py-4 pr-4">{{p.currency||'LKR'}} {{num(p.amount)|number:'1.0-2'}}</td><td class="py-4 pr-4">@if(p.proofUrl){<a class="font-extrabold underline underline-offset-4" [href]="p.proofUrl" target="_blank" rel="noopener noreferrer">View Proof</a>}@else{<span class="nv-muted">No proof URL</span>}</td><td class="py-4"><div class="flex flex-wrap gap-2"><button type="button" class="nv-btn nv-btn-primary" (click)="approve(p.paymentId)">Approve</button><button type="button" class="nv-btn nv-btn-secondary" (click)="reject(p.paymentId)">Reject</button></div></td></tr>}</tbody></table></div>
      } @else {
        <div class="mt-5"><app-page-state title="No payment proofs" message="No manual payment proofs are waiting for review."/></div>
      }
    </section>

    <section class="nv-card p-5 md:p-6">
      <div class="flex flex-wrap items-center justify-between gap-3"><div><h2 class="font-black text-xl">Receipts</h2><p class="nv-muted text-sm mt-1">Generated receipts and delivery retry controls.</p></div><button type="button" class="nv-btn nv-btn-secondary" (click)="loadReceipts()">Refresh</button></div>
      @if(receiptLoading()){
        <div class="grid lg:grid-cols-2 gap-3 mt-5" aria-label="Loading receipts">@for(i of [1,2];track i){<div class="nv-skeleton h-48"></div>}</div>
      } @else if(receiptError()){
        <div class="mt-5"><app-page-state kind="error" title="Receipts unavailable" message="We couldn't load receipts right now." actionLabel="Retry" [action]="retryReceipts"/></div>
      } @else if(receipts().length){
        <div class="grid lg:grid-cols-2 gap-3 mt-5">@for(r of receipts();track r.receiptId){<article class="nv-card-soft p-4"><div class="flex justify-between gap-3"><div><div class="font-black">{{r.receiptNumber||short(r.receiptId)}}</div><div class="text-xs nv-muted mt-1">{{r.issuedAtUtc|date:'medium'}}</div></div><div class="font-black">{{r.currency||'LKR'}} {{num(r.amount)|number:'1.0-2'}}</div></div><div class="grid grid-cols-2 gap-3 mt-4 text-sm"><div><div class="nv-muted">Booking</div><div class="font-bold mt-1">{{short(r.bookingId)}}</div></div><div><div class="nv-muted">Payment</div><div class="font-bold mt-1">{{short(r.paymentId)}}</div></div></div><div class="flex flex-wrap gap-2 mt-4"><button type="button" class="nv-btn nv-btn-secondary" (click)="showDeliveries(r.receiptId)">Delivery Status</button><button type="button" class="nv-btn nv-btn-secondary" (click)="retryReceipt(r.receiptId)">Retry Failed</button></div>@if(openReceipt()===r.receiptId){<div class="mt-4 space-y-2">@for(d of deliveries();track d.receiptDeliveryId){<div class="rounded-xl border p-3 text-sm" style="border-color:var(--border)"><div class="flex justify-between gap-3"><div class="font-bold">{{d.channel||'Delivery'}}</div><span class="nv-status">{{deliveryStatus(d.status)}}</span></div><div class="nv-muted text-xs mt-1">{{d.destinationMasked||'Protected destination'}}</div><div class="nv-muted text-xs mt-1">Attempts: {{d.attemptCount}} @if(d.sentAtUtc){<span>· Sent {{d.sentAtUtc|date:'medium'}}</span>}</div>@if(d.lastError){<div class="text-xs mt-2" style="color:var(--danger)">{{d.lastError}}</div>}</div>}@empty{<div class="nv-muted text-sm">No delivery records were returned.</div>}</div>}</article>}</div>
      } @else {
        <div class="mt-5"><app-page-state title="No receipts yet" message="No receipts have been generated yet."/></div>
      }
    </section>
    @if(message()){<div class="fixed right-4 bottom-4 nv-card px-4 py-3" role="status">{{message()}}</div>}
  </div>`
})
export class AdminPaymentsComponent implements OnInit {
  private api = inject(AdminService);
  payments = signal<ManualPaymentReviewDto[]>([]);
  receipts = signal<ReceiptDto[]>([]);
  deliveries = signal<ReceiptDeliveryDto[]>([]);
  openReceipt = signal('');
  message = signal('');
  paymentLoading = signal(true);
  paymentError = signal(false);
  receiptLoading = signal(true);
  receiptError = signal(false);
  retryPayments=()=>this.loadPayments();
  retryReceipts=()=>this.loadReceipts();
  ngOnInit(){ this.loadPayments(); this.loadReceipts(); }
  loadPayments(){ this.paymentLoading.set(true);this.paymentError.set(false);this.api.manualPayments().subscribe({next:v=>{this.payments.set(v);this.paymentLoading.set(false);},error:()=>{this.paymentLoading.set(false);this.paymentError.set(true);}}); }
  loadReceipts(){ this.receiptLoading.set(true);this.receiptError.set(false);this.api.receipts().subscribe({next:v=>{this.receipts.set(v);this.receiptLoading.set(false);},error:()=>{this.receiptLoading.set(false);this.receiptError.set(true);}}); }
  approve(id:string){ this.api.approvePayment(id).subscribe({next:()=>{this.message.set('Payment approved.');this.loadPayments();},error:e=>this.message.set(e?.error?.message||e?.error?.error||'Payment could not be approved.')}); }
  reject(id:string){ this.api.rejectPayment(id).subscribe({next:()=>{this.message.set('Payment rejected.');this.loadPayments();},error:e=>this.message.set(e?.error?.message||e?.error?.error||'Payment could not be rejected.')}); }
  showDeliveries(id:string){ this.openReceipt.set(id); this.api.receiptDeliveries(id).subscribe({next:v=>this.deliveries.set(v),error:()=>this.deliveries.set([])}); }
  retryReceipt(id:string){ this.api.retryReceipt(id).subscribe({next:v=>{this.message.set('Receipt delivery retry requested.');this.deliveries.set(v);this.openReceipt.set(id);},error:e=>this.message.set(e?.error?.message||'Receipt delivery could not be retried.')}); }
  deliveryStatus(value:number|string){ const n=Number(value); return Number.isFinite(n)?(['Pending','Sent','Failed'][n]??String(value)):String(value); }
  short(value:unknown){ const s=String(value??''); return s.length>12?`${s.slice(0,8)}…${s.slice(-4)}`:(s||'—'); }
  num(value:unknown){ return Number(value??0); }
}
