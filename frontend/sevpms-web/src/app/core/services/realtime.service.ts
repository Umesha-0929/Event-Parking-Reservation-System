import { Injectable, inject, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { SessionService } from './session.service';

@Injectable({ providedIn: 'root' })
export class RealtimeService {
  private readonly session = inject(SessionService);
  private notificationConnection?: signalR.HubConnection;
  private eventConnection?: signalR.HubConnection;

  readonly connected = signal(false);
  readonly latestNotification = signal<unknown | null>(null);
  readonly latestSeatUpdate = signal<unknown | null>(null);

  async startNotifications(): Promise<void> {
    if (typeof window === 'undefined' || this.notificationConnection || !this.session.hasSession()) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(environment.notificationHubUrl, {
        accessTokenFactory: () => this.session.accessToken() ?? '',
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .build();

    // These names match RealtimeEventNames in the ASP.NET backend.
    connection.on('notification.received', (payload) => this.latestNotification.set(payload));
    connection.onreconnected(() => this.connected.set(true));
    connection.onreconnecting(() => this.connected.set(false));
    connection.onclose(() => this.connected.set(false));

    this.notificationConnection = connection;
    try {
      await connection.start();
      this.connected.set(true);
    } catch {
      this.notificationConnection = undefined;
      this.connected.set(false);
    }
  }

  async joinEvent(eventId: string): Promise<void> {
    if (typeof window === 'undefined' || !this.session.hasSession()) return;
    if (!this.eventConnection) {
      this.eventConnection = new signalR.HubConnectionBuilder()
        .withUrl(environment.eventHubUrl, {
          accessTokenFactory: () => this.session.accessToken() ?? '',
        })
        .withAutomaticReconnect([0, 2000, 5000, 10000])
        .build();

      this.eventConnection.on('seat.availability.changed', (payload) =>
        this.latestSeatUpdate.set({ eventName: 'seat.availability.changed', payload }));

      try { await this.eventConnection.start(); }
      catch { this.eventConnection = undefined; return; }
    }

    try { await this.eventConnection.invoke('JoinEvent', eventId); }
    catch { /* Keep the page usable if the hub rejects an event join. */ }
  }

  async stop(): Promise<void> {
    await this.notificationConnection?.stop();
    await this.eventConnection?.stop();
    this.notificationConnection = undefined;
    this.eventConnection = undefined;
    this.latestNotification.set(null);
    this.latestSeatUpdate.set(null);
    this.connected.set(false);
  }
}
