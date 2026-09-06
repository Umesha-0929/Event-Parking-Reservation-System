import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NotificationItem } from '../../core/models/api.models';
import { NotificationCenterService } from '../../core/services/notification-center.service';
import { httpErrorMessage } from '../../core/utils/http-error';

@Component({
  selector: 'app-notifications',
  imports: [FormsModule],
  templateUrl: './notifications.html',
  styleUrl: './notifications.scss',
})
export class NotificationsComponent implements OnInit {
  readonly center = inject(NotificationCenterService);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly busyId = signal('');
  query = '';
  state: 'All' | 'Unread' | 'Read' = 'All';

  readonly filtered = computed(() => {
    const query = this.query.trim().toLowerCase();
    return this.center.items().filter((item) => {
      const stateMatches = this.state === 'All' || (this.state === 'Unread' ? !item.isRead : !!item.isRead);
      const text = `${item.title ?? ''} ${item.message ?? ''} ${item.type ?? ''}`.toLowerCase();
      return stateMatches && (!query || text.includes(query));
    });
  });

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    this.center.refresh().subscribe({
      next: () => this.loading.set(false),
      error: (error) => { this.loading.set(false); this.error.set(httpErrorMessage(error, 'Notifications could not be loaded.')); },
    });
  }

  id(item: NotificationItem): string { return item.notificationId ?? item.id ?? ''; }
  markRead(item: NotificationItem): void {
    const id = this.id(item);
    if (!id || item.isRead || this.busyId()) return;
    this.busyId.set(id);
    this.center.markRead(id).subscribe({
      next: () => this.busyId.set(''),
      error: (error) => { this.busyId.set(''); this.error.set(httpErrorMessage(error, 'The notification could not be marked as read.')); },
    });
  }
  when(value?: string): string {
    if (!value) return '';
    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? value : date.toLocaleString();
  }
}
