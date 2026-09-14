import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { NotificationDto } from '../../core/models/api.models';

@Component({
  selector: 'app-notification-item',
  standalone: true,
  imports: [DatePipe],
  template: `
    <article class="nv-card p-4 flex gap-4" [style.opacity]="notification.isRead ? .72 : 1">
      <button type="button" class="w-11 h-11 shrink-0 rounded-2xl grid place-items-center font-black" style="background:var(--surface-2)" (click)="markRead.emit(notification)" [attr.aria-label]="notification.isRead?'Notification read':'Mark notification as read'">{{icon}}</button>
      <div class="flex-1 min-w-0">
        <div class="flex justify-between gap-3">
          <h2 class="font-extrabold">{{notification.title}}</h2>
          <time class="text-xs nv-muted whitespace-nowrap">{{notification.createdAtUtc|date:'MMM d · h:mm a'}}</time>
        </div>
        <p class="nv-muted text-sm mt-1">{{notification.message}}</p>
        <div class="flex flex-wrap gap-2 mt-3">
          <span class="nv-status">{{category}}</span>
          @if(!notification.isRead){<button type="button" class="text-xs font-bold" (click)="markRead.emit(notification)">Mark as read</button>}
          <button type="button" class="text-xs nv-muted" (click)="removed.emit(notification)">Delete</button>
        </div>
      </div>
    </article>
  `
})
export class NotificationItemComponent {
  @Input({ required: true }) notification!: NotificationDto;
  @Input() category = 'System';
  @Input() icon = '●';
  @Output() markRead = new EventEmitter<NotificationDto>();
  @Output() removed = new EventEmitter<NotificationDto>();
}
