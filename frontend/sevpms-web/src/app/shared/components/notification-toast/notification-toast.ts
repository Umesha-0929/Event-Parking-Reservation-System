import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NotificationCenterService } from '../../../core/services/notification-center.service';
import { SessionService } from '../../../core/services/session.service';

@Component({
  selector: 'app-notification-toast',
  imports: [RouterLink],
  templateUrl: './notification-toast.html',
  styleUrl: './notification-toast.scss',
})
export class NotificationToastComponent {
  readonly center = inject(NotificationCenterService);
  readonly session = inject(SessionService);
}
