import { Injectable, inject, signal } from '@angular/core';
import { ApiService } from './api.service';
import { NotificationDto } from '../models/api.models';
import { tap } from 'rxjs';

@Injectable({providedIn:'root'})
export class NotificationsService {
  private api=inject(ApiService);
  readonly unread=signal(0);
  readonly items=signal<NotificationDto[]>([]);

  list(){
    return this.api.get<NotificationDto[]>('notifications').pipe(tap(v=>{
      this.items.set(v);
      this.unread.set(v.filter(x=>!x.isRead).length);
    }));
  }
  push(notification:NotificationDto){
    this.items.update(items=>[notification,...items.filter(x=>x.notificationId!==notification.notificationId)]);
    if(!notification.isRead)this.unread.update(v=>v+1);
  }
  read(id:string){return this.api.put<void>(`notifications/${id}/read`,{}).pipe(tap(()=>{
    let changed=false;
    this.items.update(items=>items.map(x=>{if(x.notificationId===id&&!x.isRead){changed=true;return{...x,isRead:true};}return x;}));
    if(changed)this.unread.update(x=>Math.max(0,x-1));
  }));}
  readAll(){return this.api.put<void>('notifications/read-all',{}).pipe(tap(()=>{this.items.update(xs=>xs.map(x=>({...x,isRead:true})));this.unread.set(0);}));}
  remove(id:string){return this.api.delete<void>(`notifications/${id}`).pipe(tap(()=>{
    const target=this.items().find(x=>x.notificationId===id);
    this.items.update(xs=>xs.filter(x=>x.notificationId!==id));
    if(target&&!target.isRead)this.unread.update(v=>Math.max(0,v-1));
  }));}
}
