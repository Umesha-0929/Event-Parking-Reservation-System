import { Component, ElementRef, OnDestroy, ViewChild, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BrowserQRCodeReader, IScannerControls } from '@zxing/browser';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';

interface CheckInResult { succeeded?: boolean; result?: string; message?: string; ticketId?: string; ticketNo?: string; scannedAtUtc?: string; }
@Component({selector:'app-qr-checkin',imports:[FormsModule],templateUrl:'./qr-checkin.html',styleUrl:'./qr-checkin.scss'})
export class QrCheckinComponent implements OnDestroy {
  @ViewChild('camera') camera?: ElementRef<HTMLVideoElement>;
  private readonly domain=inject(DomainApiService);
  readonly state=signal<'idle'|'requesting'|'camera'|'verifying'|'denied'|'unsupported'|'result'>('idle');
  readonly result=signal<CheckInResult|null>(null); readonly error=signal(''); readonly lastPayload=signal('');
  eventId=''; gate='Main Gate';
  private reader?:BrowserQRCodeReader; private controls?:IScannerControls;

  async startScanner():Promise<void>{
    if(!navigator.mediaDevices?.getUserMedia){this.state.set('unsupported');return;}
    if(!this.eventId.trim()){this.error.set('Enter the event ID before starting the scanner.');return;}
    this.error.set('');this.result.set(null);this.state.set('requesting');
    try{
      this.reader=new BrowserQRCodeReader();
      await new Promise<void>((resolve)=>queueMicrotask(resolve));
      const video=this.camera?.nativeElement;
      if(!video){this.state.set('idle');this.error.set('Camera view was not ready. Try again.');return;}
      this.controls=await this.reader.decodeFromVideoDevice(undefined,video,(decoded)=>{if(decoded&&this.state()==='camera'){const text=decoded.getText();this.lastPayload.set(text);this.stopCamera(false);this.verify(text);}});
      this.state.set('camera');
    }catch(error){this.state.set('denied');this.error.set(error instanceof Error?error.message:'Camera permission was denied or no camera is available.');}
  }
  verify(payload:string):void{this.state.set('verifying');this.error.set('');this.domain.scanTicket(this.eventId,{qrPayload:payload,gate:this.gate}).subscribe({next:(response)=>{this.result.set(response as CheckInResult);this.state.set('result');},error:(error)=>{const body=(error?.error??{}) as CheckInResult;this.result.set(body);this.error.set(body.message??httpErrorMessage(error,'Ticket verification failed.'));this.state.set('result');}});}
  scanNext():void{this.result.set(null);this.error.set('');this.lastPayload.set('');this.state.set('idle');void this.startScanner();}
  stopCamera(reset=true):void{this.controls?.stop();this.controls=undefined;if(reset)this.state.set('idle');}
  ngOnDestroy():void{this.controls?.stop();}
}
