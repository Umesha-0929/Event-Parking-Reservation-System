import { HttpClient } from '@angular/common/http';
import { DestroyRef, Injectable, inject, signal } from '@angular/core';
import { catchError, of, timeout } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({providedIn:'root'})
export class NetworkStatusService {
  readonly online = signal(typeof navigator === 'undefined' ? true : navigator.onLine);
  readonly apiAvailable = signal<boolean|null>(null);
  private destroyRef = inject(DestroyRef);
  private http = inject(HttpClient);
  private timer:ReturnType<typeof setInterval>|null=null;

  constructor(){
    if(typeof window === 'undefined') return;
    const update=()=>{
      this.online.set(navigator.onLine);
      if(navigator.onLine) this.checkApi(); else this.apiAvailable.set(null);
    };
    window.addEventListener('online',update);
    window.addEventListener('offline',update);
    update();
    this.timer=setInterval(()=>{if(this.online())this.checkApi();},30000);
    this.destroyRef.onDestroy(()=>{
      window.removeEventListener('online',update);
      window.removeEventListener('offline',update);
      if(this.timer)clearInterval(this.timer);
    });
  }

  checkApi(){
    const url=`${environment.apiBaseUrl.replace(/\/$/,'')}/health`;
    this.http.get(url,{withCredentials:true}).pipe(timeout(5000),catchError(()=>of(null))).subscribe(result=>this.apiAvailable.set(result!==null));
  }
}
