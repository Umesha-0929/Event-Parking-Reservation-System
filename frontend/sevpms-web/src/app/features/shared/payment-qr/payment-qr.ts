import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { catchError, of } from 'rxjs';
import { LocalMediaService } from '../../../core/services/local-media.service';
import { httpErrorMessage } from '../../../core/utils/http-error';
@Component({selector:'app-payment-qr',templateUrl:'./payment-qr.html',styleUrl:'./payment-qr.scss'})
export class PaymentQrComponent implements OnInit{
 private readonly route=inject(ActivatedRoute);private readonly media=inject(LocalMediaService);readonly qr=signal('');readonly busy=signal(false);readonly error=signal('');readonly success=signal('');readonly dragging=signal(false);readonly ownerLabel=(this.route.snapshot.data['ownerLabel'] as string|undefined)??'account';
 ngOnInit():void{this.media.myPaymentQr().pipe(catchError(()=>of(null))).subscribe(x=>this.qr.set(x?.url??''));}
 choose(input:HTMLInputElement):void{input.click();} changed(e:Event):void{const i=e.target as HTMLInputElement;const f=i.files?.[0];if(f)this.upload(f);i.value='';} over(e:DragEvent):void{e.preventDefault();this.dragging.set(true);} leave(e:DragEvent):void{e.preventDefault();this.dragging.set(false);} drop(e:DragEvent):void{e.preventDefault();this.dragging.set(false);const f=e.dataTransfer?.files?.[0];if(f)this.upload(f);} paste(e:ClipboardEvent):void{const f=[...(e.clipboardData?.items??[])].find(x=>x.kind==='file')?.getAsFile();if(f){e.preventDefault();this.upload(f);}}
 private upload(file:File):void{if(!file.type.startsWith('image/')){this.error.set('Choose a QR image file.');return;}this.busy.set(true);this.error.set('');this.media.uploadMyPaymentQr(file).subscribe({next:x=>{this.qr.set(x.url);this.busy.set(false);this.success.set('Payment QR saved. It is now available to the correct payer in Nvent.');},error:e=>{this.busy.set(false);this.error.set(httpErrorMessage(e,'Payment QR could not be saved.'));}});}
}
