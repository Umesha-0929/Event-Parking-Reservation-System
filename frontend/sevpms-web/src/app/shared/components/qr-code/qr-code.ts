import { Component, Input, OnChanges, signal } from '@angular/core';
@Component({ selector:'app-qr-code', template:'@if(dataUrl()){<img [src]="dataUrl()" [alt]="alt">}@else{<div class="qr-loading">QR</div>}', styles:[':host{display:block}.qr-loading{width:180px;height:180px;display:grid;place-items:center;background:#f4f8f9;border:1px solid var(--nvent-border);border-radius:16px;color:var(--nvent-muted);font-weight:900}img{display:block;width:180px;height:180px;border-radius:14px;background:white;padding:10px}'] })
export class QrCodeComponent implements OnChanges {
  @Input({ required:true }) value=''; @Input() alt='Ticket QR code'; readonly dataUrl=signal('');
  async ngOnChanges(): Promise<void> { if(!this.value){this.dataUrl.set('');return;} const QRCode=await import('qrcode'); this.dataUrl.set(await QRCode.toDataURL(this.value,{width:360,margin:1,errorCorrectionLevel:'M'})); }
}
