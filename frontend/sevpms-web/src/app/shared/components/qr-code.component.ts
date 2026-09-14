import { Component, Input, OnChanges, signal } from '@angular/core';
import QRCode from 'qrcode';
@Component({selector:'app-qr-code',standalone:true,template:`@if(src()){<img [src]="src()" alt="Ticket QR code" class="w-full h-full object-contain bg-white p-2 rounded-xl">}@else{<div class="nv-skeleton w-full h-full"></div>}`})
export class QrCodeComponent implements OnChanges{@Input() value='';src=signal('');ngOnChanges(){if(!this.value){this.src.set('');return;}QRCode.toDataURL(this.value,{width:320,margin:1,errorCorrectionLevel:'M'}).then(v=>this.src.set(v)).catch(()=>this.src.set(''));}}
