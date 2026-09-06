import { isPlatformBrowser } from '@angular/common';
import { Injectable, PLATFORM_ID, computed, effect, inject, signal } from '@angular/core';
import { Observable, firstValueFrom, map, tap } from 'rxjs';
import { NotificationItem } from '../models/api.models';
import { DomainApiService } from './domain-api.service';
import { RealtimeService } from './realtime.service';
import { SessionService } from './session.service';

@Injectable({ providedIn: 'root' })
export class NotificationCenterService {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly domain = inject(DomainApiService);
  private readonly realtime = inject(RealtimeService);
  private readonly session = inject(SessionService);

  private readonly itemsSignal = signal<NotificationItem[]>([]);
  private readonly toastSignal = signal<NotificationItem | null>(null);
  private started = false;
  private toastTimer?: number;

  readonly items = this.itemsSignal.asReadonly();
  readonly toast = this.toastSignal.asReadonly();
  readonly unreadCount = computed(() => this.itemsSignal().filter((item) => !item.isRead).length);

  constructor() {
    effect(() => {
      const payload = this.realtime.latestNotification();
      if (!payload || !this.session.hasSession()) return;
      const item = this.normalize(payload);
      if (!item) return;
      this.upsert(item);
      this.showToast(item);
    });
  }

  async start(): Promise<void> {
    if (!isPlatformBrowser(this.platformId) || this.started || !this.session.hasSession()) return;
    this.started = true;
    try { await firstValueFrom(this.refresh()); } catch { /* Keep the app usable while offline. */ }
    await this.realtime.startNotifications();
  }

  async stop(): Promise<void> {
    this.started = false;
    this.itemsSignal.set([]);
    this.dismissToast();
    await this.realtime.stop();
  }

  refresh(): Observable<NotificationItem[]> {
    return this.domain.notifications().pipe(
      map((items) => items.map((item) => this.normalize(item)).filter((item): item is NotificationItem => !!item)),
      tap((items) => this.itemsSignal.set(this.sort(items))),
    );
  }

  markRead(notificationId: string): Observable<unknown> {
    return this.domain.markNotificationRead(notificationId).pipe(
      tap(() => this.itemsSignal.update((items) => items.map((item) =>
        this.idOf(item) === notificationId ? { ...item, isRead: true } : item))),
    );
  }

  dismissToast(): void {
    if (this.toastTimer && typeof window !== 'undefined') window.clearTimeout(this.toastTimer);
    this.toastTimer = undefined;
    this.toastSignal.set(null);
  }

  private normalize(value: unknown): NotificationItem | null {
    if (!value || typeof value !== 'object') return null;
    const raw = value as Record<string, unknown>;
    const notificationId = String(raw['notificationId'] ?? raw['id'] ?? '');
    if (!notificationId) return null;
    return {
      notificationId,
      id: notificationId,
      userId: raw['userId'] ? String(raw['userId']) : undefined,
      title: raw['title'] ? String(raw['title']) : 'Nvent update',
      message: raw['message'] ? String(raw['message']) : '',
      type: raw['type'] ? String(raw['type']) : 'General',
      isRead: Boolean(raw['isRead']),
      createdAtUtc: raw['createdAtUtc'] ? String(raw['createdAtUtc']) : new Date().toISOString(),
    };
  }

  private upsert(item: NotificationItem): void {
    const id = this.idOf(item);
    this.itemsSignal.update((items) => this.sort([item, ...items.filter((existing) => this.idOf(existing) !== id)]));
  }

  private sort(items: NotificationItem[]): NotificationItem[] {
    return [...items].sort((a, b) =>
      new Date(b.createdAtUtc ?? 0).getTime() - new Date(a.createdAtUtc ?? 0).getTime());
  }

  private idOf(item: NotificationItem): string {
    return item.notificationId ?? item.id ?? '';
  }

  private showToast(item: NotificationItem): void {
    if (!isPlatformBrowser(this.platformId)) return;
    this.toastSignal.set(item);
    if (this.toastTimer) window.clearTimeout(this.toastTimer);
    this.toastTimer = window.setTimeout(() => this.toastSignal.set(null), 6500);
  }
}
