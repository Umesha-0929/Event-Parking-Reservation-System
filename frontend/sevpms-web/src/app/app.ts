import { Component, effect, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NotificationCenterService } from './core/services/notification-center.service';
import { SessionService } from './core/services/session.service';
import { NotificationToastComponent } from './shared/components/notification-toast/notification-toast';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NotificationToastComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  private readonly session = inject(SessionService);
  private readonly notifications = inject(NotificationCenterService);

  constructor() {
    effect(() => {
      if (this.session.hasSession()) void this.notifications.start();
      else void this.notifications.stop();
    });
  }
}
