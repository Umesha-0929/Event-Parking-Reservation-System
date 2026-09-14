import { Injectable, inject, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import { NotificationsService } from './notifications.service';
import { NotificationDto } from '../models/api.models';

export interface SeatAvailabilityChange {
  eventId:string;
  seatIds:string[];
  state:string;
  expiresAtUtc?:string|null;
}

@Injectable({providedIn:'root'})
export class RealtimeService {
  private auth=inject(AuthService);
  private notices=inject(NotificationsService);
  private notificationHub?:signalR.HubConnection;
  private eventHub?:signalR.HubConnection;
  private joinedEventId='';
  private seatHandler?: (change:SeatAvailabilityChange)=>void;
  readonly connected=signal(false);
  readonly eventConnected=signal(false);

  async start(){
    if(!this.auth.token()||this.notificationHub)return;
    this.notificationHub=new signalR.HubConnectionBuilder()
      .withUrl(`${environment.hubBaseUrl}/hubs/notifications`,{accessTokenFactory:()=>this.auth.token()??''})
      .withAutomaticReconnect()
      .build();
    this.notificationHub.on('notification.received',(notification:NotificationDto)=>this.notices.push(notification));
    this.notificationHub.onreconnected(()=>this.connected.set(true));
    this.notificationHub.onclose(()=>this.connected.set(false));
    try{await this.notificationHub.start();this.connected.set(true);}catch{this.connected.set(false);}
  }

  async joinEvent(eventId:string,onSeatChanged:(change:SeatAvailabilityChange)=>void){
    if(!this.auth.token()||!eventId)return;
    this.joinedEventId=eventId;
    this.seatHandler=onSeatChanged;
    if(!this.eventHub){
      this.eventHub=new signalR.HubConnectionBuilder()
        .withUrl(`${environment.hubBaseUrl}/hubs/events`,{accessTokenFactory:()=>this.auth.token()??''})
        .withAutomaticReconnect()
        .build();
      this.eventHub.on('seat.availability.changed',(change:SeatAvailabilityChange)=>this.seatHandler?.(change));
      this.eventHub.onreconnected(async()=>{
        this.eventConnected.set(true);
        if(this.joinedEventId){try{await this.eventHub?.invoke('JoinEvent',this.joinedEventId);}catch{this.eventConnected.set(false);}}
      });
      this.eventHub.onclose(()=>this.eventConnected.set(false));
      try{await this.eventHub.start();this.eventConnected.set(true);}catch{this.eventConnected.set(false);return;}
    }
    try{await this.eventHub.invoke('JoinEvent',eventId);}catch{this.eventConnected.set(false);}
  }

  async leaveEvent(eventId:string){
    if(this.eventHub&&eventId){try{await this.eventHub.invoke('LeaveEvent',eventId);}catch{/* connection may already be closed */}}
    if(this.joinedEventId===eventId){this.joinedEventId='';this.seatHandler=undefined;}
  }

  async stop(){
    await this.notificationHub?.stop();
    await this.eventHub?.stop();
    this.notificationHub=undefined;
    this.eventHub=undefined;
    this.joinedEventId='';
    this.seatHandler=undefined;
    this.connected.set(false);
    this.eventConnected.set(false);
  }
}
